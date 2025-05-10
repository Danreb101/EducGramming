using Microsoft.Maui.Controls;
using EducGramming.ViewModels;
using System.ComponentModel;
using System.Threading.Tasks;

namespace EducGramming.Views;

public partial class LessonPage : ContentPage, IQueryAttributable
{
    private readonly LessonViewModel _viewModel;

    public LessonPage(LessonViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
        _viewModel.PropertyChanged += ViewModel_PropertyChanged;
    }

    private async void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(LessonViewModel.IsSidebarVisible))
        {
            if (_viewModel.IsSidebarVisible)
            {
                // Make overlay visible first
                SidebarOverlay.IsVisible = true;
                // Ensure panel is at starting position
                SidebarPanel.TranslationX = -260;
                // Animate panel into view
                await SidebarPanel.TranslateTo(0, 0, 250, Easing.CubicOut);
            }
            else
            {
                // Animate panel out of view
                await SidebarPanel.TranslateTo(-260, 0, 250, Easing.CubicIn);
                // Hide overlay after animation
                SidebarOverlay.IsVisible = false;
            }
        }
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.ContainsKey("language"))
        {
            string language = query["language"].ToString();
            // Convert the language parameter to match the picker options
            _viewModel.SelectedLanguage = language.ToLower() == "csharp" ? "C#" : "Java";
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // You can add any initialization logic here
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        // Clean up any resources if needed
        _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
    }
} 