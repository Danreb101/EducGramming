using Microsoft.Maui.Controls;
using EducGramming.Views;

namespace EducGramming;

public partial class LoginShell : Shell
{
    public LoginShell()
    {
        InitializeComponent();
        
        // Register routes for navigation
        Routing.RegisterRoute("login", typeof(LoginPage));
        Routing.RegisterRoute("register", typeof(RegisterPage));
        Routing.RegisterRoute("resetpassword", typeof(ResetPasswordPage));
        Routing.RegisterRoute("changepassword", typeof(ChangePasswordPage));
    }
} 