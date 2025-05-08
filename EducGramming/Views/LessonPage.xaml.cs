using Microsoft.Maui.Controls;
using EducGramming.ViewModels;

namespace EducGramming.Views;

public partial class LessonPage : ContentPage
{
    private readonly LessonViewModel _viewModel;

    public LessonPage(LessonViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
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
    }
} 