using System;
using System.Windows;
using DungeonGame.Services;

namespace DungeonGame.Windows
{
    public partial class UpdateWindow : Window
    {
        private readonly UpdateInfo _updateInfo;
        private readonly UpdateService _updateService;

        public UpdateWindow(UpdateInfo updateInfo, UpdateService updateService)
        {
            InitializeComponent();
            _updateInfo = updateInfo;
            _updateService = updateService;

            InitializeUpdateInfo();
        }

        private void InitializeUpdateInfo()
        {
            VersionText.Text = $"New version {_updateInfo.Version} available!";
            ReleaseDateText.Text = $"Released: {_updateInfo.ReleaseDate:MMMM d, yyyy}";
            ReleaseNotesText.Text = string.IsNullOrWhiteSpace(_updateInfo.ReleaseNotes)
                ? "No release notes available."
                : _updateInfo.ReleaseNotes;

            if (_updateInfo.IsPrerelease)
            {
                PreReleaseText.Visibility = Visibility.Visible;
            }

            // Disable update button if no download URL
            if (string.IsNullOrEmpty(_updateInfo.DownloadUrl))
            {
                UpdateButton.IsEnabled = false;
                UpdateButton.Content = "Download Unavailable";
            }
        }

        private async void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_updateInfo.DownloadUrl))
                return;

            // Show progress
            ProgressPanel.Visibility = Visibility.Visible;
            UpdateButton.IsEnabled = false;
            LaterButton.IsEnabled = false;
            SkipButton.IsEnabled = false;

            var progress = new Progress<int>(percentage =>
            {
                DownloadProgress.Value = percentage;
                ProgressText.Text = $"{percentage}%";
            });

            try
            {
                var success = await _updateService.DownloadAndInstallUpdateAsync(
                    _updateInfo.DownloadUrl, progress);

                if (!success)
                {
                    MessageBox.Show(
                        "Failed to download or install the update. Please try again later.",
                        "Update Failed",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    // Re-enable buttons
                    UpdateButton.IsEnabled = true;
                    LaterButton.IsEnabled = true;
                    SkipButton.IsEnabled = true;
                    ProgressPanel.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"An error occurred during the update: {ex.Message}",
                    "Update Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                // Re-enable buttons
                UpdateButton.IsEnabled = true;
                LaterButton.IsEnabled = true;
                SkipButton.IsEnabled = true;
                ProgressPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void LaterButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void SkipButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                $"Are you sure you want to skip version {_updateInfo.Version}?\nYou won't be notified about this version again.",
                "Skip Version",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Save skipped version (could be implemented in settings)
                Properties.Settings.Default.SkippedVersion = _updateInfo.Version;
                Properties.Settings.Default.Save();

                DialogResult = false;
                Close();
            }
        }
    }
}
