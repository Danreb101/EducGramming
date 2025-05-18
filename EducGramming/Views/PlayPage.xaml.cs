using Microsoft.Maui.Controls;
using EducGramming.ViewModels;
using System.ComponentModel;
using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;
using Plugin.Maui.Audio;

namespace EducGramming.Views;

public partial class PlayPage : ContentPage
{
    private readonly PlayViewModel _viewModel;
    private int _previousLives = 3;
    private readonly SemaphoreSlim _animationSemaphore = new SemaphoreSlim(1, 1);
    private bool _isAnimatingHearts = false;
    private CancellationTokenSource _animationCancellation;

    public PlayPage(IAudioManager audioManager)
    {
        InitializeComponent();
        _viewModel = new PlayViewModel(audioManager);
        BindingContext = _viewModel;
        _animationCancellation = new CancellationTokenSource();

        // Set up drop zone
        var dropGesture = new DropGestureRecognizer();
        dropGesture.DragOver += OnDragOver;
        dropGesture.Drop += OnDrop;
        dropGesture.DragLeave += OnDragLeave;
        AnswerDropZone.GestureRecognizers.Add(dropGesture);

        // Subscribe to Lives property changes
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        _viewModel.HeartFadeRequested += OnHeartFadeRequested;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        
        // Create a new cancellation token whenever the page appears
        _animationCancellation = new CancellationTokenSource();
        
        // FORCE ALL HEARTS TO BE VISIBLE ON PAGE LOAD - NO ANIMATIONS
        ForceHeartReset();
    }

    // MINECRAFT STYLE HEART DAMAGE ANIMATION - SIMPLIFIED FOR STABILITY
    private async Task AnimateHeartDamageMinecraft(Label heart, CancellationToken cancellationToken)
        {
        if (heart == null || cancellationToken.IsCancellationRequested) return;
        
        try 
        {
            // Make sure heart is visible first
            heart.IsVisible = true;
            heart.Opacity = 1;
            heart.Scale = 1;
            
            // Save original color
            var originalColor = heart.TextColor;
            
            // Simple flash effect - more stable
            heart.TextColor = Colors.White;
            await Task.Delay(100, cancellationToken);
            heart.TextColor = originalColor;
            await Task.Delay(100, cancellationToken);
            
            // Simple fade out without rotation (more stable)
            await heart.FadeTo(0, 200, Easing.CubicOut);
            await heart.ScaleTo(0, 200, Easing.CubicOut);
            
            heart.IsVisible = false;
        }
        catch (TaskCanceledException)
        {
            // Animation was cancelled, reset the heart to a stable state
            heart.Rotation = 0;
            heart.TranslationX = 0;
            heart.TranslationY = 0;
        }
        catch (Exception)
        {
            // Handle any unexpected errors, reset heart to stable state
            heart.IsVisible = false;
            heart.Opacity = 0;
            heart.Scale = 0;
            heart.Rotation = 0;
            heart.TranslationX = 0;
            heart.TranslationY = 0;
        }
    }
    
