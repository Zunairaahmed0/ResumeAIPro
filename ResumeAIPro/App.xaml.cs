using System;
using System.Windows;
using ResumeAIPro.Models;
using ResumeAIPro.Services;
using ResumeAIPro.Views;

namespace ResumeAIPro
{
    public partial class App : Application
    {
        public static UserSession? CurrentUser { get; set; }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ShutdownMode = ShutdownMode.OnLastWindowClose;

            var settings    = new SettingsService();
            var firebaseKey = settings.LoadFirebaseKey();

            // Try to silently restore a saved Firebase session
            if (!string.IsNullOrEmpty(firebaseKey))
            {
                var saved = settings.LoadSession();
                if (saved != null && !string.IsNullOrEmpty(saved.RefreshToken))
                {
                    try
                    {
                        var auth      = new AuthService();
                        var refreshed = await auth.TryRefreshAsync(firebaseKey, saved.RefreshToken);
                        if (refreshed != null)
                        {
                            refreshed.Email       = saved.Email;
                            refreshed.DisplayName = saved.DisplayName;
                            refreshed.LocalId     = saved.LocalId;
                            CurrentUser           = refreshed;
                            settings.SaveSession(refreshed);
                            new MainWindow().Show();
                            return;
                        }
                    }
                    catch { /* session expired — fall through to login */ }
                }
            }

            // Always show login screen (demo credentials work without Firebase)
            new LoginWindow().Show();
        }
    }
}
