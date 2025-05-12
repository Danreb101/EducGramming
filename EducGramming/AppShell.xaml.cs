using Microsoft.Maui.Controls;
using System.Diagnostics;

namespace EducGramming;

public partial class AppShell : Shell
{
    private bool _isInitialized = false;
    
    public AppShell()
    {
        Debug.WriteLine("======= INITIALIZING APPSHELL =======");
        
        try
        {
            InitializeComponent();
            RegisterRoutes();
            InitializeNavigation();
            
            Debug.WriteLine("AppShell initialized successfully");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error initializing AppShell: {ex.Message}");
            Debug.WriteLine($"Stack trace: {ex.StackTrace}");
        }
        
        Debug.WriteLine("======= APPSHELL INITIALIZATION COMPLETE =======");
    }
    
    private async void InitializeNavigation()
    {
        if (_isInitialized) return;
        
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            try
            {
                await Task.Delay(150); // Increased delay for UI readiness
                
                if (Items?.Count > 0)
                {
                    CurrentItem = Items[0];
                    
                    if (CurrentItem is TabBar tabBar && tabBar.Items?.Count > 0)
                    {
                        tabBar.CurrentItem = tabBar.Items[0];
                        Debug.WriteLine($"Selected tab: {tabBar.CurrentItem?.Title ?? "unknown"}");
                    }
                }
                
                _isInitialized = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Navigation initialization error: {ex.Message}");
            }
        });
    }
    
    protected override void OnNavigating(ShellNavigatingEventArgs args)
    {
        base.OnNavigating(args);
        
        // Prevent navigation while initialization is in progress
        if (!_isInitialized && args.Source != ShellNavigationSource.ShellSectionChanged)
        {
            args.Cancel();
            return;
        }
        
        Debug.WriteLine($"Navigating to: {args.Target?.Location?.OriginalString ?? "unknown"}");
    }
    
    private void RegisterRoutes()
    {
        Debug.WriteLine("Registering navigation routes");
        
        // Register authentication routes
        Routing.RegisterRoute("login", typeof(Views.LoginPage));
        Routing.RegisterRoute("register", typeof(Views.RegisterPage));
        Routing.RegisterRoute("resetpassword", typeof(Views.ResetPasswordPage));
        Routing.RegisterRoute("changepassword", typeof(Views.ChangePasswordPage));
        
        // Register lesson routes
        Routing.RegisterRoute("lessonmenu", typeof(Views.LessonMenuPage));
        Routing.RegisterRoute("lesson", typeof(Views.LessonPage));
        
        // Register other main routes
        Routing.RegisterRoute("play", typeof(Views.PlayPage));
        Routing.RegisterRoute("leaderboard", typeof(Views.LeaderboardPage));
        Routing.RegisterRoute("profile", typeof(Views.ProfilePage));
        
        Debug.WriteLine("Routes registered successfully");
    }
}
