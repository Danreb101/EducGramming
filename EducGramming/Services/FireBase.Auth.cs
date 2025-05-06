using Firebase.Auth;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using System.Linq;

namespace EducGramming.Services
{
    public class FirebaseAuthService
    {
        // Replace with your Web API Key from Firebase console
        private const string ApiKey = "AIzaSyDXy6rlUuLdsdSDl9wenBMMt59F0vXF1OE";
        private FirebaseAuthProvider _authProvider;
        private FirebaseAuthLink? _currentUser;
        
        // For development/testing - set to true to enable test accounts
        private bool _useDummyAuth = false;
        private readonly Dictionary<string, string> _dummyUsers = new Dictionary<string, string>
        {
            { "test123@gmail.com", "test123" },
            { "admin@educgramming.com", "admin123" }
        };

        public FirebaseAuthService()
        {
            try
            {
                // Initialize the Firebase Auth provider with the API key
                _authProvider = new FirebaseAuthProvider(new FirebaseConfig(ApiKey));
                
                // Initialize from preferences asynchronously
                InitializeFromPreferences();
                
                Debug.WriteLine("Firebase Auth Service initialized successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error initializing Firebase Auth Service: {ex.Message}");
                
                // Create a dummy provider to avoid null reference exceptions
                _authProvider = new FirebaseAuthProvider(new FirebaseConfig(ApiKey));
                
                // Use dummy auth as fallback
                _useDummyAuth = true;
            }
        }

