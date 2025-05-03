namespace EducGramming;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        // Clear any existing credentials
        Preferences.Default.Clear();
        
        MainPage = new LoginShell();

        // Set up global exception handling
        TaskScheduler.UnobservedTaskException += (sender, e) =>
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Current?.MainPage?.DisplayAlert("Error", 
                    "An unexpected error occurred. Please try again.", "OK");
            });
            e.SetObserved();
        };
    }
}
