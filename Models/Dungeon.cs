using System;
using System.Collections.Generic;
using System.Linq;

namespace DungeonGameWpf.Models
{
    public class Dungeon
    {
        public int[,] Grid { get; }
        public GameTile[,]? AdvancedGrid { get; set; }
        public int M => Grid.GetLength(0);
        public int N => Grid.GetLength(1);
        public int this[int i, int j] => Grid[i, j];

        // New properties for enhanced functionality
        public string DifficultyLevel { get; set; } = "Normal";
        public Dictionary<string, object> Metadata { get; set; } = new();
        public List<(int row, int col)> SpecialTiles { get; set; } = new();
        public List<(int row, int col)> SafeZones { get; set; } = new();

        public Dungeon(int[,] grid)
        {
            Grid = grid;
            InitializeMetadata();
        }

        public Dungeon(GameTile[,] advancedGrid)
        {
            AdvancedGrid = advancedGrid;
            // Convert to simple grid for compatibility
            Grid = new int[advancedGrid.GetLength(0), advancedGrid.GetLength(1)];
            for (int i = 0; i < M; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    Grid[i, j] = advancedGrid[i, j].Value;
                }
            }
            InitializeMetadata();
        }

        private void InitializeMetadata()
        {
            AnalyzeGrid();
            FindSpecialTiles();
            IdentifySafeZones();
        }

        private void AnalyzeGrid()
        {
            int totalValue = 0;
            int positiveCount = 0;
            int negativeCount = 0;
            int minValue = int.MaxValue;
            int maxValue = int.MinValue;

            for (int i = 0; i < M; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    int value = Grid[i, j];
                    totalValue += value;
                    if (value > 0) positiveCount++;
                    if (value < 0) negativeCount++;
                    minValue = Math.Min(minValue, value);
                    maxValue = Math.Max(maxValue, value);
                }
            }

