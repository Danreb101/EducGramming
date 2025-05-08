using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace EducGramming.Views;

public partial class VideoPlayerPage : ContentPage
{
    public VideoPlayerPage(string title, string description, string videoUrl)
    {
        InitializeComponent();
        
        TitleLabel.Text = title;
        DescriptionLabel.Text = description;
        
        // Create HTML with enhanced video player
        var html = $@"
            <html>
            <head>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <style>
                    body {{ 
                        margin: 0; 
                        padding: 0; 
                        background-color: #1B3B6F;
                        display: flex;
                        justify-content: center;
                        align-items: center;
                        min-height: 100vh;
                    }}
                    .video-container {{ 
                        width: 100%;
                        max-width: 1200px;
                        position: relative;
                        aspect-ratio: 16/9;
                    }}
                    video {{ 
                        width: 100%;
                        height: 100%;
                        object-fit: contain;
                        border-radius: 12px;
                        background-color: #0A1128;
                    }}
                    video::-webkit-media-controls {{
                        background-color: rgba(0, 0, 0, 0.5);
                        border-bottom-left-radius: 12px;
                        border-bottom-right-radius: 12px;
                    }}
                    .loading {{
                        position: absolute;
                        top: 50%;
                        left: 50%;
                        transform: translate(-50%, -50%);
                        width: 50px;
                        height: 50px;
                        border: 4px solid #f3f3f3;
                        border-top: 4px solid #3498db;
                        border-radius: 50%;
                        animation: spin 1s linear infinite;
                        display: none;
                    }}
                    @keyframes spin {{
                        0% {{ transform: translate(-50%, -50%) rotate(0deg); }}
                        100% {{ transform: translate(-50%, -50%) rotate(360deg); }}
                    }}
                </style>
            </head>
            <body>
                <div class='video-container'>
                    <div class='loading' id='loadingSpinner'></div>
                    <video controls autoplay id='videoPlayer'>
                        <source src='{videoUrl}' type='video/mp4'>
                        Your browser does not support the video tag.
                    </video>
                </div>
                <script>
                    const video = document.getElementById('videoPlayer');
                    const loading = document.getElementById('loadingSpinner');
                    
                    video.addEventListener('loadstart', () => {{
                        loading.style.display = 'block';
                    }});
                    
                    video.addEventListener('canplay', () => {{
                        loading.style.display = 'none';
                    }});
                    
                    video.addEventListener('waiting', () => {{
                        loading.style.display = 'block';
                    }});
                    
                    video.addEventListener('playing', () => {{
                        loading.style.display = 'none';
                    }});
                </script>
            </body>
            </html>";
            
        VideoPlayer.Source = new HtmlWebViewSource { Html = html };
    }

    private async void OnCloseClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        VideoPlayer.Source = null;
    }
} 