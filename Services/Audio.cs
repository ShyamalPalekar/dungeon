using System;
using System.IO;
using System.Media;
using System.Windows.Media;

namespace DungeonGameWpf.Services
{
    public static class Audio
    {
        private static SoundPlayer? _click, _hover, _move, _win, _lose;
        private static MediaPlayer? _powerUp, _trap, _teleport, _keyFound;
        private static MediaPlayer? _bgmPlayer;

        public static bool Enabled { get; set; } = true;
        public static float Volume { get; set; } = 0.8f;
        public static bool BgmEnabled { get; set; } = true;
        public static float BgmVolume { get; set; } = 0.5f;

        public static void Init(bool enabled)
        {
            Enabled = enabled;

            // Load WAV files using SoundPlayer (for simple sounds)
            TryLoadWav(ref _click, "Assets/click.wav");
            TryLoadWav(ref _hover, "Assets/hover.wav");
            TryLoadWav(ref _move, "Assets/move.wav");
            TryLoadWav(ref _win, "Assets/win.wav");
            TryLoadWav(ref _lose, "Assets/lose.wav");

            // Load more complex sounds using MediaPlayer
            TryLoadMedia(ref _powerUp, "Assets/powerup.wav");
            TryLoadMedia(ref _trap, "Assets/trap.wav");
            TryLoadMedia(ref _teleport, "Assets/teleport.wav");
            TryLoadMedia(ref _keyFound, "Assets/keyfound.wav");
        }

        static void TryLoadWav(ref SoundPlayer? sp, string path)
        {
            try
            {
                if (File.Exists(path))
                    sp = new SoundPlayer(path);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load WAV {path}: {ex.Message}");
            }
        }

        static void TryLoadMedia(ref MediaPlayer? mp, string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    mp = new MediaPlayer();
                    mp.Open(new Uri(Path.GetFullPath(path)));
                    mp.Volume = Volume;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load Media {path}: {ex.Message}");
            }
        }

        static void Play(SoundPlayer? sp)
        {
            if (Enabled && sp != null)
            {
                try { sp.Play(); }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to play sound: {ex.Message}");
                }
            }
        }

        static void PlayMedia(MediaPlayer? mp)
        {
            if (Enabled && mp != null)
            {
                try
                {
                    mp.Volume = Volume;
                    mp.Position = TimeSpan.Zero;
                    mp.Play();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to play media: {ex.Message}");
                }
            }
        }

        public static void SetVolume(float volume)
        {
            Volume = Math.Max(0f, Math.Min(1f, volume));

            // Update volume for MediaPlayer instances
            if (_powerUp != null) _powerUp.Volume = Volume;
            if (_trap != null) _trap.Volume = Volume;
            if (_teleport != null) _teleport.Volume = Volume;
            if (_keyFound != null) _keyFound.Volume = Volume;
        }

        // Basic UI sounds
        public static void PlayClick() => Play(_click);
        public static void PlayHover() => Play(_hover);
        public static void PlayMove() => Play(_move);
        public static void PlayWin() => Play(_win);
        public static void PlayLose() => Play(_lose);

        // Game-specific sounds
        public static void PlayPowerUp() => PlayMedia(_powerUp);
        public static void PlayTrap() => PlayMedia(_trap);
        public static void PlayTeleport() => PlayMedia(_teleport);
        public static void PlayKeyFound() => PlayMedia(_keyFound);

        public static void SetBgmVolume(float volume)
        {
            BgmVolume = Math.Clamp(volume, 0f, 1f);
            if (_bgmPlayer != null)
                _bgmPlayer.Volume = BgmVolume;
        }

        public static void StartBgm(string? filePath = null)
        {
            if (!BgmEnabled) return;

            try
            {
                _bgmPlayer?.Close();
                _bgmPlayer = new MediaPlayer();

                // Use provided file or default BGM
                string bgmPath = filePath ?? "Assets/bgm.mp3";

                if (File.Exists(bgmPath))
                {
                    _bgmPlayer.Open(new Uri(Path.GetFullPath(bgmPath)));
                    _bgmPlayer.Volume = BgmVolume;
                    _bgmPlayer.MediaEnded += (s, e) =>
                    {
                        _bgmPlayer.Position = TimeSpan.Zero;
                        _bgmPlayer.Play();
                    };
                    _bgmPlayer.Play();
                }
                else
                {
                    Console.WriteLine("🎵 BGM file not found, music disabled");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error starting BGM: {ex.Message}");
            }
        }

        public static void StopBgm()
        {
            try
            {
                _bgmPlayer?.Stop();
                _bgmPlayer?.Close();
                _bgmPlayer = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error stopping BGM: {ex.Message}");
            }
        }

        // Play random positive sound
        public static void PlayPositiveSound()
        {
            var sounds = new Action[] { PlayPowerUp, PlayKeyFound };
            var random = new Random();
            sounds[random.Next(sounds.Length)]();
        }

        // Play random negative sound
        public static void PlayNegativeSound()
        {
            var sounds = new Action[] { PlayTrap, PlayLose };
            var random = new Random();
            sounds[random.Next(sounds.Length)]();
        }

        public static void Dispose()
        {
            try
            {
                _click?.Dispose();
                _hover?.Dispose();
                _move?.Dispose();
                _win?.Dispose();
                _lose?.Dispose();

                _powerUp?.Close();
                _trap?.Close();
                _teleport?.Close();
                _keyFound?.Close();
                _bgmPlayer?.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error disposing audio resources: {ex.Message}");
            }
        }
    }
}
