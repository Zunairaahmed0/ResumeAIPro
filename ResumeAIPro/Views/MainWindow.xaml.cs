using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ResumeAIPro.ViewModels;

namespace ResumeAIPro.Views
{
    public partial class MainWindow : Window
    {
        private MainViewModel VM => (MainViewModel)DataContext;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ApiKeyBox.Password = VM.ApiKey;
            VM.RequestLogout  += OnLogout;
            StateChanged      += MainWindow_StateChanged;
        }

        // Prevent content from going under the taskbar when maximized
        private void MainWindow_StateChanged(object? sender, System.EventArgs e)
        {
            Padding = WindowState == WindowState.Maximized
                ? new Thickness(7)
                : new Thickness(0);
        }

        private void OnLogout(object? sender, System.EventArgs e)
        {
            new LoginWindow().Show();
            Close();
        }

        // ── Window controls ──────────────────────────────────────

        private void MinBtn_Click(object sender, RoutedEventArgs e)
            => WindowState = WindowState.Minimized;

        private void MaxBtn_Click(object sender, RoutedEventArgs e)
            => WindowState = WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
            => Close();

        // ── Navigation shortcuts ─────────────────────────────────

        private void GoToUpload_Click(object sender, RoutedEventArgs e)      => VM.CurrentTabIndex = 1;
        private void GoToMatch_Click(object sender, RoutedEventArgs e)       => VM.CurrentTabIndex = 3;
        private void GoToCoverLetter_Click(object sender, RoutedEventArgs e) => VM.CurrentTabIndex = 4;

        // Dashboard quick-action shortcuts
        private void DashGoToUpload_Click(object sender, RoutedEventArgs e)       => VM.CurrentTabIndex = 1;
        private void DashGoToAnalysis_Click(object sender, RoutedEventArgs e)     => VM.CurrentTabIndex = 2;
        private void DashGoToMatch_Click(object sender, RoutedEventArgs e)        => VM.CurrentTabIndex = 3;
        private void DashGoToCoverLetter_Click(object sender, RoutedEventArgs e)  => VM.CurrentTabIndex = 4;
        private void DashGoToFinder_Click(object sender, RoutedEventArgs e)       => VM.CurrentTabIndex = 5;

        // ── Sidebar nav ──────────────────────────────────────────
        // ListBox has 7 items (0-6) matching TabControl tabs 0-6 directly.
        // Settings (tab 7) is a standalone button outside the ListBox.
        private void SidebarNav_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListBox lb && lb.SelectedIndex >= 0)
                VM.CurrentTabIndex = lb.SelectedIndex;
        }

        private void SettingsNav_Click(object sender, RoutedEventArgs e)
        {
            VM.CurrentTabIndex = 7;
            SidebarNavList.SelectedIndex = -1;
        }

        // ── Drop zone ────────────────────────────────────────────

        private void DropZone_Click(object sender, MouseButtonEventArgs e)
            => VM.UploadResumeCommand.Execute(null);

        private void DropZone_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                DropZone.BorderBrush = new SolidColorBrush(Color.FromRgb(6, 182, 212));
                DropZone.Background  = new SolidColorBrush(Color.FromArgb(60, 6, 182, 212));
                e.Effects = DragDropEffects.Copy;
            }
        }

        private void DropZone_DragLeave(object sender, DragEventArgs e)
        {
            DropZone.BorderBrush = (SolidColorBrush)FindResource("PrimaryPurple");
            DropZone.Background  = new SolidColorBrush(Color.FromRgb(26, 13, 51));
        }

        private async void DropZone_Drop(object sender, DragEventArgs e)
        {
            DropZone_DragLeave(sender, e);
            if (e.Data.GetData(DataFormats.FileDrop) is string[] files && files.Length > 0)
                await VM.ProcessResumeFile(files[0]);
        }

        private void ApiKeyBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox pb)
                VM.ApiKey = pb.Password;
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
