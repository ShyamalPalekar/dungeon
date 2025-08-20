using DungeonGameWpf.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DungeonGameWpf.AI
{
    public class QLearningAgent
    {
        // Singleton sederhana agar learning berkelanjutan di seluruh sesi
        public static QLearningAgent Shared { get; } = new QLearningAgent();

        public enum Move { Right = 0, Down = 1 }

        // Hyperparams
        public double Alpha { get; set; } = 0.2;   // learning rate
        public double Gamma { get; set; } = 0.95;  // discount
        public double Epsilon { get; set; } = 0.15; // exploration

        private readonly Dictionary<(int m, int n, int r, int c), double[]> _Q = new();
        private readonly Random _rnd = new();

        // Stats
        public int Episodes { get; private set; } = 0;
        public double Accuracy { get; private set; } = 0; // fraction of optimal actions
        public double LastLoss { get; private set; } = 0;

        double[] Q((int m, int n, int r, int c) s)
        {
            if (!_Q.TryGetValue(s, out var arr))
            {
                arr = new double[2]; // Right, Down
                _Q[s] = arr;
            }
            return arr;
        }

        public Move ChooseMove(Dungeon d, int r, int c, int[,] dp)
        {
            var state = (d.M, d.N, r, c);

            // Apply difficulty-based decision making
            double explorationRate = Epsilon;

            // Adjust exploration based on current performance vs optimal path
            if (dp != null)
            {
                int currentMinPath = dp[r, c];
                // If we're behind optimal path, increase exploration for lower difficulties
                if (Models.GameConfig.CurrentAIDifficulty == "Beginner")
                {
                    explorationRate *= 1.5; // 60% more exploration
                }
                else if (Models.GameConfig.CurrentAIDifficulty == "Master")
                {
                    explorationRate *= 0.5; // 50% less exploration - more focused
                }
            }

            // epsilon-greedy with difficulty adjustment
            if (_rnd.NextDouble() < explorationRate)
            {
                // For higher difficulties, even random moves are more strategic
                if (Models.GameConfig.CurrentAIDifficulty == "Master" || Models.GameConfig.CurrentAIDifficulty == "Expert")
                {
                    // Smart random: prefer moves that lead to better positions
                    return ChooseSmartRandomMove(d, r, c, dp);
                }
                return (Move)_rnd.Next(0, 2);
            }

            var q = Q(state);

            // greedy w.r.t Q, with difficulty-based tie-breaking
            if (Math.Abs(q[0] - q[1]) < 1e-6)
            {
                return BreakTieWithStrategy(d, r, c, dp);
            }

            return q[0] >= q[1] ? Move.Right : Move.Down;
        }

        private Move ChooseSmartRandomMove(Dungeon d, int r, int c, int[,] dp)
        {
            List<Move> validMoves = new List<Move>();
            List<double> moveValues = new List<double>();

            // Right move
            if (c + 1 < d.N)
            {
                validMoves.Add(Move.Right);
                double value = d[r, c + 1];
                if (dp != null) value += (100.0 - dp[r, c + 1]); // Prefer shorter paths
                moveValues.Add(value);
            }

            // Down move
            if (r + 1 < d.M)
            {
                validMoves.Add(Move.Down);
                double value = d[r + 1, c];
                if (dp != null) value += (100.0 - dp[r + 1, c]); // Prefer shorter paths
                moveValues.Add(value);
            }

            if (validMoves.Count == 0) return Move.Right; // Fallback
            if (validMoves.Count == 1) return validMoves[0];

            // Weighted random selection based on values
            double totalValue = moveValues.Sum();
            if (totalValue <= 0)
            {
                return validMoves[_rnd.Next(validMoves.Count)];
            }

            double rand = _rnd.NextDouble() * totalValue;
            double cumulative = 0;
            for (int i = 0; i < validMoves.Count; i++)
            {
                cumulative += Math.Max(0, moveValues[i]);
                if (rand <= cumulative)
                {
                    return validMoves[i];
                }
            }

            return validMoves.Last();
        }

        private Move BreakTieWithStrategy(Dungeon d, int r, int c, int[,] dp)
        {
            int needR = c + 1 < d.N ? dp[r, c + 1] : int.MaxValue;
            int needD = r + 1 < d.M ? dp[r + 1, c] : int.MaxValue;

            // Different strategies based on difficulty
            switch (Models.GameConfig.CurrentAIDifficulty)
            {
                case "Beginner":
                    // Sometimes make suboptimal choices
                    if (_rnd.NextDouble() < 0.3) // 30% chance of suboptimal
                    {
                        return needR <= needD ? Move.Down : Move.Right; // Opposite of optimal
                    }
                    return needR <= needD ? Move.Right : Move.Down;

                case "Expert":
                case "Master":
                    // Consider cell values too, not just path length
                    if (needR == needD)
                    {
                        double rightValue = c + 1 < d.N ? d[r, c + 1] : double.MinValue;
                        double downValue = r + 1 < d.M ? d[r + 1, c] : double.MinValue;
                        return rightValue >= downValue ? Move.Right : Move.Down;
                    }
                    return needR <= needD ? Move.Right : Move.Down;

                default: // Intermediate
                    return needR <= needD ? Move.Right : Move.Down;
            }
        }

        public double TrainEpisode(Dungeon d, int[,] dp)
        {
            // reward desain: masuk sel -> reward = cellValue; goal bonus +50
            int r = 0, c = 0;
            int steps = 0;
            double lossSum = 0;
            int optimalCount = 0, taken = 0;

            // initial HP minimal (untuk evaluasi optimal step)
            int minHp = dp[0, 0];
            int hp = minHp + d[0, 0];

            while (true)
            {
                var state = (d.M, d.N, r, c);
                var action = ChooseMove(d, r, c, dp);
                int nr = r + (action == Move.Down ? 1 : 0);
                int nc = c + (action == Move.Right ? 1 : 0);

                if (nr >= d.M || nc >= d.N)
                {
                    // illegal move -> strong negative reward
                    UpdateQ(state, (int)action, -25, state, terminal: false);
                    lossSum += 25;
                    break;
                }

                // reward = nilai cell yang dimasuki
                double reward = d[nr, nc];

                hp += d[nr, nc];
                bool dead = hp <= 0;

                bool terminal = (nr == d.M - 1 && nc == d.N - 1) || dead;
                if (nr == d.M - 1 && nc == d.N - 1) reward += 50; // goal bonus
                if (dead) reward -= 50; // died penalty

                var nextState = (d.M, d.N, nr, nc);
                double tdErr = UpdateQ(state, (int)action, reward, nextState, terminal);
                lossSum += Math.Abs(tdErr);

                // akurasi = bandingkan aksi dengan arah dp optimal (butuh HP minimal)
                if (!terminal)
                {
                    int needR = nc + 1 < d.N ? dp[nr, nc + 1] : int.MaxValue;
                    int needD = nr + 1 < d.M ? dp[nr + 1, nc] : int.MaxValue;
                    var optimal = needR <= needD ? Move.Right : Move.Down;
                    if (optimal == action) optimalCount++;
                    taken++;
                }

                r = nr; c = nc; steps++;

                if (terminal || steps > d.M * d.N + 10) break;
            }

            Episodes++;
            LastLoss = lossSum / Math.Max(1, steps);
            if (taken > 0)
                Accuracy = 0.95 * Accuracy + 0.05 * (optimalCount / (double)taken); // moving average

            return LastLoss;
        }

        double UpdateQ((int m, int n, int r, int c) s, int a, double reward, (int m, int n, int r, int c) ns, bool terminal)
        {
            var q = Q(s);
            double target = reward;
            if (!terminal)
            {
                var nq = Q(ns);
                target = reward + Gamma * Math.Max(nq[0], nq[1]);
            }
            double tdErr = target - q[a];
            q[a] += Alpha * tdErr;
            return tdErr;
        }
    }
}
