using EducGramming.ViewModels;
using System.Diagnostics;

namespace EducGramming.Views;

[QueryProperty(nameof(ViewModel), "ViewModel")]
public partial class LessonMenuPage : ContentPage
{
    private LessonViewModel _viewModel;
    public LessonViewModel ViewModel
    {
        get => _viewModel;
        set
        {
            _viewModel = value;
            BindingContext = _viewModel;
        }
    }

    public LessonMenuPage()
    {
        InitializeComponent();
    }

    private async void OnBackButtonClicked(object sender, EventArgs e)
    {
        try
        {
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Navigation error: {ex.Message}");
        }
    }

    private async void OnMenuItemTapped(object sender, TappedEventArgs e)
    {
        if (sender is Grid menuItem)
        {
            // Scale down and fade animation
            await menuItem.ScaleTo(0.95, 50, Easing.CubicOut);
            await menuItem.FadeTo(0.8, 50, Easing.CubicOut);

            // Scale up and restore opacity animation
            await Task.WhenAll(
                menuItem.ScaleTo(1, 100, Easing.CubicIn),
                menuItem.FadeTo(1, 100, Easing.CubicIn)
            );
        }
    }
} 