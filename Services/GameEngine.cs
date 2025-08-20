using DungeonGameWpf.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DungeonGameWpf.Services
{
    public class GameEngine
    {
        public GameState CurrentGame { get; private set; } = null!;
        public event Action<GameState>? GameStateChanged;
        public event Action<Player>? PlayerMoved;
        public event Action<string>? GameMessage;

        private Random _random = new();

        public void StartNewGame(GameConfig config)
        {
            CurrentGame = new GameState
            {
                Config = config,
                StartTime = DateTime.Now,
                Players = new List<Player>
                {
                    new Player { Name = "Player 1", Health = GetInitialHealth(config.DifficultyLevel) }
                }
            };

            if (config.EnableMultiplayer)
            {
                CurrentGame.Players.Add(new Player { Name = "Player 2", Health = GetInitialHealth(config.DifficultyLevel) });
            }

            GenerateGrid(config);
            GameStateChanged?.Invoke(CurrentGame);
        }

        private void GenerateGrid(GameConfig config)
        {
            var size = config.GridSize;
            CurrentGame.Grid = new GameTile[size, size];
            var settings = GameConfig.DifficultySettings[config.DifficultyLevel];

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    var tile = new GameTile();

                    // Starting position is always safe
                    if (i == 0 && j == 0)
                    {
                        tile.Value = 0;
                        tile.Type = TileType.Normal;
                        tile.IsRevealed = true;
                    }
                    // Goal position
                    else if (i == size - 1 && j == size - 1)
                    {
                        tile.Value = 50; // Big reward for reaching goal
                        tile.Type = TileType.Normal;
                    }
                    else
                    {
                        // Generate tile based on difficulty
                        double rand = _random.NextDouble();

                        if (rand < settings.trapChance)
                        {
                            tile.Type = TileType.Trap;
                            tile.Value = _random.Next(settings.min, -1);
                            tile.SpecialEffect = GetRandomTrapEffect();
                        }
                        else if (rand < settings.trapChance + settings.powerUpChance)
                        {
                            tile.Type = TileType.PowerUp;
                            tile.Value = _random.Next(1, settings.max / 2);
                            tile.SpecialEffect = GetRandomPowerUp();
                        }
                        else if (config.EnableSpecialTiles && rand < settings.trapChance + settings.powerUpChance + 0.1)
                        {
                            tile.Type = GetRandomSpecialTile();
                            tile.Value = _random.Next(settings.min / 2, settings.max / 2);
                        }
                        else
                        {
                            tile.Type = TileType.Normal;
                            tile.Value = _random.Next(settings.min, settings.max);
                        }
                    }

                    CurrentGame.Grid[i, j] = tile;
                }
            }

            // Add some special features
            if (config.EnableSpecialTiles)
            {
                AddSpecialFeatures(size);
            }
        }

        private void AddSpecialFeatures(int size)
        {
            // Add teleporter pair
            var teleporter1 = GetRandomEmptyPosition(size);
            var teleporter2 = GetRandomEmptyPosition(size);

            CurrentGame.Grid[teleporter1.row, teleporter1.col].Type = TileType.Teleporter;
            CurrentGame.Grid[teleporter1.row, teleporter1.col].Properties["TargetRow"] = teleporter2.row;
            CurrentGame.Grid[teleporter1.row, teleporter1.col].Properties["TargetCol"] = teleporter2.col;

            CurrentGame.Grid[teleporter2.row, teleporter2.col].Type = TileType.Teleporter;
            CurrentGame.Grid[teleporter2.row, teleporter2.col].Properties["TargetRow"] = teleporter1.row;
            CurrentGame.Grid[teleporter2.row, teleporter2.col].Properties["TargetCol"] = teleporter1.col;

            // Add key and locked door
            var keyPos = GetRandomEmptyPosition(size);
            var doorPos = GetRandomEmptyPosition(size);

            CurrentGame.Grid[keyPos.row, keyPos.col].Type = TileType.KeyTile;
            CurrentGame.Grid[doorPos.row, doorPos.col].Type = TileType.LockedDoor;
            CurrentGame.Grid[doorPos.row, doorPos.col].Value = -999; // Cannot pass without key

            // Add checkpoint
            var checkpointPos = GetRandomEmptyPosition(size);
            CurrentGame.Grid[checkpointPos.row, checkpointPos.col].Type = TileType.Checkpoint;
        }

        private (int row, int col) GetRandomEmptyPosition(int size)
        {
            int row, col;
            do
            {
                row = _random.Next(1, size - 1); // Avoid start and end
                col = _random.Next(1, size - 1);
            } while (CurrentGame.Grid[row, col].Type != TileType.Normal);

            return (row, col);
        }

        private string GetRandomTrapEffect()
        {
            var effects = new[] { "Poison", "Freeze", "Confusion", "Weakness", "Bleed" };
            return effects[_random.Next(effects.Length)];
        }

        private string GetRandomPowerUp()
        {
            var powerUps = new[] { "Shield", "HealthBoost", "Flight", "Vision", "Strength", "Speed" };
            return powerUps[_random.Next(powerUps.Length)];
        }

        private TileType GetRandomSpecialTile()
        {
            var types = new[] { TileType.MovingPlatform, TileType.BossTile, TileType.Checkpoint };
            return types[_random.Next(types.Length)];
        }

        private int GetInitialHealth(string difficulty)
        {
            return difficulty switch
            {
                GameConfig.Difficulty.Easy => 150,
                GameConfig.Difficulty.Normal => 100,
                GameConfig.Difficulty.Hard => 75,
                GameConfig.Difficulty.Nightmare => 50,
                _ => 100
            };
        }

        public bool MovePlayer(int playerId, int deltaRow, int deltaCol)
        {
            if (CurrentGame.IsGameOver) return false;

            var player = CurrentGame.Players[playerId];
            var newRow = player.Row + deltaRow;
            var newCol = player.Col + deltaCol;
            var size = CurrentGame.Config.GridSize;

            // Check boundaries
            if (newRow < 0 || newRow >= size || newCol < 0 || newCol >= size)
            {
                return false;
            }

            // Special flight ability bypasses some restrictions
            if (!player.CanFly)
            {
                // Classic game only allows right and down
                if (CurrentGame.Config.GameMode == GameConfig.GameType.Classic)
                {
                    if (deltaRow < 0 || deltaCol < 0) return false;
                }
            }

            var tile = CurrentGame.Grid[newRow, newCol];

            // Check if door is locked
            if (tile.Type == TileType.LockedDoor && player.Keys == 0)
            {
                GameMessage?.Invoke("You need a key to pass through this door!");
                return false;
            }

            // Move player
            player.Row = newRow;
            player.Col = newCol;
            tile.IsVisited = true;
            tile.IsRevealed = true;

            // Process tile effects
            ProcessTileEffect(player, tile);

            // Check win condition
            if (newRow == size - 1 && newCol == size - 1)
            {
                CurrentGame.IsWon = true;
                CurrentGame.IsGameOver = true;
                player.Score += CalculateBonusScore(player);
                GameMessage?.Invoke($"{player.Name} reached the goal! Final Score: {player.Score}");
            }

            // Check lose condition
            if (player.Health <= 0)
            {
                player.Lives--;
                if (player.Lives > 0)
                {
                    // Respawn at last checkpoint or start
                    var checkpoint = FindLastCheckpoint(player);
                    player.Row = checkpoint.row;
                    player.Col = checkpoint.col;
                    player.Health = player.MaxHealth / 2;
                    GameMessage?.Invoke($"{player.Name} respawned at checkpoint!");
                }
                else
                {
                    CurrentGame.IsGameOver = true;
                    CurrentGame.GameOverReason = $"{player.Name} ran out of health!";
                }
            }

            // Update elapsed time
            CurrentGame.ElapsedTime = DateTime.Now - CurrentGame.StartTime;

            // Check time limit for time attack mode
            if (CurrentGame.Config.GameMode == GameConfig.GameType.TimeAttack)
            {
                if (CurrentGame.ElapsedTime.TotalSeconds > CurrentGame.Config.TimeLimit)
                {
                    CurrentGame.IsGameOver = true;
                    CurrentGame.GameOverReason = "Time's up!";
                }
            }

            PlayerMoved?.Invoke(player);
            GameStateChanged?.Invoke(CurrentGame);
            return true;
        }

        private void ProcessTileEffect(Player player, GameTile tile)
        {
            // Apply health change
            player.Health += tile.Value;
            player.Score += Math.Max(0, tile.Value);

            // Process special tile effects
            switch (tile.Type)
            {
                case TileType.Trap:
                    ApplyTrapEffect(player, tile.SpecialEffect);
                    GameMessage?.Invoke($"You stepped on a {tile.SpecialEffect} trap!");
                    break;

                case TileType.PowerUp:
                    ApplyPowerUp(player, tile.SpecialEffect);
                    GameMessage?.Invoke($"You found a {tile.SpecialEffect} power-up!");
                    break;

                case TileType.KeyTile:
                    player.Keys++;
                    GameMessage?.Invoke("You found a key!");
                    break;

                case TileType.LockedDoor:
                    if (player.Keys > 0)
                    {
                        player.Keys--;
                        GameMessage?.Invoke("You used a key to unlock the door!");
                    }
                    break;

                case TileType.Teleporter:
                    if (tile.Properties.ContainsKey("TargetRow"))
                    {
                        player.Row = (int)tile.Properties["TargetRow"];
                        player.Col = (int)tile.Properties["TargetCol"];
                        GameMessage?.Invoke("You were teleported!");
                    }
                    break;

                case TileType.Checkpoint:
                    player.Stats["LastCheckpointRow"] = player.Row;
                    player.Stats["LastCheckpointCol"] = player.Col;
                    GameMessage?.Invoke("Checkpoint saved!");
                    break;
            }

            // Update power-up durations
            UpdatePowerUpDurations(player);
        }

        private void ApplyTrapEffect(Player player, string effect)
        {
            switch (effect)
            {
                case "Poison":
                    player.Health -= 20;
                    break;
                case "Freeze":
                    // Skip next turn (implement in UI)
                    break;
                case "Confusion":
                    // Reverse controls (implement in UI)
                    break;
                case "Weakness":
                    player.MaxHealth = Math.Max(10, player.MaxHealth - 10);
                    break;
                case "Bleed":
                    player.Health -= player.MaxHealth / 10;
                    break;
            }
        }

        private void ApplyPowerUp(Player player, string powerUp)
        {
            switch (powerUp)
            {
                case "Shield":
                    player.HasShield = true;
                    player.ShieldDuration = 5;
                    break;
                case "HealthBoost":
                    player.MaxHealth += 20;
                    player.Health += 20;
                    break;
                case "Flight":
                    player.CanFly = true;
                    player.FlyDuration = 3;
                    break;
                case "Vision":
                    RevealNearbyTiles(player.Row, player.Col, 2);
                    break;
                case "Strength":
                    player.Stats["StrengthBonus"] = 10;
                    break;
                case "Speed":
                    player.Stats["SpeedBonus"] = 3;
                    break;
            }

            if (!player.PowerUps.Contains(powerUp))
            {
                player.PowerUps.Add(powerUp);
            }
        }

        private void RevealNearbyTiles(int centerRow, int centerCol, int radius)
        {
            var size = CurrentGame.Config.GridSize;
            for (int i = Math.Max(0, centerRow - radius); i <= Math.Min(size - 1, centerRow + radius); i++)
            {
                for (int j = Math.Max(0, centerCol - radius); j <= Math.Min(size - 1, centerCol + radius); j++)
                {
                    CurrentGame.Grid[i, j].IsRevealed = true;
                }
            }
        }

        private void UpdatePowerUpDurations(Player player)
        {
            if (player.ShieldDuration > 0)
            {
                player.ShieldDuration--;
                if (player.ShieldDuration == 0)
                {
                    player.HasShield = false;
                    GameMessage?.Invoke("Shield has worn off!");
                }
            }

            if (player.FlyDuration > 0)
            {
                player.FlyDuration--;
                if (player.FlyDuration == 0)
                {
                    player.CanFly = false;
                    GameMessage?.Invoke("Flight has worn off!");
                }
            }
        }

        private (int row, int col) FindLastCheckpoint(Player player)
        {
            if (player.Stats.ContainsKey("LastCheckpointRow"))
            {
                return (player.Stats["LastCheckpointRow"], player.Stats["LastCheckpointCol"]);
            }
            return (0, 0); // Return to start
        }

        private int CalculateBonusScore(Player player)
        {
            var bonus = 0;
            bonus += player.Health * 2; // Health bonus
            bonus += player.PowerUps.Count * 50; // Power-up bonus
            bonus += Math.Max(0, CurrentGame.Config.TimeLimit - (int)CurrentGame.ElapsedTime.TotalSeconds) * 10; // Time bonus
            return bonus;
        }

        public void RevealTile(int row, int col)
        {
            if (row >= 0 && row < CurrentGame.Config.GridSize &&
                col >= 0 && col < CurrentGame.Config.GridSize)
            {
                CurrentGame.Grid[row, col].IsRevealed = true;
            }
        }

        public GameTile GetTile(int row, int col)
        {
            return CurrentGame.Grid[row, col];
        }

        public Player GetCurrentPlayer()
        {
            return CurrentGame.Players[CurrentGame.CurrentPlayerIndex];
        }

        public void SwitchToNextPlayer()
        {
            if (CurrentGame.Players.Count > 1)
            {
                CurrentGame.CurrentPlayerIndex = (CurrentGame.CurrentPlayerIndex + 1) % CurrentGame.Players.Count;
            }
        }
    }
}
