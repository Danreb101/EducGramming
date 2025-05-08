using Microsoft.Maui.Controls;
using EducGramming.ViewModels;
using System.ComponentModel;

namespace EducGramming.Views;

public partial class PlayPage : ContentPage
{
    private readonly PlayViewModel _viewModel;
    private int _previousLives = 3;

    public PlayPage(PlayViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;

        // Set up drop zone
        var dropGesture = new DropGestureRecognizer();
        dropGesture.DragOver += OnDragOver;
        dropGesture.Drop += OnDrop;
        dropGesture.DragLeave += OnDragLeave;
        AnswerDropZone.GestureRecognizers.Add(dropGesture);

        // Subscribe to Lives property changes
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    private async void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(_viewModel.Lives))
        {
            var currentLives = _viewModel.Lives;
            if (currentLives < _previousLives)
            {
                // Determine which heart to animate
                Label heartToAnimate = null;
                switch (_previousLives)
                {
                    case 3: heartToAnimate = Heart3; break;
                    case 2: heartToAnimate = Heart2; break;
                    case 1: heartToAnimate = Heart1; break;
                }

                if (heartToAnimate != null)
                {
                    // Start the animation before the heart disappears
                    await Task.WhenAll(
                        heartToAnimate.ScaleTo(1.5, 100),
                        heartToAnimate.FadeTo(0, 300)
                    );
                }
            }
            else if (currentLives > _previousLives)
            {
                // Reset heart visibility and opacity when lives are restored
                switch (currentLives)
                {
                    case 3:
                        await ResetHeart(Heart3);
                        await ResetHeart(Heart2);
                        await ResetHeart(Heart1);
                        break;
                    case 2:
                        await ResetHeart(Heart2);
                        await ResetHeart(Heart1);
                        break;
                    case 1:
                        await ResetHeart(Heart1);
                        break;
                }
            }
            _previousLives = currentLives;
        }
    }

    private async Task ResetHeart(Label heart)
    {
        heart.Scale = 1.0;
        heart.Opacity = 1.0;
        await heart.FadeTo(1, 300);
    }

    private void OnDragStarting(object sender, DragStartingEventArgs e)
    {
        if (sender is Element element && element.BindingContext is string answer)
        {
            e.Data.Properties.Add("Answer", answer);
        }
    }

    private void OnDragOver(object sender, DragEventArgs e)
    {
        // Visual feedback when dragging over the drop zone
        AnswerDropZone.BackgroundColor = new Color(43, 75, 143); // #2B4B8F
    }

    private void OnDragLeave(object sender, DragEventArgs e)
    {
        // Reset drop zone appearance
        AnswerDropZone.BackgroundColor = new Color(27, 59, 111); // #1B3B6F
    }

    private async void OnDrop(object sender, DropEventArgs e)
    {
        // Reset drop zone appearance
        AnswerDropZone.BackgroundColor = new Color(27, 59, 111); // #1B3B6F

        if (e.Data.Properties.TryGetValue("Answer", out var answer) && answer is string answerText)
        {
            // Execute the command
            _viewModel.CheckAnswerCommand.Execute(answerText);
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        // Unsubscribe from events
        _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
    }
} 