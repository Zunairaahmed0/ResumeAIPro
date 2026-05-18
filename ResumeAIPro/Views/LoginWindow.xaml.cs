using System.Windows;
using System.Windows.Input;
using ResumeAIPro.Models;
using ResumeAIPro.Services;

namespace ResumeAIPro.Views
{
    public partial class LoginWindow : Window
    {
        private readonly AuthService    _auth     = new();
        private readonly SettingsService _settings = new();
        private bool _isSignInTab = true;

        public LoginWindow()
        {
            InitializeComponent();
            Loaded       += (_, _) => StateChanged += LoginWindow_StateChanged;
        }

        // ── Window controls (match MainWindow style) ─────────────

        private void LoginWindow_StateChanged(object? sender, System.EventArgs e)
        {
            Padding = WindowState == WindowState.Maximized
                ? new Thickness(7)
                : new Thickness(0);
        }

        private void MinBtn_Click(object sender, RoutedEventArgs e)
            => WindowState = WindowState.Minimized;

        private void MaxBtn_Click(object sender, RoutedEventArgs e)
            => WindowState = WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;

        private void CloseBtn_Click(object sender, RoutedEventArgs e) => Close();

        // ── Tab switching ────────────────────────────────────────

        private void SignInTab_Click(object sender, RoutedEventArgs e)
        {
            if (_isSignInTab) return;
            _isSignInTab = true;
            SignInPanel.Visibility   = Visibility.Visible;
            RegisterPanel.Visibility = Visibility.Collapsed;
            SignInTabBtn.Style    = (Style)FindResource("TabBtnActive");
            RegisterTabBtn.Style  = (Style)FindResource("TabBtnInactive");
            ClearErrors();
        }

        private void RegisterTab_Click(object sender, RoutedEventArgs e)
        {
            if (!_isSignInTab) return;
            _isSignInTab = false;
            SignInPanel.Visibility   = Visibility.Collapsed;
            RegisterPanel.Visibility = Visibility.Visible;
            SignInTabBtn.Style   = (Style)FindResource("TabBtnInactive");
            RegisterTabBtn.Style = (Style)FindResource("TabBtnActive");
            ClearErrors();
        }

        // ── Sign In ──────────────────────────────────────────────

        private async void SignIn_Click(object sender, RoutedEventArgs e)
        {
            var email    = SignInEmail.Text.Trim();
            var password = SignInPassword.Password;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ShowSignInError("Please enter your email and password.");
                return;
            }

            if (_auth.TryDemoLogin(email, password, out var demoSession))
            {
                OpenMain(demoSession!);
                return;
            }

            var firebaseKey = _settings.LoadFirebaseKey();
            if (string.IsNullOrEmpty(firebaseKey))
            {
                ShowSignInError("Firebase is not configured. Use demo credentials or continue as guest.");
                return;
            }

            SetLoading(true);
            try
            {
                var session = await _auth.SignInAsync(firebaseKey, email, password);
                _settings.SaveSession(session);
                OpenMain(session);
            }
            catch (System.Exception ex)
            {
                ShowSignInError(ex.Message);
            }
            finally { SetLoading(false); }
        }

        // ── Register ─────────────────────────────────────────────

        private async void Register_Click(object sender, RoutedEventArgs e)
        {
            var name     = RegName.Text.Trim();
            var email    = RegEmail.Text.Trim();
            var password = RegPassword.Password;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ShowRegisterError("Please fill in all required fields.");
                return;
            }

            var firebaseKey = _settings.LoadFirebaseKey();
            if (string.IsNullOrEmpty(firebaseKey))
            {
                ShowRegisterError("Firebase is not configured. Please set it up in Settings first.");
                return;
            }

            SetLoading(true);
            try
            {
                var session = await _auth.RegisterAsync(firebaseKey, email, password, name);
                _settings.SaveSession(session);
                OpenMain(session);
            }
            catch (System.Exception ex)
            {
                ShowRegisterError(ex.Message);
            }
            finally { SetLoading(false); }
        }

        // ── Guest ────────────────────────────────────────────────

        private void Guest_Click(object sender, RoutedEventArgs e)
        {
            OpenMain(new UserSession { IsGuest = true, DisplayName = "Guest" });
        }

        // ── Helpers ──────────────────────────────────────────────

        private void OpenMain(UserSession session)
        {
            App.CurrentUser = session;
            new MainWindow().Show();
            Close();
        }

        private void ShowSignInError(string msg)
        {
            SignInError.Text       = msg;
            SignInError.Visibility = Visibility.Visible;
        }

        private void ShowRegisterError(string msg)
        {
            RegisterError.Text       = msg;
            RegisterError.Visibility = Visibility.Visible;
        }

        private void ClearErrors()
        {
            SignInError.Visibility   = Visibility.Collapsed;
            RegisterError.Visibility = Visibility.Collapsed;
        }

        private void SetLoading(bool loading)
        {
            LoadingOverlay.Visibility = loading ? Visibility.Visible : Visibility.Collapsed;
            SignInBtn.IsEnabled       = !loading;
            RegisterBtn.IsEnabled     = !loading;
        }

        private void Password_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) SignIn_Click(sender, new RoutedEventArgs());
        }
    }
}
