using EducGramming.ViewModels;

namespace EducGramming.Views;

public partial class ChangePasswordPage : ContentPage
{
    public ChangePasswordPage()
    {
        InitializeComponent();
        BindingContext = new ChangePasswordViewModel();
    }
} 