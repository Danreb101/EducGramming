using Microsoft.Maui.Controls;
using EducGramming.ViewModels;

namespace EducGramming.Views
{
    public partial class LoginPage : ContentPage
    {
        private readonly LoginViewModel _viewModel;

        public LoginPage()
        {
            InitializeComponent();
            _viewModel = new LoginViewModel();
            BindingContext = _viewModel;

            // Subscribe to events
            _viewModel.ShowTermsRequested += OnShowTermsRequested;
            if (TermsPopup != null)
            {
                TermsPopup.CloseClicked += OnTermsPopupClosed;
            }
        }

        private void OnShowTermsRequested(object sender, EventArgs e)
        {
            PopupLayer.IsVisible = true;
            // Add fade in animation
            PopupLayer.Opacity = 0;
            PopupLayer.FadeTo(1, 250, Easing.CubicOut);
        }

        private async void OnTermsPopupClosed(object sender, EventArgs e)
        {
            // Add fade out animation
            await PopupLayer.FadeTo(0, 250, Easing.CubicIn);
            PopupLayer.IsVisible = false;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            
            // Unsubscribe from events
            _viewModel.ShowTermsRequested -= OnShowTermsRequested;
            if (TermsPopup != null)
            {
                TermsPopup.CloseClicked -= OnTermsPopupClosed;
            }
        }
    }
} 