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
        // Try to get the full name first, then username, then fall back to email, and finally "Guest"
        var fullName = Preferences.Default.Get("CurrentFullName", "");
        if (!string.IsNullOrEmpty(fullName))
            return fullName;

        var username = Preferences.Default.Get("CurrentUsername", "");
        if (!string.IsNullOrEmpty(username))
            return username;

        var email = Preferences.Default.Get("CurrentEmail", "");
        if (!string.IsNullOrEmpty(email))
            return email.Split('@')[0];

        return "Guest";
    }
} 