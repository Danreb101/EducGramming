using Microsoft.Maui.Controls;

namespace EducGramming.Views
{
    public partial class TermsAndConditionsPopup : ContentView
    {
        public event EventHandler CloseClicked;

        public TermsAndConditionsPopup()
        {
            InitializeComponent();
        }

        private void OnCloseButtonClicked(object sender, EventArgs e)
        {
            CloseClicked?.Invoke(this, EventArgs.Empty);
        }
    }
} 