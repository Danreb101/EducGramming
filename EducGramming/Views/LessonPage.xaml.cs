using Microsoft.Maui.Controls;
using EducGramming.ViewModels;
using System.ComponentModel;
using System.Threading.Tasks;

namespace EducGramming.Views;

public partial class LessonPage : ContentPage, IQueryAttributable
{
    private readonly LessonViewModel _viewModel;
    private bool _isAnimating;
    private const double SIDEBAR_WIDTH = 260;
    private const uint ANIMATION_DURATION = 250;

    public LessonPage(LessonViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
        SidebarOverlay.IsVisible = false;
    }

    private void OnHamburgerMenuClicked(object sender, EventArgs e)
    {
        SidebarOverlay.IsVisible = !SidebarOverlay.IsVisible;
        SidebarOverlay.Opacity = 1;
        SidebarPanel.TranslationX = 0;
    }

    private void OnPageSizeChanged(object sender, EventArgs e)
    {
        // Ensure sidebar position is maintained when screen size changes
        if (!_viewModel.IsSidebarVisible)
        {
            SidebarPanel.TranslationX = -SIDEBAR_WIDTH;
        }
    }

    private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(LessonViewModel.IsSidebarVisible))
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await AnimateSidebarAsync(_viewModel.IsSidebarVisible);
            });
        }
    }

    private async Task AnimateSidebarAsync(bool show)
    {
        if (_isAnimating) return;

        try
        {
            _isAnimating = true;

            if (show && !SidebarOverlay.IsVisible)
            {
                // Show sidebar
                SidebarOverlay.IsVisible = true;
                SidebarOverlay.Opacity = 0;
                SidebarPanel.TranslationX = -SIDEBAR_WIDTH;

                var showAnimation = new Animation();
                showAnimation.Add(0, 1, new Animation(v => SidebarPanel.TranslationX = v, -SIDEBAR_WIDTH, 0, Easing.CubicOut));
                showAnimation.Add(0, 1, new Animation(v => SidebarOverlay.Opacity = v, 0, 1, Easing.CubicOut));

                showAnimation.Commit(this, "ShowSidebar", 16, ANIMATION_DURATION);
                await Task.Delay((int)ANIMATION_DURATION);

                // Ensure correct section is expanded
                if (_viewModel.SelectedLesson != null)
                {
                    foreach (var section in _viewModel.LessonSections)
                    {
                        section.IsExpanded = section.Lessons.Contains(_viewModel.SelectedLesson);
                    }
                }
            }
            else if (!show && SidebarOverlay.IsVisible)
            {
                // Hide sidebar
                var hideAnimation = new Animation();
                hideAnimation.Add(0, 1, new Animation(v => SidebarPanel.TranslationX = v, 0, -SIDEBAR_WIDTH, Easing.CubicIn));
                hideAnimation.Add(0, 1, new Animation(v => SidebarOverlay.Opacity = v, 1, 0, Easing.CubicIn));

                hideAnimation.Commit(this, "HideSidebar", 16, ANIMATION_DURATION);
                await Task.Delay((int)ANIMATION_DURATION);

                SidebarOverlay.IsVisible = false;
            }
        }
        finally
        {
            _isAnimating = false;
        }
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.ContainsKey("language"))
        {
            string language = query["language"].ToString();
            _viewModel.SelectedLanguage = language.ToLower() == "csharp" ? "C#" : "Java";

            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (!SidebarOverlay.IsVisible)
                {
                    _viewModel.IsSidebarVisible = true;
                }
            });
        }
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        
        if (_viewModel.SelectedLesson != null && !SidebarOverlay.IsVisible)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                _viewModel.IsSidebarVisible = true;
            });
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        
        this.AbortAnimation("ShowSidebar");
        this.AbortAnimation("HideSidebar");
        
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        }
        
        SizeChanged -= OnPageSizeChanged;
    }
}  