            Metadata["TotalValue"] = totalValue;
            Metadata["AverageValue"] = (double)totalValue / (M * N);
            Metadata["PositiveCount"] = positiveCount;
            Metadata["NegativeCount"] = negativeCount;
            Metadata["MinValue"] = minValue;
            Metadata["MaxValue"] = maxValue;
            Metadata["Difficulty"] = CalculateDifficultyScore();
        }

        private double CalculateDifficultyScore()
        {
            var avgValue = (double)Metadata["AverageValue"];
            var negativeRatio = (int)Metadata["NegativeCount"] / (double)(M * N);
            var valueRange = (int)Metadata["MaxValue"] - (int)Metadata["MinValue"];

            // Higher score means more difficult
            return negativeRatio * 50 + (valueRange / 10.0) - avgValue;
        }

        private void FindSpecialTiles()
        {
            SpecialTiles.Clear();

            if (AdvancedGrid != null)
            {
                for (int i = 0; i < M; i++)
                {
                    for (int j = 0; j < N; j++)
                    {
                        if (AdvancedGrid[i, j].Type != TileType.Normal)
                        {
                            SpecialTiles.Add((i, j));
                        }
                    }
                }
            }
            else
            {
                // For simple grid, identify special based on values
                for (int i = 0; i < M; i++)
                {
                    for (int j = 0; j < N; j++)
                    {
                        int value = Grid[i, j];
                        // Consider extreme values as special
                        if (Math.Abs(value) > 20 || value == 0)
                        {
                            SpecialTiles.Add((i, j));
                        }
                    }
                }
            }
        }

        private void IdentifySafeZones()
        {
            SafeZones.Clear();

            // Safe zones are areas with positive or zero values
            for (int i = 0; i < M; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    if (Grid[i, j] >= 0)
                    {
                        SafeZones.Add((i, j));
                    }
                }
            }
        }

        public static Dungeon CreateDefault() => new Dungeon(new int[,] {
            { -2, -3,  3,  8, -5 },
            { -5,-10,  1, -2, 12 },
            { 10, 30, -5, -8,  3 },
            { -1,  5,-15, 20, -3 },
            {  7, -4,  2, -1, 10 }
        });

        public static Dungeon CreateByDifficulty(string difficulty, int size = 5)
        {
            var settings = GameConfig.DifficultySettings.ContainsKey(difficulty)
                ? GameConfig.DifficultySettings[difficulty]
                : GameConfig.DifficultySettings[GameConfig.Difficulty.Normal];

            return GenerateBalanced(size, size, settings.min, settings.max, difficulty);
        }

        public static Dungeon GenerateRandom(int m, int n, int vmin, int vmax)
        {
            var rng = new Random();
            var g = new int[m, n];
            for (int i = 0; i < m; i++)
                for (int j = 0; j < n; j++)
                    g[i, j] = rng.Next(vmin, vmax + 1);
            return new Dungeon(g);
        }

        public static Dungeon GenerateBalanced(int m, int n, int vmin, int vmax, string difficulty = "Normal")
        {
            var rng = new Random();
            var g = new int[m, n];
            // var targetSum = 0; // We want the grid to be roughly balanced (unused for now)

            // Generate with some structure
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    double distanceFromStart = Math.Sqrt(i * i + j * j);
                    double distanceFromEnd = Math.Sqrt((m - 1 - i) * (m - 1 - i) + (n - 1 - j) * (n - 1 - j));

                    // Make it harder in the middle, easier near start and end
                    double difficulty_factor = 1.0 - Math.Min(distanceFromStart, distanceFromEnd) / Math.Max(m, n);

                    int minVal = (int)(vmin * (0.5 + difficulty_factor * 0.5));
                    int maxVal = (int)(vmax * (0.5 + difficulty_factor * 0.5));

                    g[i, j] = rng.Next(minVal, maxVal + 1);
                }
            }

            // Ensure start is safe and end is rewarding
            g[0, 0] = Math.Max(0, g[0, 0]);
            g[m - 1, n - 1] = Math.Max(10, g[m - 1, n - 1]);

            var dungeon = new Dungeon(g) { DifficultyLevel = difficulty };
            return dungeon;
        }

        public static Dungeon GenerateAdvanced(GameConfig config)
        {
            var engine = new Services.GameEngine();
            engine.StartNewGame(config);
            return new Dungeon(engine.CurrentGame.Grid);
        }

        // Path finding helpers
        public List<(int row, int col)> GetValidMoves(int row, int col, bool allowDiagonal = false)
        {
            var moves = new List<(int, int)>();
            var directions = allowDiagonal
                ? new[] { (-1, -1), (-1, 0), (-1, 1), (0, -1), (0, 1), (1, -1), (1, 0), (1, 1) }
                : new[] { (-1, 0), (1, 0), (0, -1), (0, 1) };

            foreach (var (dr, dc) in directions)
            {
                int newRow = row + dr;
                int newCol = col + dc;

                if (newRow >= 0 && newRow < M && newCol >= 0 && newCol < N)
                {
                    moves.Add((newRow, newCol));
                }
            }

            return moves;
        }

        public bool IsPositionSafe(int row, int col, int minHealth)
        {
            if (row < 0 || row >= M || col < 0 || col >= N) return false;
            return Grid[row, col] + minHealth > 0;
        }

        public List<(int row, int col)> FindPath(int startRow, int startCol, int endRow, int endCol, int initialHealth)
        {
            // Simple A* pathfinding considering health
            var openSet = new List<(int row, int col, int health, List<(int, int)> path)>();
            var closedSet = new HashSet<(int, int)>();

            openSet.Add((startRow, startCol, initialHealth, new List<(int, int)> { (startRow, startCol) }));

            while (openSet.Count > 0)
            {
                // Find the path with best score (health + distance to goal)
                var current = openSet.OrderBy(x => -x.health + Math.Abs(x.row - endRow) + Math.Abs(x.col - endCol)).First();
                openSet.Remove(current);

                if (current.row == endRow && current.col == endCol && current.health > 0)
                {
                    return current.path;
                }

                if (closedSet.Contains((current.row, current.col))) continue;
                closedSet.Add((current.row, current.col));

                foreach (var (newRow, newCol) in GetValidMoves(current.row, current.col))
                {
                    if (closedSet.Contains((newRow, newCol))) continue;

                    int newHealth = current.health + Grid[newRow, newCol];
                    if (newHealth <= 0 && !(newRow == endRow && newCol == endCol)) continue;

                    var newPath = new List<(int, int)>(current.path) { (newRow, newCol) };
                    openSet.Add((newRow, newCol, newHealth, newPath));
                }
            }

            return new List<(int, int)>(); // No path found
        }

        public double GetTileValueDistribution()
        {
            // Returns variance as measure of distribution
            var avg = (double)Metadata["AverageValue"];
            double variance = 0;

            for (int i = 0; i < M; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    variance += Math.Pow(Grid[i, j] - avg, 2);
                }
            }

            return variance / (M * N);
        }

        public (int minHP, List<(int row, int col)> optimalPath) GetOptimalSolution()
        {
            var dp = DpSolver.Build(Grid);
            var minHP = dp[0, 0];

            // Reconstruct optimal path
            var path = new List<(int, int)> { (0, 0) };
            int currentRow = 0, currentCol = 0;

            while (currentRow < M - 1 || currentCol < N - 1)
            {
                // Choose the direction that requires less HP
                int rightHP = currentCol + 1 < N ? dp[currentRow, currentCol + 1] : int.MaxValue;
                int downHP = currentRow + 1 < M ? dp[currentRow + 1, currentCol] : int.MaxValue;

                if (rightHP <= downHP && currentCol + 1 < N)
                {
                    currentCol++;
                }
                else if (currentRow + 1 < M)
                {
                    currentRow++;
                }

                path.Add((currentRow, currentCol));
            }

            return (minHP, path);
        }

        public string GetDifficultyRating()
        {
            var score = (double)Metadata["Difficulty"];

            return score switch
            {
                < 10 => "Trivial",
                < 20 => "Easy",
                < 35 => "Normal",
                < 50 => "Hard",
                < 75 => "Expert",
                _ => "Nightmare"
            };
        }

        public void PrintAnalysis()
        {
            Console.WriteLine($"Dungeon Analysis ({M}x{N}):");
            Console.WriteLine($"Difficulty: {GetDifficultyRating()}");
            Console.WriteLine($"Average Value: {Metadata["AverageValue"]:F2}");
            Console.WriteLine($"Positive Tiles: {Metadata["PositiveCount"]}/{M * N}");
            Console.WriteLine($"Special Tiles: {SpecialTiles.Count}");
            Console.WriteLine($"Safe Zones: {SafeZones.Count}");

            var (minHP, optimalPath) = GetOptimalSolution();
            Console.WriteLine($"Minimum Required HP: {minHP}");
            Console.WriteLine($"Optimal Path Length: {optimalPath.Count}");
        }
    }
}
