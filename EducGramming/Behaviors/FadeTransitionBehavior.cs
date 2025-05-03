using Microsoft.Maui.Controls;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace EducGramming.Behaviors
{
    public class FadeTransitionBehavior : Behavior<View>
    {
        protected override void OnAttachedTo(View view)
        {
            base.OnAttachedTo(view);
            view.PropertyChanged += OnViewPropertyChanged;
        }

        protected override void OnDetachingFrom(View view)
        {
            base.OnDetachingFrom(view);
            view.PropertyChanged -= OnViewPropertyChanged;
        }

        private async void OnViewPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == View.IsVisibleProperty.PropertyName && sender is View view)
            {
                if (!view.IsVisible)
                {
                    // Quick pop-up effect
                    await view.ScaleTo(1.2, 50, Easing.CubicOut);
                    
                    // Parallel animations for game-like effect
                    var animations = new List<Task>
                    {
                        // Float upward with bounce
                        view.TranslateTo(0, -15, 300, Easing.CubicOut),
                        // Scale down while floating
                        view.ScaleTo(0.8, 300, Easing.CubicIn),
                        // Fade out
                        view.FadeTo(0, 300, Easing.CubicIn)
                    };

                    await Task.WhenAll(animations);
                }
                else
                {
                    // Reset all transformations instantly for next use
                    view.Scale = 1;
                    view.TranslationY = 0;
                    view.Opacity = 1;
                }
            }
        }
    }
} 