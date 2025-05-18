using Microsoft.Maui.Controls;
using EducGramming.ViewModels;
using EducGramming.Services;
using System.Diagnostics;

namespace EducGramming.Views
{
    public partial class ResetPasswordPage : ContentPage
    {
        private FirebaseAuthService _authService;

        public ResetPasswordPage()
        {
            try
            {
                InitializeComponent();
                BindingContext = new ResetPasswordViewModel();
                _authService = new FirebaseAuthService();

                // Start animations when page appears
                this.Loaded += OnPageLoaded;
                Debug.WriteLine("ResetPasswordPage initialized successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error initializing ResetPasswordPage: {ex.Message}");
            }
        }

        private async void OnPageLoaded(object sender, EventArgs e)
        {
            // Reset initial states
            MainContent.Opacity = 0;
            LogoContainer.TranslationY = 50;
            FormCard.TranslationY = 50;
            FormCard.Opacity = 0;
            LoginLink.Opacity = 0;

            // Animate main content
            await MainContent.FadeTo(1, 300, Easing.CubicOut);

            // Animate logo container
            await Task.WhenAll(
                LogoContainer.TranslateTo(0, 0, 500, Easing.CubicOut),
                LogoContainer.FadeTo(1, 500, Easing.CubicOut)
            );

            // Animate form card
            await Task.WhenAll(
                FormCard.TranslateTo(0, 0, 500, Easing.CubicOut),
                FormCard.FadeTo(1, 500, Easing.CubicOut)
            );

            // Animate login link
            await LoginLink.FadeTo(1, 300, Easing.CubicOut);
        }

        private async void OnResetPasswordClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(emailEntry.Text))
            {
                // Shake animation for validation error
                await resetButton.TranslateTo(-5, 0, 50);
                await resetButton.TranslateTo(5, 0, 50);
                await resetButton.TranslateTo(0, 0, 50);

                await DisplayAlert("Error", "Please enter your email", "OK");
                return;
            }

            try
            {
                resetButton.IsEnabled = false;
                await resetButton.ScaleTo(0.95, 100, Easing.CubicInOut);

                // Send password reset email
                await _authService.SendPasswordResetEmailAsync(emailEntry.Text);

                // Simulate reset delay
                await Task.Delay(1000);

                // Scale back up
                await resetButton.ScaleTo(1, 100, Easing.CubicInOut);

                // Show success message
                await DisplayAlert("Success", "Password reset email has been sent to your email address. Please check your inbox to complete the password reset process.", "OK");

                // Fade out animation before navigation
                await MainContent.FadeTo(0, 200, Easing.CubicIn);

                // Navigate to login page
                if (Shell.Current is LoginShell)
                {
                    // Navigate within LoginShell
                    await Shell.Current.GoToAsync("//login");
                }
                else
                {
                    // Switch to LoginShell and set it as main page
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        Application.Current.MainPage = new LoginShell();
                    });
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "Password reset failed: " + ex.Message, "OK");
            }
            finally
            {
                resetButton.IsEnabled = true;
            }
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            // Fade out animation
            await MainContent.FadeTo(0, 200, Easing.CubicIn);

            // Check if we need to navigate within the current shell or switch shells
            if (Shell.Current is LoginShell)
            {
                // Navigate within LoginShell
                await Shell.Current.GoToAsync("//login");
            }
            else
            {
                // Switch to LoginShell and set it as main page
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Application.Current.MainPage = new LoginShell();
                });
            }
        }
    }
} 