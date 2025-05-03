using Microsoft.Maui.Controls;

namespace EducGramming;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        
        // Register routes for auth flow
        Routing.RegisterRoute("login", typeof(Views.LoginPage));
        Routing.RegisterRoute("register", typeof(Views.RegisterPage));
        Routing.RegisterRoute("resetpassword", typeof(Views.ResetPasswordPage));
    }
}
