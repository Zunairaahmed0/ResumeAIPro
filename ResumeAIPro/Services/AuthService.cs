using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ResumeAIPro.Models;

namespace ResumeAIPro.Services
{
    public class AuthService
    {
        private readonly HttpClient _http = new();
        private const string AuthBase  = "https://identitytoolkit.googleapis.com/v1/accounts";
        private const string TokenBase = "https://securetoken.googleapis.com/v1/token";

        // Demo credentials — work without Firebase
        private const string DemoEmail    = "demo@resumeaipro.com";
        private const string DemoPassword = "Demo@1234";

        public bool TryDemoLogin(string email, string password, out UserSession? session)
        {
            if (email.Equals(DemoEmail, StringComparison.OrdinalIgnoreCase) && password == DemoPassword)
            {
                session = new UserSession
                {
                    Email       = DemoEmail,
                    DisplayName = "Demo User",
                    LocalId     = "demo-user",
                    IsGuest     = false
                };
                return true;
            }
            session = null;
            return false;
        }

        public async Task<UserSession> SignInAsync(string firebaseKey, string email, string password)
        {
            var body = JsonConvert.SerializeObject(new { email, password, returnSecureToken = true });
            var resp = await _http.PostAsync($"{AuthBase}:signInWithPassword?key={firebaseKey}",
                new StringContent(body, Encoding.UTF8, "application/json"));
            var raw = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode) ThrowFirebaseError(raw);
            return ParseSession(raw);
        }

        public async Task<UserSession> RegisterAsync(string firebaseKey, string email, string password, string name)
        {
            var body = JsonConvert.SerializeObject(new { email, password, returnSecureToken = true });
            var resp = await _http.PostAsync($"{AuthBase}:signUp?key={firebaseKey}",
                new StringContent(body, Encoding.UTF8, "application/json"));
            var raw = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode) ThrowFirebaseError(raw);
            var session = ParseSession(raw);

            if (!string.IsNullOrWhiteSpace(name))
            {
                try
                {
                    var upd = JsonConvert.SerializeObject(new { idToken = session.IdToken, displayName = name });
                    await _http.PostAsync($"{AuthBase}:update?key={firebaseKey}",
                        new StringContent(upd, Encoding.UTF8, "application/json"));
                    session.DisplayName = name;
                }
                catch { }
            }
            return session;
        }

        public async Task<UserSession?> TryRefreshAsync(string firebaseKey, string refreshToken)
        {
            try
            {
                var body = $"grant_type=refresh_token&refresh_token={Uri.EscapeDataString(refreshToken)}";
                var resp = await _http.PostAsync($"{TokenBase}?key={firebaseKey}",
                    new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded"));
                if (!resp.IsSuccessStatusCode) return null;
                var j = JObject.Parse(await resp.Content.ReadAsStringAsync());
                return new UserSession
                {
                    IdToken      = j["id_token"]?.ToString()     ?? "",
                    RefreshToken = j["refresh_token"]?.ToString() ?? "",
                    ExpiresAt    = DateTime.Now.AddSeconds(double.Parse(j["expires_in"]?.ToString() ?? "3600"))
                };
            }
            catch { return null; }
        }

        private static UserSession ParseSession(string raw)
        {
            var j = JObject.Parse(raw);
            return new UserSession
            {
                Email        = j["email"]?.ToString()        ?? "",
                DisplayName  = j["displayName"]?.ToString()  ?? "",
                LocalId      = j["localId"]?.ToString()      ?? "",
                IdToken      = j["idToken"]?.ToString()      ?? "",
                RefreshToken = j["refreshToken"]?.ToString() ?? "",
                ExpiresAt    = DateTime.Now.AddSeconds(int.Parse(j["expiresIn"]?.ToString() ?? "3600")),
                IsGuest      = false
            };
        }

        private static void ThrowFirebaseError(string raw)
        {
            string msg;
            try   { msg = JObject.Parse(raw)["error"]?["message"]?.ToString() ?? "Authentication failed"; }
            catch { msg = "Authentication failed"; }

            throw new Exception(msg switch
            {
                "EMAIL_NOT_FOUND"                              => "No account found with this email.",
                "INVALID_PASSWORD"                             => "Incorrect password. Please try again.",
                "INVALID_LOGIN_CREDENTIALS"                    => "Invalid email or password.",
                "USER_DISABLED"                                => "This account has been disabled.",
                "EMAIL_EXISTS"                                 => "An account already exists with this email.",
                "OPERATION_NOT_ALLOWED"                        => "Email/password sign-in is not enabled in Firebase.",
                "TOO_MANY_ATTEMPTS_TRY_LATER"                  => "Too many failed attempts. Please try later.",
                var m when m.Contains("WEAK_PASSWORD")         => "Password must be at least 6 characters.",
                var m                                          => m
            });
        }
    }
}
