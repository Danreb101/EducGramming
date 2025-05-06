using Microsoft.Maui.Controls;
using EducGramming.ViewModels;
using EducGramming.Services;

namespace EducGramming.Views;

public partial class ProfilePage : ContentPage
{
    private ProfileViewModel _viewModel;
    private int _titleTapCount = 0;
    private UserService _userService;

    public ProfilePage(ProfileViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
        _userService = new UserService();
        
        // Add tap gesture to profile title for admin view
        var titleTapGesture = new TapGestureRecognizer();
        titleTapGesture.Tapped += OnProfileTitleTapped;
        
        // Find the title label - update this to match your actual XAML
        var profileElements = Content.FindByName<VerticalStackLayout>("ProfileHeaderSection");
        if (profileElements != null)
        {
            foreach (var element in profileElements.Children)
            {
                if (element is Label label && label.Text == "My Profile")
                {
                    label.GestureRecognizers.Add(titleTapGesture);
                    break;
                }
            }
        }
    }
    
    private async void OnProfileTitleTapped(object sender, EventArgs e)
    {
        _titleTapCount++;
        
        // After 5 taps, show registered users
        if (_titleTapCount >= 5)
        {
            _titleTapCount = 0;
            
            try
            {
                var users = await _userService.GetAllUsers();
                
                // Build the user list text
                string usersText = "";
                if (users.Count == 0)
                {
                    usersText = "No registered users found.";
                }
                else
                {
                    usersText = $"Registered Users ({users.Count}):\n\n";
                    foreach (var user in users)
                    {
                        usersText += $"• Email: {user.Email}\n  Username: {user.Username}\n  Name: {user.FullName}\n\n";
                    }
                }
                
                // Display the users
                await DisplayAlert("Registered Users (Admin Mode)", usersText, "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Could not retrieve users: {ex.Message}", "OK");
            }
        }
    }
} 