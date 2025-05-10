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
                    // Create a smoother animation sequence
                    await heartToAnimate.ScaleTo(1.2, 200, Easing.CubicOut); // Gentle pulse outward
                    await Task.WhenAll(
                        heartToAnimate.ScaleTo(0.8, 400, Easing.CubicInOut), // Shrink while fading
                        heartToAnimate.FadeTo(0, 600, Easing.CubicIn)        // Longer, smoother fade out
                    );
                    heartToAnimate.IsVisible = false;
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
        heart.IsVisible = true;
        heart.Scale = 0.8;           // Start slightly smaller
        heart.Opacity = 0;           // Start fully transparent
        
        // Smooth fade in with scale
        await Task.WhenAll(
            heart.ScaleTo(1.0, 400, Easing.CubicOut),     // Scale up smoothly
            heart.FadeTo(1, 500, Easing.CubicOut)         // Fade in smoothly
        );
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

    private void OnDrop(object sender, DropEventArgs e)
    {
        // Reset drop zone appearance
        AnswerDropZone.BackgroundColor = new Color(27, 59, 111); // #1B3B6F

        if (e.Data.Properties.TryGetValue("Answer", out var answer) && answer is string answerText)
        {
            // Execute the command immediately
            MainThread.BeginInvokeOnMainThread(() => {
                _viewModel.CheckAnswerCommand.Execute(answerText);
            });
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        // Unsubscribe from events
        _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
    }
} 