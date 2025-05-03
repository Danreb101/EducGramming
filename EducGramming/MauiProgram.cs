using Microsoft.Extensions.Logging;
using EducGramming.Views;
using EducGramming.ViewModels;
using EducGramming.Services;
using EducGramming.Converters;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;

namespace EducGramming;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Configure handlers
        builder.ConfigureMauiHandlers(handlers =>
        {
            // Add any custom handlers here if needed
        });

        // Register services
        builder.Services.AddSingleton<ILeaderboardService, LeaderboardService>();

        // Register converters as singletons
        builder.Services.AddSingleton<BoolToColorConverter>();
        builder.Services.AddSingleton<BoolToIconConverter>();
        builder.Services.AddSingleton<InverseBoolConverter>();
        builder.Services.AddSingleton<IntToHeartVisibilityConverter>();

        // Register view models
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<RegisterViewModel>();
        builder.Services.AddTransient<ResetPasswordViewModel>();
        builder.Services.AddTransient<HomeViewModel>();
        builder.Services.AddTransient<LessonViewModel>();
        builder.Services.AddTransient<PlayViewModel>();
        builder.Services.AddTransient<ProfileViewModel>();
        builder.Services.AddTransient<LeaderboardViewModel>();

        // Register pages
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<ResetPasswordPage>();
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<LessonPage>();
        builder.Services.AddTransient<PlayPage>();
        builder.Services.AddTransient<LeaderboardPage>();
        builder.Services.AddTransient<ProfilePage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
