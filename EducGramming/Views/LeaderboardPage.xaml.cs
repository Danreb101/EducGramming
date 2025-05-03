using EducGramming.ViewModels;

namespace EducGramming.Views;

public partial class LeaderboardPage : ContentPage
{
    private readonly LeaderboardViewModel _viewModel;

    public LeaderboardPage(LeaderboardViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadLeaderboardAsync();
    }
} 