    // MINECRAFT STYLE HEART RESTORATION ANIMATION - STABLE VERSION
    private async Task AnimateHeartRestoreMinecraft(Label heart, CancellationToken cancellationToken)
    {
        if (heart == null || cancellationToken.IsCancellationRequested) return;
        
        try
        {
            // Start with heart invisible
            heart.IsVisible = true;
            heart.Scale = 0;
            heart.Opacity = 0;
            heart.Rotation = 0;
            heart.TranslationX = 0;
            heart.TranslationY = 0;
            
            // Save original color and set gold for regeneration effect (Minecraft golden hearts)
            var originalColor = heart.TextColor;
            heart.TextColor = Colors.Gold;
            
            // Minecraft-style pop-in effect
            await Task.WhenAll(
                heart.FadeTo(1, 150, Easing.CubicIn),
                heart.ScaleTo(1.2, 150, Easing.SpringOut)
            );
            
            if (cancellationToken.IsCancellationRequested) return;
            
            // Small bobbing animation (Minecraft heart bounce)
            await heart.ScaleTo(0.9, 100, Easing.SpringIn);
            if (cancellationToken.IsCancellationRequested) return;
            
            await heart.ScaleTo(1.1, 100, Easing.SpringOut);
            if (cancellationToken.IsCancellationRequested) return;
            
            await heart.ScaleTo(1.0, 100, Easing.SpringIn);
            
            // Return to original red color
            await Task.Delay(100, cancellationToken);
            heart.TextColor = originalColor;
        }
        catch (TaskCanceledException)
        {
            // Reset heart to visible if animation was cancelled
            heart.TextColor = new Color(255, 59, 48); // #FF3B30
            heart.IsVisible = true;
            heart.Opacity = 1;
            heart.Scale = 1;
            heart.Rotation = 0;
        }
        catch (Exception)
        {
            // Ensure heart is visible even if animation fails
            heart.TextColor = new Color(255, 59, 48); // #FF3B30
            heart.IsVisible = true;
            heart.Opacity = 1;
            heart.Scale = 1;
            heart.Rotation = 0;
                    }
    }

