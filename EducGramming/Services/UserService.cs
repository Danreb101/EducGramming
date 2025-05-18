using System.Text.Json;
using Microsoft.Maui.Storage;
using System.Diagnostics;
using Firebase.Auth;

namespace EducGramming.Services
{
    public class UserProfile
    {
        public string Email { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
    }

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

        public async Task RegisterUser(string email, string password, string fullName)
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

                    // Store user info in preferences immediately
                    Preferences.Default.Set("CurrentEmail", email);
                    Preferences.Default.Set("CurrentFullName", fullName);
                    Preferences.Default.Set("CurrentUsername", email);
                }
                catch (FirebaseAuthException firebaseEx)
                {
                    Debug.WriteLine($"Firebase registration error: {firebaseEx.Message}");
                    throw new Exception($"Firebase registration failed: {firebaseEx.Message}");
                }

                // Add user to local storage for profile data
                users.Add(new UserData 
                { 
                    Email = email,
                    Password = password,
                    FullName = fullName,
                    Username = email // Use email as username
                });

                // Save updated users list
                await SaveUsers(users);
                Debug.WriteLine($"Successfully registered user: {email} with full name: {fullName}");
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

                // Get all users to check if email exists
                var users = await GetAllUsers();
                var user = users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

                if (user == null)
                {
                    Debug.WriteLine($"Login failed: No account found with email {email}");
                    throw new Exception("No account found with this email address.");
                }

                try
                {
                    // Attempt Firebase authentication
                    var firebaseUser = await _firebaseAuthService.SignIn(email, password);
                    
                    // If we get here, authentication was successful
                    // Store complete user info in preferences
                    Preferences.Default.Set("CurrentEmail", email);
                    Preferences.Default.Set("CurrentFullName", user.FullName);
                    Preferences.Default.Set("CurrentUsername", user.Username ?? email);
                    Debug.WriteLine($"Login successful: {email}, FullName={user.FullName}");
                    
                    return true;
                }
                catch (FirebaseAuthException firebaseEx)
                {
                    Debug.WriteLine($"Firebase auth failed: {firebaseEx.Message}");
                    throw new Exception("Incorrect password. Please try again.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Login validation failed: {ex.Message}");
                throw; // Rethrow the exception to be handled by the UI
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
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(oldPassword) || string.IsNullOrWhiteSpace(newPassword))
                {
                    throw new Exception("Please fill in all fields.");
                }

                if (newPassword.Length < 6)
                {
                    throw new Exception("New password must be at least 6 characters long.");
                }

                try
                {
                    // Change password in Firebase
                    await _firebaseAuthService.ChangePasswordAsync(email, oldPassword, newPassword);
                    Debug.WriteLine("Password changed successfully in Firebase");

                    // Update password in local storage
                    var allUsers = await GetAllUsers();
                    var userToUpdate = allUsers.FirstOrDefault(u => 
                        u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

                    if (userToUpdate != null)
                    {
                        userToUpdate.Password = newPassword;
                        await SaveUsers(allUsers);
                        Debug.WriteLine("Password updated in local storage");

                        // Send custom email notification
                        var emailService = new EmailService();
                        await emailService.SendPasswordChangedEmailAsync(email, userToUpdate.FullName ?? email);
                    }

                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        await Application.Current.MainPage.DisplayAlert(
                            "Success",
                            "Your password has been changed successfully.",
                            "OK");
                    });
                }
                catch (FirebaseAuthException authEx)
                {
                    Debug.WriteLine($"Firebase auth error: {authEx.Message}");
                    throw new Exception("Invalid current password. Please try again.");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error during password change: {ex.Message}");
                    throw new Exception("Failed to change password. Please try again later.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in ChangePassword: {ex.Message}");
                throw;
            }
        }

        public async Task<UserProfile> GetCurrentUserProfile()
        {
            try
            {
                // For now, just return data from preferences
                return new UserProfile
                {
                    Email = Preferences.Default.Get<string>("CurrentEmail", ""),
                    Username = Preferences.Default.Get<string>("CurrentUsername", ""),
                    FullName = Preferences.Default.Get<string>("CurrentFullName", "")
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting user profile: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> UpdateUserProfile(UserProfile profile)
        {
            try
            {
                // Save to preferences
                Preferences.Default.Set("CurrentEmail", profile.Email);
                Preferences.Default.Set("CurrentUsername", profile.Username);
                Preferences.Default.Set("CurrentFullName", profile.FullName);

                // Here you would typically also update the profile in your backend
                // For now, we'll just save to preferences
                
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating user profile: {ex.Message}");
                return false;
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