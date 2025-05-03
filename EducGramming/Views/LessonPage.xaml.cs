using EducGramming.ViewModels;

namespace EducGramming.Views;

public partial class LessonPage : ContentPage
{
    public LessonPage(LessonViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
} 