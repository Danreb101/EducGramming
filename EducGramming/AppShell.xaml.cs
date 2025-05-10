using Microsoft.Maui.Controls;
using System.Diagnostics;

namespace EducGramming;

public partial class AppShell : Shell
{
    public AppShell()
    {
        Debug.WriteLine("======= INITIALIZING APPSHELL =======");
        
        try
        {
            // Initialize the component
            InitializeComponent();
            
            // Register routes for navigation
            RegisterRoutes();
            
            // Force Home tab selection on startup - this is critical
            Debug.WriteLine("Selecting Home tab on startup");
            
            // Small delay to ensure UI is ready
            MainThread.BeginInvokeOnMainThread(async () => 
            {
                await Task.Delay(100);
                
                try
                {
                    if (Items.Count > 0)
                    {
                        Debug.WriteLine($"Shell has {Items.Count} items");
                        CurrentItem = Items[0]; // Should be the TabBar
                        
                        if (CurrentItem is TabBar tabBar)
                        {
                            Debug.WriteLine($"TabBar has {tabBar.Items.Count} tabs");
                            if (tabBar.Items.Count > 0)
                            {
                                // Force select first tab (Home)
                                tabBar.CurrentItem = tabBar.Items[0];
                                Debug.WriteLine($"Selected tab: {tabBar.CurrentItem?.Title ?? "unknown"}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error selecting Home tab: {ex.Message}");
                }
            });
            
            Debug.WriteLine("AppShell initialized successfully");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error initializing AppShell: {ex.Message}");
            Debug.WriteLine($"Stack trace: {ex.StackTrace}");
        }
        
        Debug.WriteLine("======= APPSHELL INITIALIZATION COMPLETE =======");
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
        
        // Register any other routes here
        
        Debug.WriteLine("Routes registered successfully");
    }
}
