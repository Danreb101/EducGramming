using System.Text.Json;
using Microsoft.Maui.Storage;
using System.Diagnostics;
using Firebase.Auth;

namespace EducGramming.Services
{
    public class UserService
    {
        private const string UsersKey = "RegisteredUsers";
        private FirebaseAuthService _firebaseAuthService;

        public class UserData
        {
            public string Email { get; set; }
            public string Password { get; set; }
            public string Username { get; set; }
            public string FullName { get; set; }
        }

        public UserService()
        {
            _firebaseAuthService = new FirebaseAuthService();
        }

        public async Task RegisterUser(string email, string password, string username, string fullName)
        {
            try
            {
                // Check if user already exists locally
                var users = await GetAllUsers();
                if (users.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
                {
                    throw new Exception("A user with this email already exists.");
                }

                // Register with Firebase first
                try
                {
                    Debug.WriteLine($"Registering user with Firebase: {email}");
                    var firebaseUser = await _firebaseAuthService.SignUp(email, password);
                    Debug.WriteLine($"Successfully registered with Firebase: {firebaseUser.User.Email}");
                }
                catch (FirebaseAuthException firebaseEx)
                {
                    Debug.WriteLine($"Firebase registration error: {firebaseEx.Message}");
                    throw new Exception($"Firebase registration failed: {firebaseEx.Message}");
                }

                // Add user to local storage for additional profile data
                users.Add(new UserData 
                { 
                    Email = email,
                    Password = password, // In a real app, this should be hashed
                    Username = username, // This is now derived from email
                    FullName = fullName
                });

                // Save updated users list
                await SaveUsers(users);
                Debug.WriteLine($"Successfully registered user: {email} with username: {username}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error registering user: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> ValidateLogin(string email, string password)
        {
            try
            {
                // Ensure Firebase auth service exists
                if (_firebaseAuthService == null)
                {
                    _firebaseAuthService = new FirebaseAuthService();
                }

                // Direct Firebase authentication
                var firebaseUser = await _firebaseAuthService.SignIn(email, password);
                
                // Store minimal info
                Preferences.Default.Set("CurrentEmail", email);
                
                // Get user profile in background
                _ = Task.Run(async () => {
                    var users = await GetAllUsers();
                    var user = users.FirstOrDefault(u => 
                        u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
                        
                    if (user != null)
                    {
                        Preferences.Default.Set("CurrentUsername", user.Username);
                        Preferences.Default.Set("CurrentFullName", user.FullName);
                    }
                });

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        // Helper method to add user to local storage
        private async Task AddUserToLocalStorage(string email, string password, string username, string fullName)
        {
            var users = await GetAllUsers();
            
            // Add the user if not already in local storage
            if (!users.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
            {
                users.Add(new UserData 
                { 
                    Email = email,
                    Password = password,
                    Username = username,
                    FullName = fullName
                });
                
                await SaveUsers(users);
                Debug.WriteLine($"Added user to local storage: {email}");
            }
        }

        public async Task<List<UserData>> GetAllUsers()
        {
            try
            {
                var usersJson = Preferences.Default.Get<string>(UsersKey, null);
                Debug.WriteLine($"Retrieved users JSON: {(usersJson != null ? "data found" : "no data")}");

                if (string.IsNullOrEmpty(usersJson))
                {
                    return new List<UserData>();
                }

                var users = JsonSerializer.Deserialize<List<UserData>>(usersJson);
                return users ?? new List<UserData>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting users: {ex.Message}");
                return new List<UserData>();
            }
        }

        private async Task SaveUsers(List<UserData> users)
        {
            try
            {
                var usersJson = JsonSerializer.Serialize(users);
                Preferences.Default.Set(UsersKey, usersJson);
                Debug.WriteLine("Successfully saved users to preferences");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving users: {ex.Message}");
                throw new Exception("Failed to save user data", ex);
            }
        }

        // Add method to migrate existing users to Firebase Authentication
        public async Task MigrateExistingUsersToFirebase()
        {
            try
            {
                Debug.WriteLine("Migrating existing users to Firebase Authentication...");
                
                // Get all users from local storage
                var users = await GetAllUsers();
                
                Debug.WriteLine($"Found {users.Count} users in local storage to check for Firebase migration");
                
                foreach (var user in users)
                {
                    try
                    {
                        // Check if email is the current logged in user
                        bool isCurrentUser = user.Email == Preferences.Default.Get<string>("CurrentEmail", "");
                        
                        if (isCurrentUser)
                        {
                            Debug.WriteLine($"Checking Firebase registration for current user: {user.Email}");
                            
                            // Ensure this user exists in Firebase
                            bool ensureResult = await _firebaseAuthService.EnsureUserExistsInFirebase(user.Email, user.Password);
                            
                            if (ensureResult)
                            {
                                Debug.WriteLine($"✓ Successfully verified/created user in Firebase: {user.Email}");
                                
                                // Skip showing the confirmation popup to the user
                                Debug.WriteLine("Skipping account creation notification");
                            }
                            else
                            {
                                Debug.WriteLine($"⚠ Failed to verify/create user in Firebase: {user.Email}");
                            }
                        }
                    }
                    catch (Exception userEx)
                    {
                        Debug.WriteLine($"Error migrating user {user.Email}: {userEx.Message}");
                        // Continue with the next user
                    }
                }
                
                Debug.WriteLine("User migration check complete");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in MigrateExistingUsersToFirebase: {ex.Message}");
            }
        }

        public async Task ChangePassword(string email, string oldPassword, string newPassword)
        {
            try
            {
                // First verify the old password is correct by signing in
                try
                {
                    // Try to sign in with Firebase using the old password
                    Debug.WriteLine($"Verifying old password for {email}");
                    var firebaseUser = await _firebaseAuthService.SignIn(email, oldPassword);
                    Debug.WriteLine("Old password verified successfully");
                }
                catch (FirebaseAuthException)
                {
                    // Verify with local storage as fallback
                    var localUsers = await GetAllUsers();
                    var localUser = localUsers.FirstOrDefault(u => 
                        u.Email.Equals(email, StringComparison.OrdinalIgnoreCase) &&
                        u.Password.Equals(oldPassword));
                    
                    if (localUser == null)
                    {
                        throw new Exception("Invalid old password.");
                    }
                }

                // Send password reset email through Firebase
                await _firebaseAuthService.SendPasswordResetEmailAsync(email);
                
                // Update password in local storage in the meantime
                var allUsers = await GetAllUsers();
                var userToUpdate = allUsers.FirstOrDefault(u => 
                    u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

                if (userToUpdate != null)
                {
                    // Update password locally
                    userToUpdate.Password = newPassword;
                    await SaveUsers(allUsers);
                }
                
                Debug.WriteLine($"Password reset email sent for user: {email}");
                
                // Inform the user
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Password Reset",
                        "A password reset email has been sent to your email address. Please check your inbox to complete the password change process.",
                        "OK");
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error changing password: {ex.Message}");
                throw;
            }
        }

        public async Task<UserData> GetCurrentUserProfile()
        {
            try
            {
                Debug.WriteLine("Getting current user profile");
                // Get the current email from preferences
                string currentEmail = Preferences.Default.Get<string>("CurrentEmail", null);
                
                if (string.IsNullOrEmpty(currentEmail))
                {
                    Debug.WriteLine("No current user email found in preferences");
                    return null;
                }
                
                // Get all users
                var users = await GetAllUsers();
                
                // Find the current user
                var user = users.FirstOrDefault(u => u.Email.Equals(currentEmail, StringComparison.OrdinalIgnoreCase));
                
                if (user == null)
                {
                    Debug.WriteLine($"User with email {currentEmail} not found in local storage");
                    
                    // Create a basic profile based on preferences if not found in storage
                    user = new UserData
                    {
                        Email = currentEmail,
                        Username = Preferences.Default.Get<string>("CurrentUsername", currentEmail.Split('@')[0]),
                        FullName = Preferences.Default.Get<string>("CurrentFullName", currentEmail.Split('@')[0]),
                        Password = "********" // Never return actual password
                    };
                }
                else
                {
                    Debug.WriteLine($"Found user profile for {currentEmail}");
                    // Don't return the actual password
                    user.Password = "********";
                }
                
                return user;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting current user profile: {ex.Message}");
                return null;
            }
        }

        // Save the current user's profile data
        public async Task UpdateCurrentUserProfile(string fullName, string username)
        {
            try
            {
                string currentEmail = Preferences.Default.Get<string>("CurrentEmail", null);
                
                if (string.IsNullOrEmpty(currentEmail))
                {
                    throw new Exception("Not logged in");
                }
                
                Debug.WriteLine($"Updating profile for {currentEmail} - FullName: {fullName}, Username: {username}");
                
                // Get all users
                var users = await GetAllUsers();
                
                // Find the current user
                var user = users.FirstOrDefault(u => u.Email.Equals(currentEmail, StringComparison.OrdinalIgnoreCase));
                
                if (user != null)
                {
                    // Update existing user
                    user.FullName = fullName;
                    user.Username = username;
                    
                    // Save all users
                    await SaveUsers(users);
                    
                    // Update preferences
                    Preferences.Default.Set("CurrentUsername", username);
                    Preferences.Default.Set("CurrentFullName", fullName);
                    
                    Debug.WriteLine($"Updated profile for {currentEmail}: Username={username}, FullName={fullName}");
                }
                else
                {
                    // Get the current password if available (or use a placeholder)
                    string password = "placeholder-password";
                    
                    // Add new user entry
                    await AddUserToLocalStorage(currentEmail, password, username, fullName);
                    
                    // Update preferences
                    Preferences.Default.Set("CurrentUsername", username);
                    Preferences.Default.Set("CurrentFullName", fullName);
                    
                    Debug.WriteLine($"Created new profile for {currentEmail}: Username={username}, FullName={fullName}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating user profile: {ex.Message}");
                throw;
            }
        }
    }
} 