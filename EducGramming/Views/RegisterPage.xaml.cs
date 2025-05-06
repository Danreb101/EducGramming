using Microsoft.Maui.Controls;
using EducGramming.ViewModels;
using EducGramming.Services;
using System.Diagnostics;

namespace EducGramming.Views
{
    public partial class RegisterPage : ContentPage
    {
        private RegisterViewModel _viewModel;
        private FirebaseAuthService _authService;

        // Constructor for dependency injection (used by DI container)
        public RegisterPage(RegisterViewModel viewModel)
        {
            try 
            {
                InitializeComponent();
                _viewModel = viewModel;
                BindingContext = _viewModel;
                _authService = new FirebaseAuthService();

                // Start animations when page appears
                this.Loaded += OnPageLoaded;

                // Subscribe to the ShowTerms event from ViewModel
                _viewModel.ShowTermsRequested += OnShowTermsRequested;

                // Subscribe to terms popup events
                if (TermsPopup != null)
                {
                    TermsPopup.CloseClicked += OnTermsPopupClosed;
                }
                
                Debug.WriteLine("RegisterPage initialized successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error initializing RegisterPage: {ex.Message}");
            }
        }
        
        // Default constructor (used when directly creating a page)
        public RegisterPage()
        {
            try 
            {
                InitializeComponent();
                _viewModel = new RegisterViewModel();
                BindingContext = _viewModel;
                _authService = new FirebaseAuthService();

                // Start animations when page appears
                this.Loaded += OnPageLoaded;

                // Subscribe to the ShowTerms event from ViewModel
                _viewModel.ShowTermsRequested += OnShowTermsRequested;

                // Subscribe to terms popup events
                if (TermsPopup != null)
                {
                    TermsPopup.CloseClicked += OnTermsPopupClosed;
                }
                
                Debug.WriteLine("RegisterPage initialized successfully (default constructor)");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error initializing RegisterPage: {ex.Message}");
            }
        }

        private void OnShowTermsRequested(object sender, EventArgs e)
        {
            // Show the popup layer with animation
            PopupLayer.IsVisible = true;
            PopupLayer.Opacity = 0;
            PopupLayer.FadeTo(1, 250, Easing.CubicOut);
        }

        private async void OnTermsPopupClosed(object sender, EventArgs e)
        {
            // When the user clicks "I Understand", we consider it as accepting the terms
            _viewModel.HandleTermsAcceptance(true);

            // Hide the popup with animation
            await PopupLayer.FadeTo(0, 250, Easing.CubicOut);
            PopupLayer.IsVisible = false;

            // Show a confirmation message
            await DisplayAlert("Terms Accepted", "You have accepted the Terms and Conditions.", "OK");
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            
            // Unsubscribe from events
            if (_viewModel != null)
            {
                _viewModel.ShowTermsRequested -= OnShowTermsRequested;
            }

            if (TermsPopup != null)
            {
                TermsPopup.CloseClicked -= OnTermsPopupClosed;
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

        private async void OnRegisterButtonClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                button.IsEnabled = false;
            }

            try
            {
                // Validate fields
                if (string.IsNullOrWhiteSpace(_viewModel.Email) || 
                    string.IsNullOrWhiteSpace(_viewModel.Password) || 
                    string.IsNullOrWhiteSpace(_viewModel.FullName))
                {
                    await DisplayAlert("Error", "Please fill in all fields", "OK");
                    return;
                }

                if (!_viewModel.IsTermsAccepted)
                {
                    await DisplayAlert("Error", "Please accept the Terms and Conditions", "OK");
                    return;
                }

                bool success = await _viewModel.RegisterUserAsync();
                if (success)
                {
                    await DisplayAlert("Success", "Registration successful! Please login with your credentials.", "OK");
                    await Shell.Current.GoToAsync("//login");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
            finally
            {
                if (button != null)
                {
                    button.IsEnabled = true;
                }
            }
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//login");
        }
    }
} 