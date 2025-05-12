using EducGramming.ViewModels;
using System.Diagnostics;

namespace EducGramming.Views;

[QueryProperty(nameof(ViewModel), "ViewModel")]
[QueryProperty(nameof(Language), "language")]
public partial class LessonMenuPage : ContentPage, IQueryAttributable
{
    private LessonViewModel _viewModel;
    private string _language;
    private bool _isAnimating;
    
    public string Language
    {
        get => _language;
        set
        {
            _language = value;
            UpdateLanguageSelection();
        }
    }
    
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
        _viewModel = new LessonViewModel();
        BindingContext = _viewModel;
        
        // Subscribe to navigation events
        Shell.Current.Navigating += Current_Navigating;
    }
    
    private void Current_Navigating(object sender, ShellNavigatingEventArgs e)
    {
        // Save state before navigation
        if (e.Source == ShellNavigationSource.Pop || e.Source == ShellNavigationSource.PopToRoot)
        {
            SaveCurrentState();
        }
    }
    
    private void SaveCurrentState()
    {
        try
        {
            if (_viewModel != null)
            {
                Preferences.Default.Set($"LastLanguage_{_viewModel.SelectedLanguage}", _viewModel.SelectedLanguage);
                Preferences.Default.Set($"LastLesson_{_viewModel.SelectedLanguage}", _viewModel.CurrentLessonIndex);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error saving state: {ex.Message}");
        }
    }
    
    private void RestoreState()
    {
        try
        {
            if (_viewModel != null && !string.IsNullOrEmpty(_viewModel.SelectedLanguage))
            {
                int lastLesson = Preferences.Default.Get($"LastLesson_{_viewModel.SelectedLanguage}", 0);
                _viewModel.CurrentLessonIndex = lastLesson;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error restoring state: {ex.Message}");
        }
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        foreach (var pair in query)
        {
            if (pair.Key == "language")
            {
                Language = pair.Value?.ToString();
            }
            else if (pair.Key == "ViewModel")
            {
                ViewModel = pair.Value as LessonViewModel;
            }
        }
    }
    
    private void UpdateLanguageSelection()
    {
        if (string.IsNullOrEmpty(_language)) return;
        
        try
        {
            _viewModel.SelectedLanguage = _language.ToLower() == "csharp" ? "C#" : "Java";
            RestoreState();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error updating language: {ex.Message}");
        }
    }

    private async void OnBackButtonClicked(object sender, EventArgs e)
    {
        if (_isAnimating) return;
        
        try
        {
            SaveCurrentState();
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Navigation error: {ex.Message}");
        }
    }

    private async void OnMenuItemTapped(object sender, TappedEventArgs e)
    {
        if (_isAnimating || sender is not Grid menuItem) return;
        
        _isAnimating = true;
        
        try
        {
            // Scale down and fade animation
            await Task.WhenAll(
                menuItem.ScaleTo(0.95, 50, Easing.CubicOut),
                menuItem.FadeTo(0.8, 50, Easing.CubicOut)
            );

            // Scale up and restore opacity animation
            await Task.WhenAll(
                menuItem.ScaleTo(1, 100, Easing.CubicIn),
                menuItem.FadeTo(1, 100, Easing.CubicIn)
            );
            
            // Handle navigation or action based on the menu item
            if (_viewModel != null)
            {
                await _viewModel.HandleMenuItemSelection(menuItem.ClassId);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Menu item interaction error: {ex.Message}");
        }
        finally
        {
            _isAnimating = false;
        }
    }
    
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        SaveCurrentState();
        Shell.Current.Navigating -= Current_Navigating;
    }
} 