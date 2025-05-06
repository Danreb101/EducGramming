using EducGramming.Services;
using System.Diagnostics;

namespace EducGramming;

public partial class App : Application
{
    private FirebaseAuthService _authService;

    public App(FirebaseAuthService authService)
    {
        InitializeComponent();
        _authService = authService;

        // Set up global exception handling
        SetupExceptionHandling();
        
        // Initialize app with appropriate shell
        InitializeApp();
    }
    
    private void SetupExceptionHandling()
    {
        TaskScheduler.UnobservedTaskException += (sender, e) =>
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                Debug.WriteLine($"UNHANDLED EXCEPTION: {e.Exception?.Message}");
                if (Current?.MainPage != null)
                {
                    await Current.MainPage.DisplayAlert("Error", 
                        "An unexpected error occurred. Please try again.", "OK");
                }
            });
            e.SetObserved();
        };
    }

    private void InitializeApp()
    {
        try
        {
            Debug.WriteLine("=== App Initialization Started ===");
            
            // Always start with the login shell for simplicity and reliability
            MainPage = new LoginShell();
            
            // Just to be safe, clean up any old tokens if not authenticated
            if (!_authService.IsSignedIn)
            {
                Debug.WriteLine("User not signed in, clearing any old tokens");
                Preferences.Default.Remove("FirebaseToken");
                Preferences.Default.Remove("CurrentEmail");
                Preferences.Default.Remove("StaySignedIn");
            }
            
            Debug.WriteLine("=== App Initialization Complete ===");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error initializing app: {ex.Message}");
            Debug.WriteLine(ex.StackTrace);
            
            // Ensure we at least have a MainPage
            MainPage = new LoginShell();
        }
    }
}