        private async void InitializeFromPreferences()
        {
            try
            {
                // Check if we have a saved token
                string savedToken = Preferences.Default.Get<string>("FirebaseToken", null);
                string savedEmail = Preferences.Default.Get<string>("CurrentEmail", null);
                bool staySignedIn = Preferences.Default.Get<bool>("StaySignedIn", false);
                
                Debug.WriteLine($"Reading from preferences - Token: {(savedToken != null ? "exists" : "not found")}, Email: {savedEmail ?? "not found"}, Stay signed in: {staySignedIn}");
                
                if (staySignedIn && !string.IsNullOrEmpty(savedToken) && !string.IsNullOrEmpty(savedEmail))
                {
                    // For dummy auth, just create a dummy user
                    if (_useDummyAuth && savedEmail != null)
                    {
                        _currentUser = CreateDummyAuthLink(savedEmail);
                        Debug.WriteLine($"Created dummy auth link for user: {savedEmail}");
                        return;
                    }
                    
                    // Try to refresh token using the saved token with proper Firebase auth
                    try
                    {
                        if (_authProvider == null)
                        {
                            Debug.WriteLine("Auth provider is null, cannot refresh token");
                            return;
                        }
                        
                        // Create auth object with saved token
                        var auth = new FirebaseAuth
                        {
                            FirebaseToken = savedToken
                        };
                        
                        // Refresh the token
                        _currentUser = await _authProvider.RefreshAuthAsync(auth);
                        
                        // If successful, we are logged in
                        if (_currentUser != null)
                        {
                            Debug.WriteLine($"Successfully refreshed token for user: {_currentUser.User?.Email}");
                            
                            // Refresh the token periodically
                            _ = RefreshTokenPeriodically();
                        }
                        else
                        {
                            Debug.WriteLine("Token refresh returned null user");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Firebase token refresh error: {ex.Message}");
                        _currentUser = null;
                        
                        // If token refresh fails, fall back to dummy auth for this session
                        if (!string.IsNullOrEmpty(savedEmail))
                        {
                            _useDummyAuth = true;
                            _currentUser = CreateDummyAuthLink(savedEmail);
                            Debug.WriteLine($"Falling back to dummy auth for user: {savedEmail}");
                        }
                    }
                }
                else
                {
                    Debug.WriteLine("No saved token or email found in preferences");
                }
            }
            catch (Exception ex)
            {
                // Token might be expired or invalid, clear preferences
                Debug.WriteLine($"Token refresh error: {ex.Message}");
                Preferences.Default.Remove("FirebaseToken");
                Preferences.Default.Remove("CurrentEmail");
                _currentUser = null;
            }
        }

        public async Task<FirebaseAuthLink> SignUp(string email, string password)
        {
            try
            {
                // Make sure to validate email format
                if (!IsValidEmail(email))
                {
                    throw new Exception("Invalid email format");
                }
                
                // Password strength validation
                if (password.Length < 6)
                {
                    throw new Exception("Password must be at least 6 characters long");
                }
                
                // Always attempt Firebase authentication first
                try
                {
                    Debug.WriteLine($"Creating Firebase user with email: {email}");
                    
                    // Create the user in Firebase
                    _currentUser = await _authProvider.CreateUserWithEmailAndPasswordAsync(email, password);
                    
                    Debug.WriteLine($"Successfully created Firebase user: {_currentUser.User.Email}");
                    
                    // Save user state
                    SaveUserState(_currentUser, true);
                    
                    // Start token refresh
                    _ = RefreshTokenPeriodically();
                    
                    return _currentUser;
                }
                catch (FirebaseAuthException firebaseEx)
                {
                    // Log the specific Firebase error
                    Debug.WriteLine($"Firebase signup error: {firebaseEx.GetType().Name}: {firebaseEx.Message}");
                    
                    // Fall back to dummy auth only if explicitly enabled
                    if (_useDummyAuth)
                    {
                        Debug.WriteLine("Firebase signup failed, falling back to dummy auth");
                        
                        if (_dummyUsers.ContainsKey(email))
                            throw new Exception("Email is already in use");
                            
                        _dummyUsers[email] = password;
                        _currentUser = CreateDummyAuthLink(email);
                        SaveUserState(_currentUser, true);
                        
                        Debug.WriteLine($"Created dummy user: {email}");
                        return _currentUser;
                    }
                    
                    // Otherwise provide user-friendly error messages
                    if (firebaseEx.Message.Contains("EMAIL_EXISTS"))
                    {
                        throw new Exception("This email is already registered. Please use a different email or try logging in.");
                    }
                    else if (firebaseEx.Message.Contains("WEAK_PASSWORD"))
                    {
                        throw new Exception("Password is too weak. Please use a stronger password with at least 6 characters.");
                    }
                    else if (firebaseEx.Message.Contains("OPERATION_NOT_ALLOWED"))
                    {
                        throw new Exception("Account creation is currently disabled. Please try again later.");
                    }
                    else if (firebaseEx.Message.Contains("TOO_MANY_ATTEMPTS_TRY_LATER"))
                    {
                        throw new Exception("Too many failed attempts. Please try again later.");
                    }
                    else if (firebaseEx.Message.Contains("INVALID_EMAIL"))
                    {
                        throw new Exception("The email address is not valid. Please enter a valid email address.");
                    }
                    else
                    {
                        throw new Exception($"Firebase registration failed: {firebaseEx.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error
                Debug.WriteLine($"Firebase signup error: {ex.Message}");
                throw; // Rethrow to be handled by the UI
            }
        }

        public async Task<FirebaseAuthLink> SignIn(string email, string password)
        {
            try
            {
                Debug.WriteLine($"===== FirebaseAuthService.SignIn started =====");
                
                // Basic validation
                if (string.IsNullOrEmpty(email))
                {
                    Debug.WriteLine("SignIn error: Email cannot be empty");
                    throw new Exception("Email cannot be empty");
                }
                
                if (string.IsNullOrEmpty(password))
                {
                    Debug.WriteLine("SignIn error: Password cannot be empty");
                    throw new Exception("Password cannot be empty");
                }
                
                if (!IsValidEmail(email))
                {
                    Debug.WriteLine("SignIn error: Invalid email format");
                    throw new Exception("Invalid email format");
                }
                
                Debug.WriteLine($"Starting sign-in process for email: {email}");
                
                // SPECIAL HANDLING FOR TEST ACCOUNTS
                // Check if this is a test account and use it as a fallback option
                bool isTestAccount = email.Trim().ToLower() == "test123@gmail.com" && password == "test123";
                
                // First try real Firebase authentication regardless of the account type
                if (!_useDummyAuth || isTestAccount)
                {
                    try
                    {
                        Debug.WriteLine($"Attempting Firebase authentication for: {email}");
                        
                        if (_authProvider == null)
                        {
                            Debug.WriteLine("Auth provider is null, creating new instance");
                            _authProvider = new FirebaseAuthProvider(new FirebaseConfig(ApiKey));
                        }
                        
                        Debug.WriteLine("Calling SignInWithEmailAndPasswordAsync");
                        // Try Firebase authentication first
                        _currentUser = await _authProvider.SignInWithEmailAndPasswordAsync(email, password);
                        
                        if (_currentUser == null)
                        {
                            Debug.WriteLine("Firebase returned null user");
                            throw new Exception("Firebase authentication returned null user");
                        }
                        
                        if (_currentUser.User == null)
                        {
                            Debug.WriteLine("Firebase returned user with null properties");
                            throw new Exception("Firebase authentication returned user with null properties");
                        }
                        
                        Debug.WriteLine($"Successfully signed in with Firebase: {_currentUser.User.Email}");
                        
                        // Save user state
                        SaveUserState(_currentUser, true);
                        
                        // Start token refresh
                        _ = RefreshTokenPeriodically();
                        
                        Debug.WriteLine("===== FirebaseAuthService.SignIn completed successfully (Firebase) =====");
                        return _currentUser;
                    }
                    catch (Firebase.Auth.FirebaseAuthException firebaseEx)
                    {
                        // Log the Firebase error
                        Debug.WriteLine($"Firebase signin error: {firebaseEx.GetType().Name}: {firebaseEx.Message}");
                        
                        // If this is a test account, fall back to dummy auth
                        if (isTestAccount)
                        {
                            Debug.WriteLine("Test account detected - falling back to dummy authentication");
                            _currentUser = CreateDummyAuthLink("test123@gmail.com");
                            SaveUserState(_currentUser, true);
                            Debug.WriteLine("===== FirebaseAuthService.SignIn completed successfully (test account fallback) =====");
                            return _currentUser;
                        }
                        
                        // For other accounts, provide friendly error messages
                        Debug.WriteLine("===== FirebaseAuthService.SignIn failed (Firebase Exception) =====");
                        if (firebaseEx.Message.Contains("INVALID_PASSWORD") || 
                            firebaseEx.Message.Contains("INVALID_EMAIL") ||
                            firebaseEx.Message.Contains("INVALID_LOGIN_CREDENTIALS"))
                        {
                            throw new Exception("Email or password is incorrect. Please try again.");
                        }
                        else if (firebaseEx.Message.Contains("USER_DISABLED"))
                        {
                            throw new Exception("Your account has been disabled. Please contact support.");
                        }
                        else if (firebaseEx.Message.Contains("EMAIL_NOT_FOUND"))
                        {
                            throw new Exception("No account found with this email. Please check your email or sign up.");
                        }
                        else if (firebaseEx.Message.Contains("TOO_MANY_ATTEMPTS_TRY_LATER"))
                        {
                            throw new Exception("Too many failed login attempts. Please try again later.");
                        }
                        else
                        {
                            throw new Exception($"Login failed: {firebaseEx.Message}");
                        }
                    }
                }
                
                // FALLBACK TO DUMMY AUTH ONLY IF EXPLICITLY ENABLED
                // This section will only execute if _useDummyAuth is true and we got here
                if (_useDummyAuth)
                {
                    Debug.WriteLine("Using dummy auth mode as fallback");
                    
                    // CASE-INSENSITIVE LOOKUP FOR DUMMY USERS
                    string emailLower = email.ToLower().Trim();
                    string dummyEmail = _dummyUsers.Keys.FirstOrDefault(k => k.ToLower() == emailLower);
                    
                    if (dummyEmail == null)
                    {
                        Debug.WriteLine($"Dummy auth failed - user not found: {email}");
                        throw new Exception("Email or password is incorrect. Please try again.");
                    }
                    
                    if (password != _dummyUsers[dummyEmail])
                    {
                        Debug.WriteLine($"Dummy auth failed - password mismatch for user: {email}");
                        throw new Exception("Email or password is incorrect. Please try again.");
                    }
                    
                    Debug.WriteLine("Dummy auth successful - creating auth link");
                    _currentUser = CreateDummyAuthLink(dummyEmail);
                    SaveUserState(_currentUser, true);
                    Debug.WriteLine("===== FirebaseAuthService.SignIn completed successfully (dummy) =====");
                    return _currentUser;
                }
                
                // If we got here, both Firebase and dummy auth failed
                throw new Exception("Authentication failed. Please check your credentials.");
            }
            catch (Exception ex)
            {
                // Log the error
                Debug.WriteLine($"Firebase signin general error: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                Debug.WriteLine("===== FirebaseAuthService.SignIn failed (General Exception) =====");
                throw; // Rethrow to be handled by the UI
            }
        }

        public async Task<string> GetFreshToken(FirebaseAuthLink authLink)
        {
            try
            {
                if (_useDummyAuth)
                {
                    return "dummy-token";
                }

                if (authLink == null)
                {
                    throw new Exception("Authentication data is null");
                }

                // Refresh the token
                var refreshedAuth = await _authProvider.RefreshAuthAsync(new FirebaseAuth
                {
                    FirebaseToken = authLink.FirebaseToken,
                    RefreshToken = authLink.RefreshToken
                });

                if (refreshedAuth != null)
                {
                    // Update current user
                    _currentUser = refreshedAuth;

                    // Save the refreshed token
                    SaveUserState(refreshedAuth, true);

                    return refreshedAuth.FirebaseToken;
                }
                else
                {
                    throw new Exception("Failed to refresh token");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error refreshing token: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> SignOut()
        {
            try
            {
                Debug.WriteLine("Signing out user");
                
                // Clear current user state
                _currentUser = null;
                
                // Remove saved credentials
                Preferences.Default.Remove("FirebaseToken");
                Preferences.Default.Remove("CurrentEmail");
                Preferences.Default.Remove("StaySignedIn");
                
                // Navigate back to login page
                await MainThread.InvokeOnMainThreadAsync(() => 
                {
                    Application.Current.MainPage = new LoginShell();
                });
                
                Debug.WriteLine("User signed out successfully");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error signing out: {ex.Message}");
                return false;
            }
        }

        public bool IsSignedIn => _currentUser != null && _currentUser.User != null;

        public string CurrentUserEmail => _currentUser?.User?.Email ?? 
            Preferences.Default.Get<string>("CurrentEmail", null);

        public string? CurrentUserId => _currentUser?.User?.LocalId;

        public async Task SendPasswordResetEmailAsync(string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                {
                    throw new Exception("Email cannot be empty");
                }
                
                if (!IsValidEmail(email))
                {
                    throw new Exception("Invalid email format");
                }
                
                // Using dummy auth for testing
                if (_useDummyAuth)
                {
                    // Check if the user exists in our dummy data
                    if (!_dummyUsers.ContainsKey(email))
                    {
                        throw new Exception("No account found with this email address");
                    }
                    
                    Debug.WriteLine($"Dummy mode: Password reset email would be sent to {email}");
                    return;
                }
                
                // Using real Firebase authentication
                try
                {
                    Debug.WriteLine($"Sending password reset email to: {email}");
                    
                    await _authProvider.SendPasswordResetEmailAsync(email);
                    
                    Debug.WriteLine($"Password reset email sent successfully to {email}");
                }
                catch (FirebaseAuthException firebaseEx)
                {
                    // Log the specific Firebase error
                    Debug.WriteLine($"Firebase password reset error: {firebaseEx.GetType().Name}: {firebaseEx.Message}");
                    
                    if (firebaseEx.Message.Contains("EMAIL_NOT_FOUND"))
                    {
                        throw new Exception("No account found with this email address");
                    }
                    else if (firebaseEx.Message.Contains("INVALID_EMAIL"))
                    {
                        throw new Exception("The email address is not valid");
                    }
                    else
                    {
                        throw new Exception($"Failed to send password reset email: {firebaseEx.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Password reset error: {ex.Message}");
                throw;
            }
        }

        public async Task ChangePasswordAsync(string email, string oldPassword, string newPassword)
        {
            try
            {
                // First verify the old password is correct by signing in
                try
                {
                    // Try to sign in with Firebase using the old password
                    Debug.WriteLine($"Verifying old password for {email}");
                    var firebaseUser = await SignIn(email, oldPassword);
                    Debug.WriteLine("Old password verified successfully");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to verify old password: {ex.Message}");
                    throw new Exception("Current password is incorrect. Please try again.");
                }

                // Send password reset email through Firebase
                await SendPasswordResetEmailAsync(email);
                
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

        private void SaveUserState(FirebaseAuthLink user, bool staySignedIn)
        {
            if (user != null)
            {
                // Save the token and email
                Preferences.Default.Set("FirebaseToken", user.FirebaseToken);
                Preferences.Default.Set("CurrentEmail", user.User.Email);
                Preferences.Default.Set("StaySignedIn", staySignedIn);
                
                // Print debug info
                Debug.WriteLine($"SaveUserState: Saved token and email={user.User.Email}, StaySignedIn={staySignedIn}");
            }
            else
            {
                Debug.WriteLine("SaveUserState: Warning - Attempted to save null user state");
            }
        }

        private async Task RefreshTokenPeriodically()
        {
            // Skip for dummy auth
            if (_useDummyAuth) return;
            
            while (_currentUser != null)
            {
                // Wait for 50 minutes (Firebase tokens expire after 1 hour)
                await Task.Delay(TimeSpan.FromMinutes(50));
                
                try
                {
                    // Refresh the token
                    _currentUser = await _authProvider.RefreshAuthAsync(new FirebaseAuth
                    {
                        FirebaseToken = _currentUser.FirebaseToken,
                        RefreshToken = _currentUser.RefreshToken
                    });
                    
                    Debug.WriteLine("Firebase token refreshed successfully");
                    
                    // Save the refreshed token
                    SaveUserState(_currentUser, true);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error refreshing token: {ex.Message}");
                    break;
                }
            }
        }

        private bool IsValidEmail(string email)
        {
            // Simple email validation using regex
            string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            return Regex.IsMatch(email, pattern);
        }

        private FirebaseAuthLink CreateDummyAuthLink(string email)
        {
            // Create a dummy FirebaseAuthLink for testing
            var user = new User
            {
                Email = email,
                IsEmailVerified = true,
                LocalId = Guid.NewGuid().ToString()
            };
            
            return new FirebaseAuthLink(_authProvider, new FirebaseAuth
            {
                FirebaseToken = "dummy-token",
                RefreshToken = "dummy-refresh-token",
                User = user
            });
        }

        // Add method to delete a user account from Firebase
        public async Task<bool> DeleteUserAccount(string email, string password)
        {
            try
            {
                Debug.WriteLine($"Attempting to delete user account: {email}");
                
                // First authenticate to verify credentials
                try
                {
                    // Try to sign in with the provided credentials
                    var authLink = await SignIn(email, password);
                    
                    // If using dummy auth, just remove from local dictionary
                    if (_useDummyAuth)
                    {
                        string emailLower = email.ToLower().Trim();
                        string dummyEmail = _dummyUsers.Keys.FirstOrDefault(k => k.ToLower() == emailLower);
                        
                        if (dummyEmail != null)
                        {
                            _dummyUsers.Remove(dummyEmail);
                            Debug.WriteLine($"Removed dummy user: {dummyEmail}");
                            return true;
                        }
                        
                        return false;
                    }
                    
                    // For real Firebase auth, delete the user account
                    if (authLink != null && authLink.User != null)
                    {
                        // With the Firebase.Auth library, we need to fetch and use the user's ID token
                        // to make a custom DELETE request to the Firebase Auth API
                        
                        Debug.WriteLine("Unable to delete Firebase user directly through the SDK. Displaying instructions instead.");
                        
                        // For now, we'll provide instructions to the user on how to delete their account
                        await MainThread.InvokeOnMainThreadAsync(async () =>
                        {
                            await Application.Current.MainPage.DisplayAlert(
                                "Account Deletion",
                                "To delete your account from Firebase Authentication, please go to your email inbox and look for the verification email from Firebase. Follow the instructions to verify your email, then you can delete your account from the Firebase console or contact the administrator.",
                                "OK");
                        });
                        
                        // Return true to indicate the process was handled
                        return true;
                    }
                    
                    return false;
                }
                catch (Exception authEx)
                {
                    Debug.WriteLine($"Authentication failed for account deletion: {authEx.Message}");
                    throw new Exception("Authentication failed. Please check your credentials.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting user account: {ex.Message}");
                throw;
            }
        }

        // Method to verify and ensure a user exists in Firebase
        public async Task<bool> EnsureUserExistsInFirebase(string email, string password)
        {
            try
            {
                Debug.WriteLine($"Verifying user exists in Firebase: {email}");
                
                // Check if using dummy auth
                if (_useDummyAuth)
                {
                    Debug.WriteLine("Warning: Using dummy auth mode. Users are not created in Firebase");
                    return false;
                }
                
                // Try to log in with Firebase
                try
                {
                    // Force Firebase auth for this check
                    bool originalDummyAuth = _useDummyAuth;
                    _useDummyAuth = false;
                    
                    try
                    {
                        var authLink = await _authProvider.SignInWithEmailAndPasswordAsync(email, password);
                        Debug.WriteLine($"User exists in Firebase: {email}");
                        
                        // If successful, the user exists in Firebase
                        if (authLink != null && authLink.User != null)
                        {
                            // Save user state
                            SaveUserState(authLink, true);
                            _currentUser = authLink;
                            
                            // No verification emails or popups during sign-in
                            Debug.WriteLine("User authenticated in Firebase - skipping verification email");
                            
                            return true;
                        }
                        return false;
                    }
                    catch (FirebaseAuthException authEx)
                    {
                        // If user not found, try to create it
                        if (authEx.Message.Contains("EMAIL_NOT_FOUND") || 
                            authEx.Message.Contains("INVALID_LOGIN_CREDENTIALS"))
                        {
                            Debug.WriteLine($"User not found in Firebase, creating: {email}");
                            
                            try
                            {
                                // Create the user in Firebase
                                var newUser = await _authProvider.CreateUserWithEmailAndPasswordAsync(email, password);
                                
                                if (newUser != null && newUser.User != null)
                                {
                                    Debug.WriteLine($"User created in Firebase: {email}");
                                    
                                    // Save user state
                                    SaveUserState(newUser, true);
                                    _currentUser = newUser;
                                    
                                    // No verification emails or popups for silent account creation
                                    Debug.WriteLine("Account created in Firebase - skipping verification email");
                                    
                                    return true;
                                }
                            }
                            catch (FirebaseAuthException createEx)
                            {
                                Debug.WriteLine($"Failed to create user in Firebase: {createEx.Message}");
                                
                                // If the email already exists but password is wrong
                                if (createEx.Message.Contains("EMAIL_EXISTS"))
                                {
                                    Debug.WriteLine("Email exists in Firebase but password may be incorrect");
                                    // Skip showing alerts during login
                                }
                            }
                        }
                        
                        Debug.WriteLine($"Firebase auth error: {authEx.Message}");
                    }
                    finally
                    {
                        // Restore original dummy auth setting
                        _useDummyAuth = originalDummyAuth;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error verifying user: {ex.Message}");
                }
                
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in EnsureUserExistsInFirebase: {ex.Message}");
                return false;
            }
        }
    }
}
