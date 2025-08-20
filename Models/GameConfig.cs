using System;
using System.Collections.Generic;

namespace DungeonGameWpf.Models
{
    public class GameConfig
    {
        public static class Difficulty
        {
            public const string Easy = "Easy";
            public const string Normal = "Normal";
            public const string Hard = "Hard";
            public const string Nightmare = "Nightmare";
        }

        public static class GameType
        {
            public const string Classic = "Classic";
            public const string Survival = "Survival";
            public const string TimeAttack = "Time Attack";
            public const string Adventure = "Adventure";
        }

        public string DifficultyLevel { get; set; } = Difficulty.Normal;
        public string GameMode { get; set; } = GameType.Classic;
        public int GridSize { get; set; } = 5;
        public bool EnablePowerUps { get; set; } = true;
        public bool EnableSpecialTiles { get; set; } = true;
        public bool EnableMultiplayer { get; set; } = false;
        public int TimeLimit { get; set; } = 300; // seconds for time attack
        public int Lives { get; set; } = 3;

        // AI Configuration - Static properties for global access
        public static string CurrentAIDifficulty { get; set; } = "Intermediate";
        public static int AIThinkingDelay { get; set; } = 1200; // milliseconds

        public static Dictionary<string, (int min, int max, double trapChance, double powerUpChance)> DifficultySettings = new()
        {
            [Difficulty.Easy] = (-5, 15, 0.3, 0.4),
            [Difficulty.Normal] = (-10, 20, 0.4, 0.3),
            [Difficulty.Hard] = (-20, 25, 0.5, 0.2),
            [Difficulty.Nightmare] = (-30, 30, 0.6, 0.1)
        };
    }

    public enum TileType
    {
        Normal,
        Trap,
        PowerUp,
        Teleporter,
        MovingPlatform,
        KeyTile,
        LockedDoor,
        BossTile,
        Checkpoint
    }

    public class GameTile
    {
        public int Value { get; set; }
        public TileType Type { get; set; } = TileType.Normal;
        public Dictionary<string, object> Properties { get; set; } = new();
        public bool IsRevealed { get; set; } = false;
        public bool IsVisited { get; set; } = false;

        // Special effects
        public string SpecialEffect { get; set; } = "";
        public int EffectDuration { get; set; } = 0;
    }

    public class Player
    {
        public string Name { get; set; } = "Player";
        public int Health { get; set; } = 100;
        public int MaxHealth { get; set; } = 100;
        public int Row { get; set; } = 0;
        public int Col { get; set; } = 0;
        public int Score { get; set; } = 0;
        public int Keys { get; set; } = 0;
        public int Lives { get; set; } = 3;
        public List<string> PowerUps { get; set; } = new();
        public Dictionary<string, int> Stats { get; set; } = new();
        public bool HasShield { get; set; } = false;
        public int ShieldDuration { get; set; } = 0;
        public bool CanFly { get; set; } = false;
        public int FlyDuration { get; set; } = 0;
    }

    public class GameState
    {
        public GameTile[,] Grid { get; set; } = null!;
        public List<Player> Players { get; set; } = new();
        public int CurrentPlayerIndex { get; set; } = 0;
        public DateTime StartTime { get; set; }
        public TimeSpan ElapsedTime { get; set; }
        public GameConfig Config { get; set; } = new();
        public bool IsGameOver { get; set; } = false;
        public bool IsWon { get; set; } = false;
        public string GameOverReason { get; set; } = "";
        public Dictionary<string, object> GameData { get; set; } = new();
    }
}
