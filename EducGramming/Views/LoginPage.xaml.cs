using Microsoft.Maui.Controls;
using EducGramming.Services;
using System.Diagnostics;
using EducGramming.Views;
using System.Linq;

namespace EducGramming.Views
{
    public partial class LoginPage : ContentPage
    {
        private FirebaseAuthService _authService;
        private bool _isBusy = false;
        private int _tapCount = 0;
        private const string TEST_EMAIL = "test123@gmail.com";
        private const string TEST_PASSWORD = "test123";

        public LoginPage()
        {
            try
        {
            InitializeComponent();
                _authService = new FirebaseAuthService();
                
                // Add tap gesture to logo for showing test credentials
                var logoTapGesture = new TapGestureRecognizer();
                logoTapGesture.Tapped += OnLogoTapped;
                var logoImages = this.FindByName<Image>("logoImage");
                if (logoImages != null)
                {
                    logoImages.GestureRecognizers.Add(logoTapGesture);
                }
                
                Debug.WriteLine("LoginPage initialized successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error initializing LoginPage: {ex.Message}");
            }
        }
        
        private async void OnLogoTapped(object sender, EventArgs e)
        {
            _tapCount++;
            
            // After 3 taps, show test credentials
            if (_tapCount >= 3)
            {
                _tapCount = 0;
                await DisplayAlert("Test Credentials", 
                    $"For testing, you can use:\nEmail: {TEST_EMAIL}\nPassword: {TEST_PASSWORD}", 
                    "OK");
                    
                // Auto-fill the credentials
                emailEntry.Text = TEST_EMAIL;
                passwordEntry.Text = TEST_PASSWORD;
            }
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            // Prevent multiple clicks
            if (_isBusy) return;
            _isBusy = true;
            errorLabel.IsVisible = false;

            try
            {
                // Add detailed logging
                Debug.WriteLine("==== LOGIN ATTEMPT STARTED ====");
                
                // Validate inputs
                if (string.IsNullOrEmpty(emailEntry.Text) || string.IsNullOrEmpty(passwordEntry.Text))
                {
                    Debug.WriteLine("ERROR: Email or password is empty");
                    errorLabel.Text = "Please enter email and password";
                    errorLabel.IsVisible = true;
                    _isBusy = false;
                    return;
                }

                // Remove any whitespace from email
                string email = emailEntry.Text.Trim();
                string password = passwordEntry.Text;

                // Show loading indicator
                errorLabel.TextColor = Color.FromArgb("#007AFF"); // Blue color
                errorLabel.Text = $"Signing in as {email}...";
                errorLabel.IsVisible = true;

                // Validate login credentials
                var userService = new UserService();
                try 
                {
                    bool isValid = await userService.ValidateLogin(email, password);
                    
                    if (isValid)
                    {
                        // Save stay signed in preference
                        Preferences.Default.Set("StaySignedIn", staySignedInCheckbox.IsChecked);
                        
                        if (staySignedInCheckbox.IsChecked)
                        {
                            Preferences.Default.Set("LastUsedEmail", email);
                            Debug.WriteLine("Saved login preferences for stay signed in");
                        }
                        else
                        {
                            Preferences.Default.Remove("LastUsedEmail");
                            Debug.WriteLine("Cleared stay signed in preferences");
                        }
                        
                        Debug.WriteLine("Creating new AppShell instance and setting as MainPage");
                        Application.Current.MainPage = new AppShell();
                        Debug.WriteLine("==== LOGIN SUCCESSFUL - Navigated to AppShell ====");
                    }
                }
                catch (Exception authEx)
                {
                    Debug.WriteLine($"Authentication error: {authEx.Message}");
                    errorLabel.TextColor = Color.FromArgb("#FF4444"); // Red color
                    // Show a user-friendly message for incorrect password
                    if (authEx.Message.Contains("password is incorrect") || authEx.Message.Contains("INVALID_PASSWORD") || authEx.Message.Contains("Incorrect password"))
                    {
                        errorLabel.Text = "The password you entered is incorrect.";
                    }
                    else if (authEx.Message.Contains("Exception occured while authenticating"))
                    {
                        errorLabel.Text = "An error occurred while trying to sign in. Please check your credentials and try again.";
                    }
                    else
                    {
                        errorLabel.Text = authEx.Message;
                    }
                    errorLabel.IsVisible = true;
                    
                    // Clear password field on authentication error
                    passwordEntry.Text = string.Empty;
                    
                    // If no account exists, clear email field too
                    if (authEx.Message.Contains("No account found"))
                    {
                        emailEntry.Text = string.Empty;
                    }
                    
                    _isBusy = false;
                    return;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"EXCEPTION: {ex.GetType().Name}: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                
                errorLabel.TextColor = Color.FromArgb("#FF4444"); // Red color
                errorLabel.Text = "Login error: " + ex.Message;
                errorLabel.IsVisible = true;
                
                // Clear password field on any error
                passwordEntry.Text = string.Empty;
            }
            finally
            {
                _isBusy = false;
            }
        }

        private void OnTogglePasswordVisibility(object sender, EventArgs e)
        {
            // Toggle the visibility state of eye icons
            eyeIcon.IsVisible = !eyeIcon.IsVisible;
            eyeOffIcon.IsVisible = !eyeOffIcon.IsVisible;
            
            // IsPassword binding will automatically update based on eyeIcon.IsVisible
        }

        private async void OnForgotPasswordClicked(object sender, EventArgs e)
        {
            // Remove the check for empty email and always navigate to reset password page
            // Store the email for the reset password page if present
            if (!string.IsNullOrWhiteSpace(emailEntry.Text))
            {
                Preferences.Default.Set("ResetEmail", emailEntry.Text.Trim());
            }
            // Navigate to reset password page
            await Shell.Current.GoToAsync("//resetpassword");
        }

        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            // Navigate to register page
            await Shell.Current.GoToAsync("//register");
        }

        private async Task DisplaySuccessAnimation()
        {
            // Create success animation
            errorLabel.TextColor = Color.FromArgb("#28A745"); // Green color
            errorLabel.Text = "Login successful!";
            errorLabel.IsVisible = true;
            
            // Perform a quick animation - shorter delay before navigation
            await Task.WhenAll(
                errorLabel.ScaleTo(1.1, 100, Easing.SpringOut),
                errorLabel.FadeTo(0.9, 100)
            );
            
            await Task.WhenAll(
                errorLabel.ScaleTo(1.0, 100, Easing.SpringIn),
                errorLabel.FadeTo(1, 100)
            );
            
            // Very short delay before navigation
            await Task.Delay(50);
        }
    }
} 