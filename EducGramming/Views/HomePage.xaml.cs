using EducGramming.ViewModels;
using EducGramming.Services;
using Microsoft.Maui.Controls;

namespace EducGramming.Views;

public partial class HomePage : ContentPage
{
    public string WelcomeMessage => $"Welcome back, {GetUsername()}!";
    public string SubtitleMessage => "Your personal programming learning companion";

    public HomePage()
    {
        InitializeComponent();
        BindingContext = this;
    }

    private string GetUsername()
    {
        var email = Preferences.Default.Get("CurrentEmail", "");
        return string.IsNullOrEmpty(email) ? "Guest" : email.Split('@')[0];
    }
} 