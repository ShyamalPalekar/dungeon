using System;
using System.Configuration;

namespace DungeonGame.Properties
{
    public static class Settings
    {
        public static SettingsData Default { get; } = new SettingsData();
    }

    public class SettingsData
    {
        public bool AutoUpdateEnabled { get; set; } = true;
        public string SkippedVersion { get; set; } = string.Empty;
        public DateTime LastUpdateCheck { get; set; } = DateTime.MinValue;
        public int UpdateCheckInterval { get; set; } = 24; // hours

        public void Save()
        {
            // Placeholder for settings persistence
            // Could be implemented with file or registry storage
        }
    }
}
