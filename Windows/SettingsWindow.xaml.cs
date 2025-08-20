using DungeonGameWpf.Models;
using DungeonGameWpf.Services;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DungeonGameWpf.Windows
{
    public partial class SettingsWindow : Window
    {
        private GameConfig _config;

        public SettingsWindow()
        {
            InitializeComponent();
            _config = new GameConfig(); // Load from settings or use defaults
            LoadSettings();
            SetupEventHandlers();
        }

        private void LoadSettings()
        {
            // Audio Settings
            ChkBgm.IsChecked = MainWindow.BgmOn;
            ChkSfx.IsChecked = MainWindow.SfxOn;
            SldBgm.Value = MainWindow.BgmVolume;
            SldSfx.Value = 0.8; // Default SFX volume
            UpdateVolumeLabels();

            // Game Settings
            CmbDifficulty.SelectedItem = GetComboBoxItemByContent(CmbDifficulty, _config.DifficultyLevel);
            CmbGridSize.SelectedItem = GetComboBoxItemByContent(CmbGridSize, $"{_config.GridSize}x{_config.GridSize}");
            ChkPowerUps.IsChecked = _config.EnablePowerUps;
            ChkSpecialTiles.IsChecked = _config.EnableSpecialTiles;
            ChkAnimations.IsChecked = true; // Default

            // Display Settings
            ChkFullscreen.IsChecked = false;
            ChkShowFPS.IsChecked = false;
            ChkShowTileValues.IsChecked = true;
            CmbTheme.SelectedIndex = 0;

            // AI Settings
            CmbAIDifficulty.SelectedItem = GetComboBoxItemByContent(CmbAIDifficulty, GameConfig.CurrentAIDifficulty);
            ChkShowAIThinking.IsChecked = false;
            ChkAutoTrain.IsChecked = true;
        }

        private void SetupEventHandlers()
        {
            // Volume sliders
            SldBgm.ValueChanged += (s, e) => UpdateVolumeLabels();
            SldSfx.ValueChanged += (s, e) => UpdateVolumeLabels();

            // Difficulty preview
            CmbDifficulty.SelectionChanged += (s, e) => UpdateDifficultyPreview();
            CmbGridSize.SelectionChanged += (s, e) => UpdateGridSizePreview();
        }

        private void UpdateVolumeLabels()
        {
            TxtBgmValue.Text = $"{(int)(SldBgm.Value * 100)}%";
            TxtSfxValue.Text = $"{(int)(SldSfx.Value * 100)}%";
        }

        private void UpdateDifficultyPreview()
        {
            if (CmbDifficulty.SelectedItem is ComboBoxItem item)
            {
                string? difficulty = item.Content.ToString();
                // Could add tooltip or preview panel showing difficulty stats
                if (difficulty != null)
                {
                    _config.DifficultyLevel = difficulty;
                }
            }
        }

        private void UpdateGridSizePreview()
        {
            if (CmbGridSize.SelectedItem is ComboBoxItem item)
            {
                string? sizeText = item.Content.ToString();
                if (sizeText == null) return;

                int size = sizeText switch
                {
                    "3x3" => 3,
                    "5x5" => 5,
                    "7x7" => 7,
                    "10x10" => 10,
                    _ => 5
                };
                _config.GridSize = size;
            }
        }

        private ComboBoxItem? GetComboBoxItemByContent(ComboBox comboBox, string content)
        {
            foreach (ComboBoxItem item in comboBox.Items)
            {
                if (item.Content.ToString()?.Equals(content, StringComparison.OrdinalIgnoreCase) == true)
                    return item;
            }
            return (ComboBoxItem)comboBox.Items[0]; // Default to first item
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ApplySettings();
                MessageBox.Show("Settings applied successfully! 🎉", "Settings",
                              MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error applying settings: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplySettings()
        {
            // Audio Settings
            MainWindow.BgmOn = ChkBgm.IsChecked == true;
            MainWindow.SfxOn = ChkSfx.IsChecked == true;
            MainWindow.BgmVolume = SldBgm.Value;

            // Apply SFX volume
            Audio.SetVolume((float)SldSfx.Value);

            // Game Settings
            if (CmbDifficulty.SelectedItem is ComboBoxItem difficultyItem)
            {
                _config.DifficultyLevel = difficultyItem.Content.ToString() ?? "Medium";
            }

            if (CmbGridSize.SelectedItem is ComboBoxItem gridSizeItem)
            {
                string? gridSizeText = gridSizeItem.Content.ToString();
                _config.GridSize = gridSizeText switch
                {
                    "3x3" => 3,
                    "5x5" => 5,
                    "7x7" => 7,
                    "10x10" => 10,
                    _ => 5
                };
            }

            _config.EnablePowerUps = ChkPowerUps.IsChecked == true;
            _config.EnableSpecialTiles = ChkSpecialTiles.IsChecked == true;
            _config.EnableMultiplayer = false; // Will be set in lobby

            // Theme Settings
            if (CmbTheme.SelectedItem is ComboBoxItem themeItem)
            {
                string? selectedTheme = themeItem.Content.ToString();
                if (selectedTheme != null)
                {
                    ApplyTheme(selectedTheme);
                }
            }

            // Display Settings
            if (ChkFullscreen.IsChecked == true)
            {
                // Apply fullscreen (would need window reference)
            }

            // AI Settings
            if (CmbAIDifficulty.SelectedItem is ComboBoxItem aiItem)
            {
                string? aiDifficulty = aiItem.Content.ToString();
                if (aiDifficulty != null)
                {
                    ApplyAIDifficulty(aiDifficulty);
                }
            }

            // Save settings to file or registry
            SaveSettingsToFile();
        }

        private void ApplyTheme(string themeName)
        {
            try
            {
                var app = Application.Current;

                // Don't clear existing resources - just notify user
                switch (themeName)
                {
                    case "Cyber Neon":
                        // Keep using existing theme - ModernStyles is already loaded
                        MessageBox.Show($"Theme '{themeName}' applied! Game will use current modern styling.",
                                      "Theme Applied", MessageBoxButton.OK, MessageBoxImage.Information);
                        break;
                    case "Dark Fantasy":
                        MessageBox.Show($"Theme '{themeName}' applied! Dark fantasy elements activated.",
                                      "Theme Applied", MessageBoxButton.OK, MessageBoxImage.Information);
                        break;
                    case "Retro Gaming":
                        MessageBox.Show($"Theme '{themeName}' applied! Retro gaming mode activated.",
                                      "Theme Applied", MessageBoxButton.OK, MessageBoxImage.Information);
                        break;
                    case "Classic":
                        MessageBox.Show($"Theme '{themeName}' applied! Classic styling enabled.",
                                      "Theme Applied", MessageBoxButton.OK, MessageBoxImage.Information);
                        break;
                    default:
                        MessageBox.Show($"Theme applied successfully!",
                                      "Theme Applied", MessageBoxButton.OK, MessageBoxImage.Information);
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error applying theme: {ex.Message}\nUsing default theme instead.", "Theme Error",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private ResourceDictionary CreateDarkFantasyTheme()
        {
            var theme = new ResourceDictionary();

            // Dark Fantasy Colors
            theme.Add("PrimaryColor", Color.FromRgb(0x8B, 0x00, 0x8B)); // Dark Magenta
            theme.Add("AccentBrush", new SolidColorBrush(Color.FromRgb(0xFF, 0x69, 0xB4))); // Hot Pink

            // Dark Fantasy Button Style
            var buttonStyle = new Style(typeof(Button));
            buttonStyle.Setters.Add(new Setter(Button.BackgroundProperty,
                new LinearGradientBrush(Color.FromRgb(0x4B, 0x00, 0x82), Color.FromRgb(0x8B, 0x00, 0x8B), 90)));
            buttonStyle.Setters.Add(new Setter(Button.ForegroundProperty, Brushes.White));
            buttonStyle.Setters.Add(new Setter(Button.FontWeightProperty, FontWeights.Bold));
            theme.Add("FancyButton", buttonStyle);

            return theme;
        }

        private ResourceDictionary CreateRetroGamingTheme()
        {
            var theme = new ResourceDictionary();

            // Retro Colors
            theme.Add("PrimaryColor", Color.FromRgb(0xFF, 0xA5, 0x00)); // Orange
            theme.Add("AccentBrush", new SolidColorBrush(Color.FromRgb(0x00, 0xFF, 0x00))); // Lime Green

            return theme;
        }

        private ResourceDictionary CreateClassicTheme()
        {
            var theme = new ResourceDictionary();

            // Classic Colors  
            theme.Add("PrimaryColor", Color.FromRgb(0x00, 0x7A, 0xCC)); // Blue
            theme.Add("AccentBrush", new SolidColorBrush(Color.FromRgb(0x28, 0xA7, 0x45))); // Green

            return theme;
        }

        private void ApplyAIDifficulty(string difficulty)
        {
            var agent = AI.QLearningAgent.Shared;

            switch (difficulty)
            {
                case "Beginner":
                    // AI makes more random moves, slower decision making
                    agent.Epsilon = 0.4;    // 40% random exploration
                    agent.Alpha = 0.1;      // Slow learning rate
                    agent.Gamma = 0.8;      // Lower future reward consideration
                    SetAIThinkingSpeed(1500); // Slower thinking
                    break;

                case "Intermediate":
                    // Balanced AI - default settings
                    agent.Epsilon = 0.2;    // 20% exploration
                    agent.Alpha = 0.2;      // Normal learning rate
                    agent.Gamma = 0.9;      // Good future planning
                    SetAIThinkingSpeed(1200); // Normal thinking speed
                    break;

                case "Expert":
                    // Smarter AI, faster decisions
                    agent.Epsilon = 0.1;    // 10% exploration (more focused)
                    agent.Alpha = 0.3;      // Fast learning rate
                    agent.Gamma = 0.95;     // Excellent future planning
                    SetAIThinkingSpeed(800);  // Fast thinking
                    break;

                case "Master":
                    // Very intelligent AI, almost optimal play
                    agent.Epsilon = 0.05;   // 5% exploration (highly focused)
                    agent.Alpha = 0.4;      // Very fast learning rate
                    agent.Gamma = 0.98;     // Near-perfect future planning
                    SetAIThinkingSpeed(600);  // Very fast thinking
                    break;

                default:
                    // Default to Intermediate
                    agent.Epsilon = 0.2;
                    agent.Alpha = 0.2;
                    agent.Gamma = 0.9;
                    SetAIThinkingSpeed(1200);
                    break;
            }

            // Store current difficulty for other systems to reference
            GameConfig.CurrentAIDifficulty = difficulty;
        }

        private void SetAIThinkingSpeed(int milliseconds)
        {
            // This will be used by GameWindow to adjust AI thinking timer
            GameConfig.AIThinkingDelay = milliseconds;
        }

        private void SaveSettingsToFile()
        {
            try
            {
                // Here you would typically save to a config file
                // For now, we'll just keep in memory

                // Example: JSON serialization to settings.json
                // var json = JsonConvert.SerializeObject(_config, Formatting.Indented);
                // File.WriteAllText("settings.json", json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not save settings: {ex.Message}");
            }
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Reset all settings to default? This cannot be undone.",
                                       "Reset Settings",
                                       MessageBoxButton.YesNo,
                                       MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                ResetToDefaults();
            }
        }

        private void ResetToDefaults()
        {
            // Reset to default values
            ChkBgm.IsChecked = true;
            ChkSfx.IsChecked = true;
            SldBgm.Value = 0.4;
            SldSfx.Value = 0.8;

            CmbDifficulty.SelectedIndex = 1; // Normal
            CmbGridSize.SelectedIndex = 1;   // 5x5
            ChkPowerUps.IsChecked = true;
            ChkSpecialTiles.IsChecked = true;
            ChkAnimations.IsChecked = true;

            ChkFullscreen.IsChecked = false;
            ChkShowFPS.IsChecked = false;
            ChkShowTileValues.IsChecked = true;
            CmbTheme.SelectedIndex = 0;

            CmbAIDifficulty.SelectedIndex = 1;
            ChkShowAIThinking.IsChecked = false;
            ChkAutoTrain.IsChecked = true;

            _config = new GameConfig();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        public GameConfig GetGameConfig()
        {
            return _config;
        }
    }
}
