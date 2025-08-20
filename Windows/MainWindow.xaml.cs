using DungeonGameWpf.Services;
using DungeonGame.Services;
using DungeonGame.Windows;
using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace DungeonGameWpf.Windows
{
    public partial class MainWindow : Window
    {
        private MediaPlayer _bgm = new MediaPlayer();
        private readonly UpdateService _updateService = new UpdateService();

        public static bool BgmOn = true;
        public static bool SfxOn = true;
        public static double BgmVolume = 0.4;

        public MainWindow()
        {
            InitializeComponent();
            Audio.Init(SfxOn);           // load SFX
            TryLoadAudio();              // load BGM

            // Always show splash first for better user experience
            Loaded += async (_, __) =>
            {
                // Auto-show splash screen
                ShowSplash();

                // Start background music after a short delay
                var musicTimer = new System.Windows.Threading.DispatcherTimer();
                musicTimer.Interval = TimeSpan.FromSeconds(0.5);
                musicTimer.Tick += (s, e) =>
                {
                    if (BgmOn) PlayBgm();
                    musicTimer.Stop();
                };
                musicTimer.Start();

                // Check for updates after UI loads
                _ = CheckForUpdatesAsync();
            };
        }

        void TryLoadAudio()
        {
            try
            {
                // Prioritas WAV, fallback MP3
                string? bgmPath = File.Exists("Assets/bgm.wav") ? "Assets/bgm.wav"
                               : (File.Exists("Assets/bgm.mp3") ? "Assets/bgm.mp3" : null);
                if (bgmPath != null)
                {
                    _bgm.Open(new Uri(Path.GetFullPath(bgmPath)));
                    _bgm.MediaEnded += (_, __) => { _bgm.Position = TimeSpan.Zero; _bgm.Play(); };
                    _bgm.Volume = BgmVolume;
                }
            }
            catch { /* ignore */ }
        }

        void PlayBgm()
        {
            try
            {
                Audio.BgmEnabled = true;
                Audio.SetBgmVolume((float)BgmVolume);
                Audio.StartBgm();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"BGM Error: {ex.Message}");
            }
        }

        void StopBgm()
        {
            try
            {
                Audio.StopBgm();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Stop BGM Error: {ex.Message}");
            }
        }

        void ShowSplash()
        {
            SplashOverlay.Opacity = 0;
            SplashOverlay.Visibility = Visibility.Visible;
            var sb = new Storyboard();
            sb.Children.Add(new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(320))
            { EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut } });
            Storyboard.SetTarget(sb.Children[0], SplashOverlay);
            Storyboard.SetTargetProperty(sb.Children[0], new PropertyPath("Opacity"));
            sb.Begin();
        }

        void HideSplash()
        {
            var da = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(220))
            { EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn } };
            da.Completed += (_, __) => SplashOverlay.Visibility = Visibility.Collapsed;
            SplashOverlay.BeginAnimation(OpacityProperty, da);
        }

        // Buttons
        private void DismissSplash_Click(object sender, RoutedEventArgs e) => HideSplash();
        private void Tutorial_Click(object sender, RoutedEventArgs e) { Audio.PlayClick(); ShowSplash(); }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Audio.PlayClick();
                var gameWindow = new GameWindow(GameWindow.GameMode.SingleVsAI);
                gameWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting game: {ex.Message}", "Game Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreateRoom_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Audio.PlayClick();
                var lobbyWindow = new LobbyWindow();
                lobbyWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating room: {ex.Message}", "Lobby Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TrainAI_Click(object sender, RoutedEventArgs e)
        {
            Audio.PlayClick();
            new TrainWindow().ShowDialog();
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            Audio.PlayClick();
            var w = new SettingsWindow();
            if (w.ShowDialog() == true)
            {
                if (BgmOn) PlayBgm(); else StopBgm();
                _bgm.Volume = BgmVolume;
                Audio.Enabled = SfxOn;
            }
        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            Audio.PlayClick();
            MessageBox.Show(
                "Gerak hanya Right atau Down. HP ≤ 0 = kalah.\n" +
                "Solver DP menghitung minimum HP awal.\n" +
                "AI memakai Q-Learning: latih di menu Train AI.\n" +
                "Create Room = Multiplayer lokal (gantian satu PC).",
                "Help");
        }

        private async System.Threading.Tasks.Task CheckForUpdatesAsync()
        {
            try
            {
                // Check settings
                var settings = DungeonGame.Properties.Settings.Default;
                if (!settings.AutoUpdateEnabled) return;

                // Check if enough time has passed since last check
                var hoursSinceLastCheck = (DateTime.Now - settings.LastUpdateCheck).TotalHours;
                if (hoursSinceLastCheck < settings.UpdateCheckInterval) return;

                // Update last check time
                settings.LastUpdateCheck = DateTime.Now;
                settings.Save();

                var updateInfo = await _updateService.CheckForUpdatesAsync();
                if (updateInfo != null && updateInfo.Version != settings.SkippedVersion)
                {
                    // Show update dialog on UI thread
                    Dispatcher.Invoke(() =>
                    {
                        var updateWindow = new UpdateWindow(updateInfo, _updateService);
                        updateWindow.Owner = this;
                        updateWindow.ShowDialog();
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Update check failed: {ex.Message}");
                // Silently fail - don't interrupt user experience
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e) => Close();
    }
}
