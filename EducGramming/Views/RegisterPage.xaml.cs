using Microsoft.Maui.Controls;
using EducGramming.ViewModels;

namespace EducGramming.Views
{
    public partial class RegisterPage : ContentPage
    {
        private bool _isPasswordVisible;
        private readonly RegisterViewModel _viewModel;

        public RegisterPage(RegisterViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            _isPasswordVisible = false;
            togglePasswordImage.Source = "eye.svg";
            BindingContext = _viewModel;

            // Start animations when page appears
            this.Loaded += OnPageLoaded;
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

        private void OnTogglePasswordVisibility(object sender, EventArgs e)
        {
            _isPasswordVisible = !_isPasswordVisible;
            passwordEntry.IsPassword = !_isPasswordVisible;
            togglePasswordImage.Source = _isPasswordVisible ? "eye_off.svg" : "eye.svg";

            // Add subtle animation to the toggle
            togglePasswordImage.ScaleTo(0.8, 50, Easing.CubicInOut)
                .ContinueWith((t) => togglePasswordImage.ScaleTo(1, 50, Easing.CubicInOut));
        }

        private async void OnRegisterButtonClicked(object sender, EventArgs e)
        {
            if (!termsCheckbox.IsChecked)
            {
                // Shake animation for checkbox
                var originalColor = termsCheckbox.Color;
                termsCheckbox.Color = Colors.Red;
                await termsCheckbox.TranslateTo(-5, 0, 50);
                await termsCheckbox.TranslateTo(5, 0, 50);
                await termsCheckbox.TranslateTo(0, 0, 50);
                termsCheckbox.Color = originalColor;

                await DisplayAlert("Error", "Please accept the Terms and Conditions", "OK");
                return;
            }

            try
            {
                registerButton.IsEnabled = false;
                
                // Scale down animation
                await registerButton.ScaleTo(0.95, 100, Easing.CubicInOut);
                
                // Execute the register command
                if (_viewModel.RegisterCommand.CanExecute(null))
                {
                    _viewModel.RegisterCommand.Execute(null);
                }

                // Scale back up
                await registerButton.ScaleTo(1, 100, Easing.CubicInOut);
            }
            finally
            {
                registerButton.IsEnabled = true;
            }
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            if (_viewModel.BackToLoginCommand.CanExecute(null))
            {
                _viewModel.BackToLoginCommand.Execute(null);
            }
        }
    }
} 