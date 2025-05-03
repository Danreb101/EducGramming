using System.Text.Json;

namespace EducGramming.Services
{
    public class UserService
    {
        private const string UsersKey = "RegisteredUsers";

        public class UserData
        {
            public string Email { get; set; }
            public string Password { get; set; }
            public string Username { get; set; }
            public string FullName { get; set; }
        }

        public async Task RegisterUser(string email, string password, string username, string fullName)
        {
            var users = await GetAllUsers();
            
            // Check if user already exists
            if (users.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
            {
                throw new Exception("A user with this email already exists.");
            }

            // Add new user
            users.Add(new UserData 
            { 
                Email = email,
                Password = password, // In a real app, this should be hashed
                Username = username,
                FullName = fullName
            });

            // Save updated users list
            await SaveUsers(users);
        }

        public async Task<bool> ValidateLogin(string email, string password)
        {
            var users = await GetAllUsers();
            var user = users.FirstOrDefault(u => 
                u.Email.Equals(email, StringComparison.OrdinalIgnoreCase) && 
                u.Password.Equals(password));

            if (user != null)
            {
                // Store current user info
                Preferences.Default.Set("CurrentEmail", user.Email);
                Preferences.Default.Set("CurrentUsername", user.Username);
                Preferences.Default.Set("CurrentFullName", user.FullName);
                return true;
            }

            return false;
        }

        private async Task<List<UserData>> GetAllUsers()
        {
            var usersJson = Preferences.Default.Get<string>(UsersKey, null);
            if (string.IsNullOrEmpty(usersJson))
            {
                return new List<UserData>();
            }

            return JsonSerializer.Deserialize<List<UserData>>(usersJson) ?? new List<UserData>();
        }

        private async Task SaveUsers(List<UserData> users)
        {
            var usersJson = JsonSerializer.Serialize(users);
            Preferences.Default.Set(UsersKey, usersJson);
        }
    }
} 