    // MASTER HEART CONTROL - Keep fade animations but simplify
    private async Task UpdateHeartDisplayMinecraft(int liveCount)
    {
        if (_isAnimatingHearts) return;
        _isAnimatingHearts = true;
        try
        {
            using (var animationCts = CancellationTokenSource.CreateLinkedTokenSource(_animationCancellation.Token))
            {
                await MainThread.InvokeOnMainThreadAsync(async () => {
                    Heart1.TranslationX = Heart2.TranslationX = Heart3.TranslationX = 0;
                    Heart1.TranslationY = Heart2.TranslationY = Heart3.TranslationY = 0;
                    Heart1.Rotation = Heart2.Rotation = Heart3.Rotation = 0;

                    // Always fade the rightmost visible heart (3, 2, 1)
                    if (_previousLives > liveCount)
                    {
                        if (_previousLives == 3 && liveCount == 2)
                        {
                            await AnimateHeartDamageMinecraft(Heart3, animationCts.Token);
                        }
                        else if (_previousLives == 2 && liveCount == 1)
                        {
                            await AnimateHeartDamageMinecraft(Heart2, animationCts.Token);
                        }
                        else if (_previousLives == 1 && liveCount == 0)
                        {
                            await AnimateHeartDamageMinecraft(Heart1, animationCts.Token);
                        }
                    }
                    else if (_previousLives < liveCount)
                    {
                        SetHeartStates(liveCount);
                    }
                    else
                    {
                        SetHeartStates(liveCount);
                    }
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in heart animation: {ex.Message}");
            await MainThread.InvokeOnMainThreadAsync(() => {
                SetHeartStates(liveCount);
            });
        }
        finally
        {
            _isAnimatingHearts = false;
        }
    }
    
    // Set heart states directly without animation (failsafe method)
    private void SetHeartStates(int liveCount)
                        {
        // Reset any problematic properties
        Heart1.TranslationX = Heart2.TranslationX = Heart3.TranslationX = 0;
        Heart1.TranslationY = Heart2.TranslationY = Heart3.TranslationY = 0;
        Heart1.Rotation = Heart2.Rotation = Heart3.Rotation = 0;
        Heart1.TextColor = Heart2.TextColor = Heart3.TextColor = new Color(255, 59, 48); // #FF3B30
        
        // Set visibility based on lives
        Heart1.IsVisible = liveCount >= 1;
        Heart1.Opacity = liveCount >= 1 ? 1 : 0;
        Heart1.Scale = liveCount >= 1 ? 1 : 0;
        
        Heart2.IsVisible = liveCount >= 2;
        Heart2.Opacity = liveCount >= 2 ? 1 : 0;
        Heart2.Scale = liveCount >= 2 ? 1 : 0;
        
        Heart3.IsVisible = liveCount >= 3;
        Heart3.Opacity = liveCount >= 3 ? 1 : 0;
        Heart3.Scale = liveCount >= 3 ? 1 : 0;
    }
    
    // Reset all hearts safely
    private async void ResetAllHeartsMinecraft()
    {
        // Cancel any ongoing animations
        try 
        {
            _animationCancellation.Cancel();
            _animationCancellation.Dispose();
            _animationCancellation = new CancellationTokenSource();
        }
        catch { /* Ignore errors during cancellation */ }
        
        // Make sure we're not in the middle of another animation
        if (_isAnimatingHearts)
        {
            // Just directly set the hearts state instead of animating
            MainThread.BeginInvokeOnMainThread(() => {
                SetHeartStates(3);
                _previousLives = 3;
            });
            return;
        }
        
        _isAnimatingHearts = true;
        
        try
        {
            await MainThread.InvokeOnMainThreadAsync(async () => {
                // Safe reset of heart states
                SetHeartStates(0);
                _previousLives = 0;
                
                // Small delay before restoring hearts
                await Task.Delay(200);
                
                // Restore hearts one by one with simple animations
                if (!_animationCancellation.Token.IsCancellationRequested)
                    await AnimateHeartRestoreMinecraft(Heart1, _animationCancellation.Token);
                
                await Task.Delay(100);
                
                if (!_animationCancellation.Token.IsCancellationRequested)
                    await AnimateHeartRestoreMinecraft(Heart2, _animationCancellation.Token);
                
                await Task.Delay(100);
                
                if (!_animationCancellation.Token.IsCancellationRequested)
                    await AnimateHeartRestoreMinecraft(Heart3, _animationCancellation.Token);
                
                // Update previous lives
                _previousLives = 3;
            });
        }
        catch (Exception)
        {
            // If animation fails, just set the hearts directly
            MainThread.BeginInvokeOnMainThread(() => {
                SetHeartStates(3);
                _previousLives = 3;
            });
            }
            finally
            {
            _isAnimatingHearts = false;
        }
    }

    private async void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        try
        {
            if (e.PropertyName == nameof(_viewModel.Lives))
            {
                var currentLives = _viewModel.Lives;
                // Always animate heart fade immediately when lives decrease
                await UpdateHeartDisplayMinecraft(currentLives);
                _previousLives = currentLives;
            }
        }
        catch (Exception)
        {
            // Fallback if property change handler fails
            MainThread.BeginInvokeOnMainThread(() => {
                SetHeartStates(_viewModel.Lives);
                _previousLives = _viewModel.Lives;
            });
        }
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
        try
    {
        AnswerDropZone.BackgroundColor = new Color(43, 75, 143); // #2B4B8F
        }
        catch { /* Ignore failures to change background color */ }
    }

    private void OnDragLeave(object sender, DragEventArgs e)
    {
        try
    {
        AnswerDropZone.BackgroundColor = new Color(27, 59, 111); // #1B3B6F
        }
        catch { /* Ignore failures to change background color */ }
    }

    private void OnDrop(object sender, DropEventArgs e)
    {
        try
        {
            AnswerDropZone.BackgroundColor = new Color(27, 59, 111);
            if (e.Data.Properties.TryGetValue("Answer", out var answer) && answer is string answerText)
            {
                _viewModel.DroppedAnswer = answerText;
                _viewModel.CheckAnswer(answerText);
                // Heart animation is now always handled in OnViewModelPropertyChanged
            }
        }
        catch (Exception)
        {
            // Fallback for drop handler
        }
    }

    protected override void OnDisappearing()
    {
        try
        {
            // Cancel any ongoing animations
            _animationCancellation?.Cancel();
            
            // Unsubscribe from events
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
            
            // Dispose resources
            _animationCancellation?.Dispose();
            _animationSemaphore?.Dispose();
        }
        catch { /* Ignore disposal errors */ }
        
        base.OnDisappearing();
                    }

    // Handle restart from Game Over screen - INSTANT RESET
    private void OnRestartClicked(object sender, EventArgs e)
    {
        try
        {
            // Cancel any ongoing animations
            _animationCancellation?.Cancel();
            _animationCancellation?.Dispose();
            _animationCancellation = new CancellationTokenSource();
            
            // Instant heart reset with NO animation
            MainThread.BeginInvokeOnMainThread(() => {
                // Direct UI reset with 100% guaranteed hearts
                Heart1.IsVisible = Heart2.IsVisible = Heart3.IsVisible = true;
                Heart1.Opacity = Heart2.Opacity = Heart3.Opacity = 1;
                Heart1.Scale = Heart2.Scale = Heart3.Scale = 1;
                Heart1.Rotation = Heart2.Rotation = Heart3.Rotation = 0;
                Heart1.TranslationX = Heart2.TranslationX = Heart3.TranslationX = 0;
                Heart1.TranslationY = Heart2.TranslationY = Heart3.TranslationY = 0;
                Heart1.TextColor = Heart2.TextColor = Heart3.TextColor = new Color(255, 59, 48); // #FF3B30
                
                // Reset previous lives tracker
                _previousLives = 3;
                
                // Log reset for debugging
                System.Diagnostics.Debug.WriteLine("Hearts reset - INSTANT");
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in heart reset: {ex.Message}");
            
            // Ultra-safe fallback
            Task.Delay(100).ContinueWith(_ => {
                MainThread.BeginInvokeOnMainThread(() => {
                    ForceHeartReset();
                });
            });
        }
    }

    // GUARANTEED HEART RESET - This will ALWAYS show 3 hearts with no glitches
    private void ForceHeartReset()
    {
        try
        {
            MainThread.BeginInvokeOnMainThread(() => {
                // Direct UI reset with 100% guaranteed hearts
                Heart1.IsVisible = Heart2.IsVisible = Heart3.IsVisible = true;
                Heart1.Opacity = Heart2.Opacity = Heart3.Opacity = 1;
                Heart1.Scale = Heart2.Scale = Heart3.Scale = 1;
                Heart1.Rotation = Heart2.Rotation = Heart3.Rotation = 0;
                Heart1.TranslationX = Heart2.TranslationX = Heart3.TranslationX = 0;
                Heart1.TranslationY = Heart2.TranslationY = Heart3.TranslationY = 0;
                
                // Set original heart color
                Heart1.TextColor = Heart2.TextColor = Heart3.TextColor = new Color(255, 59, 48); // #FF3B30
                
                // Reset previous lives tracker
                _previousLives = 3;
                
                // Also ensure ViewModel has 3 lives
                if (_viewModel.Lives != 3)
                {
                    _viewModel.ResetLives();
                }
                
                // Log success
                System.Diagnostics.Debug.WriteLine("Force heart reset successful");
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in ForceHeartReset: {ex.Message}");
            
            // Ultra-safe fallback with delay
            Task.Delay(100).ContinueWith(_ => {
                try
                {
                    MainThread.BeginInvokeOnMainThread(() => {
                        Heart1.IsVisible = Heart2.IsVisible = Heart3.IsVisible = true;
                        Heart1.Opacity = Heart2.Opacity = Heart3.Opacity = 1;
                        Heart1.Scale = Heart2.Scale = Heart3.Scale = 1;
                        _previousLives = 3;
                        _viewModel.ResetLives();
                    });
                }
                catch { /* Final fail-safe - nothing more we can do */ }
            });
        }
    }

    private async void OnHeartFadeRequested()
    {
        // Wait for the heart fade animation to complete, then continue
        await UpdateHeartDisplayMinecraft(_viewModel.Lives);
        _viewModel.ContinueAfterHeartFade();
    }
} 