using Firebase.Auth;

namespace YourMauiApp.Services
{
    public class FirebaseAuthService
    {
        private const string ApiKey = "AIzaSyDXy6rlUuLdsdSDl9wenBMMt59F0vXF1OE";
        private readonly FirebaseAuthProvider authProvider;

        public FirebaseAuthService()
        {
            authProvider = new FirebaseAuthProvider(new FirebaseConfig(ApiKey));
        }

        public async Task<FirebaseAuthLink> SignUp(string email, string password)
        {
            return await authProvider.CreateUserWithEmailAndPasswordAsync(email, password);
        }

        public async Task<FirebaseAuthLink> SignIn(string email, string password)
        {
            return await authProvider.SignInWithEmailAndPasswordAsync(email, password);
        }

        public async Task<string> GetFreshToken(FirebaseAuthLink authLink)
        {
            await authLink.GetFreshAuthAsync();
            return authLink.FirebaseToken;
        }
    }
}
