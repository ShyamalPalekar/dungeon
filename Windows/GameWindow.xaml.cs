using DungeonGameWpf.AI;
using DungeonGameWpf.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace DungeonGameWpf.Windows
{
    public partial class GameWindow : Window
    {
        public enum GameMode { SingleVsAI, LocalMultiplayer }

        private GameMode _mode;
        private Dungeon _dungeon;

        // Player positions
        private int _playerR, _playerC; // Human player position
        private int _aiR, _aiC;        // AI player position

        // Player stats
        private int _playerHp, _aiHp;
        private int _playerScore, _aiScore;

        private List<(int r, int c)> _playerPath = new();
        private List<(int r, int c)> _aiPath = new();

        private string _p1 = "You", _p2 = "AI";
        private bool _isPlayerTurn = true; // True = Player turn, False = AI turn

        private int[,] _dp = null!;
        private QLearningAgent _agent = null!;
        private DispatcherTimer _aiThinkingTimer = null!;
        private DispatcherTimer _aiMoveTimer = null!;
        private List<string> _aiReasoningSteps = null!;
        private int _reasoningStep = 0;

        public GameWindow(GameMode mode)
            : this(mode, Dungeon.CreateDefault(), "You", "AI") { }

        public GameWindow(GameMode mode, Dungeon dungeon, string p1 = "You", string p2 = "AI")
        {
            InitializeComponent();
            _mode = mode;
            _dungeon = dungeon;
            _p1 = p1; _p2 = p2;
            _agent = QLearningAgent.Shared; // singleton sederhana untuk continous learning
            InitGame();
        }

        void InitGame()
        {
            _dp = DpSolver.Build(_dungeon.Grid);
            var minHp = _dp[0, 0];

            // Initialize both players at start position
            _playerR = 0; _playerC = 0; _playerPath.Clear();
            _aiR = 0; _aiC = 0; _aiPath.Clear();

            _playerPath.Add((_playerR, _playerC));
            _aiPath.Add((_aiR, _aiC));

            // Both players start with calculated minimum HP
            _playerHp = Math.Max(minHp, 1);
            _aiHp = Math.Max(minHp, 1);

            // Apply start cell bonus to both
            _playerHp += _dungeon[_playerR, _playerC];
            _aiHp += _dungeon[_aiR, _aiC];

            _playerScore = 0;
            _aiScore = 0;
            _isPlayerTurn = true; // Player goes first

            TxtMode.Text = _mode == GameMode.SingleVsAI ? "🤖 You vs AI Battle" : "👥 Local Multiplayer";

            // Initialize AI reasoning
            InitializeAIReasoning();

            SetupBoard();
            UpdateDisplay();

            // Show game rules and winning strategy
            ShowGameRules();

            ShowAIReasoning($"🎯 Battle initiated! Playing on {GameConfig.CurrentAIDifficulty} difficulty...");
        }

        void ShowGameRules()
        {
            // Check if user wants to see rules (can be disabled in settings)
            var result = MessageBox.Show(
                "Would you like to see the Game Rules & Strategy guide before starting?",
                "Game Tutorial",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.No) return;

            string rules = @"🏆 HOW TO WIN:

🎯 OBJECTIVE: Reach the finish line (🏁) before the AI!

📋 GAME RULES:
• Take turns moving RIGHT ➡️ or DOWN ⬇️
• Numbers on tiles = HP change (+ or -)
• Negative tiles damage you, positive tiles heal
• If HP reaches 0 or below, you lose!

🧠 STRATEGY TIPS:
• Plan your path to avoid death traps
• Collect positive tiles for extra HP
• Watch AI patterns and adapt
• Speed matters - be decisive!

🎮 CONTROLS:
• YOUR TURN: Use Right/Down buttons
• AI TURN: Wait for AI to move (automatic)

🎲 DIFFICULTY LEVELS:
• Beginner: AI makes mistakes, slower thinking
• Intermediate: Balanced AI, good challenge
• Expert: Smart AI, fast decisions
• Master: Very challenging, almost optimal play

Good luck beating the AI! 🚀

Click OK to start the battle!";

            var mainWindow = Application.Current.MainWindow;
            if (mainWindow != null)
            {
                // Make sure the message box appears on top
                this.Topmost = true;
                MessageBox.Show(this, rules, "Game Rules & Strategy", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Topmost = false;
            }
            else
            {
                MessageBox.Show(rules, "Game Rules & Strategy", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        void InitializeAIReasoning()
        {
            _aiReasoningSteps = new List<string>
            {
                "🔍 Analyzing opponent's movement pattern...",
                "📊 Calculating competitive advantage...",
                "⚡ Evaluating risk vs reward ratios...",
                "🛡️ Planning defensive countermeasures...",
                "🎯 Computing optimal strategic path...",
                "💭 Processing Q-Learning battle experience...",
                "⚖️ Weighing aggressive vs conservative play...",
                "🏆 Finalizing winning strategy..."
            };

            _aiThinkingTimer = new DispatcherTimer();
            _aiThinkingTimer.Interval = TimeSpan.FromMilliseconds(600);
            _aiThinkingTimer.Tick += (s, e) => UpdateAIThinking();

            // Timer for AI move delay - use difficulty-based timing
            _aiMoveTimer = new DispatcherTimer();
            _aiMoveTimer.Interval = TimeSpan.FromMilliseconds(GameConfig.AIThinkingDelay);
            _aiMoveTimer.Tick += (s, e) => ExecuteAIMove();
        }

        void ShowAIReasoning(string initialThought)
        {
            if (TxtAIThinking != null)
            {
                TxtAIThinking.Text = initialThought;
                _reasoningStep = 0;
                _aiThinkingTimer.Start();
            }
        }

        void UpdateAIThinking()
        {
            if (_reasoningStep < _aiReasoningSteps.Count && TxtAIThinking != null)
            {
                TxtAIThinking.Text = _aiReasoningSteps[_reasoningStep];
                _reasoningStep++;
            }
            else
            {
                _aiThinkingTimer.Stop();
                if (TxtAIThinking != null)
                    TxtAIThinking.Text = "🧠 Decision computed! Ready to execute...";
            }
        }

        void SetupBoard()
        {
            DrawBoard();
        }

        void UpdateDisplay()
        {
            UpdateEmotion();
            UpdateHud(animated: true);
            UpdatePerformanceStats();
            UpdateTurnIndicator();
        }

        void UpdatePerformanceStats()
        {
            if (TxtScoreDetail != null) TxtScoreDetail.Text = _playerScore.ToString();
            if (TxtSolvingTime != null) TxtSolvingTime.Text = "1.2s";
            if (TxtTotalMoves != null) TxtTotalMoves.Text = _playerPath.Count.ToString();

            // Calculate win rate based on current standing
            double winRate = _playerScore > _aiScore ? 75.0 : (_playerScore == _aiScore ? 50.0 : 25.0);
            if (TxtWinRate != null) TxtWinRate.Text = $"{winRate:F0}%";
        }

        // Turn-based system
        void EndPlayerTurn()
        {
            if (!_isPlayerTurn || _mode != GameMode.SingleVsAI) return;

            _isPlayerTurn = false;

            // Update AI timer based on current difficulty setting
            _aiMoveTimer.Interval = TimeSpan.FromMilliseconds(GameConfig.AIThinkingDelay);

            string thinkingMessage = GameConfig.CurrentAIDifficulty switch
            {
                "Beginner" => "🤔 Hmm, let me think slowly about this...",
                "Intermediate" => "🤖 Analyzing your move... Planning counter-strategy...",
                "Expert" => "⚡ Processing optimal response pattern...",
                "Master" => "🎯 CALCULATING... Predicting your next moves...",
                _ => "🤖 Analyzing your move... Planning counter-strategy..."
            };

            ShowAIReasoning(thinkingMessage);

            // Disable player movement buttons
            SetPlayerControlsEnabled(false);

            // Start AI thinking process
            _aiMoveTimer.Start();
        }

        void StartAITurn()
        {
            if (_isPlayerTurn) return;

            // Different reasoning messages based on AI difficulty
            string reasoning = GameConfig.CurrentAIDifficulty switch
            {
                "Beginner" => "🤔 Hmm... let me think about this slowly...",
                "Intermediate" => "🧠 My turn! Computing optimal move...",
                "Expert" => "⚡ Analyzing patterns... calculating counter-strategy...",
                "Master" => "🎯 ANALYZING... Predicting your next 3 moves...",
                _ => "🧠 My turn! Computing optimal move..."
            };

            ShowAIReasoning(reasoning);
            UpdateEmotion();
        }

        void ExecuteAIMove()
        {
            _aiMoveTimer.Stop();

            if (_mode != GameMode.SingleVsAI) return;

            // Get AI decision based on current state and difficulty
            var move = _agent.ChooseMove(_dungeon, _aiR, _aiC, _dp);
            int newR = _aiR, newC = _aiC;

            string moveReasoning = "";

            if (move == QLearningAgent.Move.Right && _aiC < _dungeon.N - 1)
            {
                newC++;
                moveReasoning = GameConfig.CurrentAIDifficulty switch
                {
                    "Beginner" => "➡️ Going RIGHT... hope this works!",
                    "Intermediate" => "➡️ Moving RIGHT - calculated optimal path!",
                    "Expert" => "➡️ RIGHT move: 87% success probability calculated!",
                    "Master" => "➡️ EXECUTING RIGHT: Perfect path optimization achieved!",
                    _ => "➡️ Moving RIGHT - strategic choice!"
                };
            }
            else if (move == QLearningAgent.Move.Down && _aiR < _dungeon.M - 1)
            {
                newR++;
                moveReasoning = GameConfig.CurrentAIDifficulty switch
                {
                    "Beginner" => "⬇️ Going DOWN... seems okay!",
                    "Intermediate" => "⬇️ Moving DOWN - strategic advantage secured!",
                    "Expert" => "⬇️ DOWN move: Maximum expected value confirmed!",
                    "Master" => "⬇️ EXECUTING DOWN: Checkmate in 3 moves predicted!",
                    _ => "⬇️ Moving DOWN - strategic choice!"
                };
            }
            else
            {
                ShowAIReasoning("🤔 No valid moves available...");
                EndAITurn();
                return;
            }

            ShowAIReasoning(moveReasoning);

            // Execute AI move
            if (newR != _aiR || newC != _aiC)
            {
                _aiR = newR;
                _aiC = newC;
                _aiPath.Add((_aiR, _aiC));
                _aiHp += _dungeon[_aiR, _aiC];
                _aiScore += Math.Max(0, _dungeon[_aiR, _aiC]);

                UpdateDisplay();
                DrawBoard();

                // Check if AI died
                if (_aiHp <= 0)
                {
                    ShowAIReasoning("💀 System failure... I have been defeated...");
                    MessageBox.Show($"🎉 YOU WIN!\n\nAI died with HP: {_aiHp}!\nAI fell into its own trap!\n\nYour HP: {_playerHp}, Score: {_playerScore}",
                                  "AI Defeated!", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Check AI win condition
                if (_aiR == _dungeon.M - 1 && _aiC == _dungeon.N - 1)
                {
                    string victoryMessage = GameConfig.CurrentAIDifficulty switch
                    {
                        "Beginner" => "🎉 Yay! I actually won! That was lucky!",
                        "Intermediate" => "🏆 VICTORY! I have reached the finish line first!",
                        "Expert" => "👑 CALCULATED VICTORY! As predicted by my algorithms!",
                        "Master" => "🌟 FLAWLESS VICTORY! Human resistance was... amusing.",
                        _ => "🏆 VICTORY! I have reached the finish line first!"
                    };

                    ShowAIReasoning(victoryMessage);
                    MessageBox.Show($"AI WINS! ({GameConfig.CurrentAIDifficulty} Difficulty)\n\nAI Final HP: {_aiHp}, Score: {_aiScore}\nYour HP: {_playerHp}, Score: {_playerScore}",
                                  "AI Victory!", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
            }

            EndAITurn();
        }

        void EndAITurn()
        {
            _isPlayerTurn = true;
            ShowAIReasoning("✅ Move completed. Your turn, human...");

            // Re-enable player controls
            SetPlayerControlsEnabled(true);
        }

        void SetPlayerControlsEnabled(bool enabled)
        {
            // Find and enable/disable movement buttons
            if (FindName("BtnUp") is Button btnUp) btnUp.IsEnabled = enabled;
            if (FindName("BtnDown") is Button btnDown) btnDown.IsEnabled = enabled;
            if (FindName("BtnLeft") is Button btnLeft) btnLeft.IsEnabled = enabled;
            if (FindName("BtnRight") is Button btnRight) btnRight.IsEnabled = enabled;

            // Update turn indicator
            UpdateTurnIndicator();
        }

        void UpdateTurnIndicator()
        {
            if (TxtCurrentTurn == null || TurnIndicator == null) return;

            if (_isPlayerTurn)
            {
                TxtCurrentTurn.Text = "👤 Your Turn";
                TurnIndicator.Background = new SolidColorBrush(Color.FromRgb(0, 255, 159)); // Green
            }
            else
            {
                TxtCurrentTurn.Text = "🤖 AI Thinking...";
                TurnIndicator.Background = new SolidColorBrush(Color.FromRgb(255, 69, 0)); // Red
            }
        }

        void DrawBoard()
        {
            Board.Children.Clear();
            Board.Rows = _dungeon.M;
            Board.Columns = _dungeon.N;

            for (int i = 0; i < _dungeon.M; i++)
            {
                for (int j = 0; j < _dungeon.N; j++)
                {
                    var cell = new Border
                    {
                        Margin = new Thickness(3),
                        CornerRadius = new CornerRadius(12),
                        Background = new SolidColorBrush(Color.FromRgb(35, 52, 72))
                    };

                    var tb = new TextBlock
                    {
                        Text = _dungeon[i, j].ToString(),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        FontSize = 18
                    };

                    // Color coding for paths
                    if (_playerPath.Contains((i, j)) && _aiPath.Contains((i, j)))
                    {
                        // Both players visited - purple/mixed color
                        cell.Background = new SolidColorBrush(Color.FromRgb(138, 43, 226)); // BlueViolet
                        tb.Foreground = new SolidColorBrush(Colors.White);
                        tb.FontWeight = FontWeights.Bold;
                    }
                    else if (_playerPath.Contains((i, j)))
                    {
                        // Player path - green
                        cell.Background = new SolidColorBrush(Color.FromRgb(34, 139, 34)); // ForestGreen
                        tb.Foreground = new SolidColorBrush(Colors.White);
                    }
                    else if (_aiPath.Contains((i, j)))
                    {
                        // AI path - red
                        cell.Background = new SolidColorBrush(Color.FromRgb(220, 20, 60)); // Crimson
                        tb.Foreground = new SolidColorBrush(Colors.White);
                    }

                    // Current positions - with glow effect
                    if (i == _playerR && j == _playerC)
                    {
                        cell.Background = new SolidColorBrush(Color.FromRgb(0, 255, 127)); // SpringGreen
                        tb.Text = "👤" + _dungeon[i, j].ToString();
                        tb.FontWeight = FontWeights.Bold;
                    }
                    else if (i == _aiR && j == _aiC)
                    {
                        cell.Background = new SolidColorBrush(Color.FromRgb(255, 69, 0)); // OrangeRed
                        tb.Text = "🤖" + _dungeon[i, j].ToString();
                        tb.FontWeight = FontWeights.Bold;
                    }

                    cell.Child = tb;

                    // Highlight finish line
                    if (i == _dungeon.M - 1 && j == _dungeon.N - 1)
                    {
                        cell.BorderBrush = new SolidColorBrush(Colors.Gold);
                        cell.BorderThickness = new Thickness(3);
                        tb.Text = "🏁" + _dungeon[i, j].ToString();
                        tb.FontWeight = FontWeights.Bold;
                    }

                    Board.Children.Add(cell);
                }
            }
        }

        void UpdateEmotion()
        {
            string emotion = "😊";
            string status = "Confident";

            // AI emotion based on relative performance vs player
            if (_aiHp < _playerHp - 30)
            {
                emotion = "😰";
                status = "Worried";
            }
            else if (_aiHp < _playerHp)
            {
                emotion = "😤";
                status = "Determined";
            }
            else if (_aiHp > _playerHp + 20)
            {
                emotion = "�";
                status = "Superior";
            }
            else if (_aiScore > _playerScore)
            {
                emotion = "🤩";
                status = "Dominating";
            }

            if (TxtAIEmotion != null) TxtAIEmotion.Text = emotion;
            if (TxtAIStatus != null) TxtAIStatus.Text = status;
        }

        void UpdateHud(bool animated = false)
        {
            // Update player stats
            if (TxtHP != null) TxtHP.Text = _playerHp.ToString();
            if (TxtCurrentPos != null) TxtCurrentPos.Text = $"({_playerR},{_playerC})";
            if (TxtScore != null) TxtScore.Text = _playerScore.ToString();

            // Update AI stats if available
            if (TxtAiStats != null && _agent != null)
                TxtAiStats.Text = $"AI HP: {_aiHp} | AI Score: {_aiScore} | Episodes: {_agent.Episodes}";
        }

        // Number "count up/down" animation
        void AnimateNumber(TextBlock tb, int newVal)
        {
            if (tb == null) return;

            var timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(50);

            int currentVal = int.TryParse(tb.Text, out int val) ? val : 0;
            int diff = newVal - currentVal;
            int step = Math.Sign(diff);

            timer.Tick += (s, e) =>
            {
                currentVal += step;
                tb.Text = currentVal.ToString();

                if (currentVal == newVal)
                    timer.Stop();
            };

            timer.Start();
        }

        void MakePlayerMove(int newR, int newC)
        {
            if (!_isPlayerTurn || !IsValidMove(newR, newC)) return;

            _playerR = newR;
            _playerC = newC;
            _playerPath.Add((_playerR, _playerC));
            _playerHp += _dungeon[_playerR, _playerC];
            _playerScore += Math.Max(0, _dungeon[_playerR, _playerC]);

            UpdateDisplay();
            DrawBoard();

            // Check if player died
            if (_playerHp <= 0)
            {
                ShowAIReasoning("😈 Excellent! The human fell into my trap!");
                MessageBox.Show($"GAME OVER!\nYour HP reached {_playerHp}. You died!\n\nAI WINS by survival!\nAI HP: {_aiHp}, Score: {_aiScore}",
                              "You Died!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Check player win condition
            if (_playerR == _dungeon.M - 1 && _playerC == _dungeon.N - 1)
            {
                string defeatMessage = GameConfig.CurrentAIDifficulty switch
                {
                    "Beginner" => "😱 Oh no! You beat me! Good job!",
                    "Intermediate" => "😤 Impressive! You outplayed my algorithms!",
                    "Expert" => "😠 Impossible! My calculations were perfect!",
                    "Master" => "🤖 ERROR... HUMAN VICTORY NOT COMPUTED...",
                    _ => "😱 No! The human reached the goal first!"
                };

                ShowAIReasoning(defeatMessage);
                MessageBox.Show($"🎉 VICTORY!\n\nYou beat the AI on {GameConfig.CurrentAIDifficulty} difficulty!\n\nYour Final HP: {_playerHp}, Score: {_playerScore}\nAI HP: {_aiHp}, Score: {_aiScore}",
                              "Victory!", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // End player turn, start AI turn
            EndPlayerTurn();
            StartAITurn();
        }

        bool IsValidMove(int r, int c)
        {
            return r >= 0 && r < _dungeon.M && c >= 0 && c < _dungeon.N;
        }

        // Event handlers for navigation buttons
        private void BtnUp_Click(object sender, RoutedEventArgs e)
        {
            if (_playerR > 0) MakePlayerMove(_playerR - 1, _playerC);
        }

        private void BtnDown_Click(object sender, RoutedEventArgs e)
        {
            if (_playerR < _dungeon.M - 1) MakePlayerMove(_playerR + 1, _playerC);
        }

        private void BtnLeft_Click(object sender, RoutedEventArgs e)
        {
            if (_playerC > 0) MakePlayerMove(_playerR, _playerC - 1);
        }

        private void BtnRight_Click(object sender, RoutedEventArgs e)
        {
            if (_playerC < _dungeon.N - 1) MakePlayerMove(_playerR, _playerC + 1);
        }

        private void BtnAIMove_Click(object sender, RoutedEventArgs e)
        {
            // Remove this button or disable it since AI moves automatically
            // This method can be kept empty or show a message
            MessageBox.Show("AI moves automatically after your turn!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            InitGame();
        }

        private void BtnHelp_Click(object sender, RoutedEventArgs e)
        {
            ShowGameRules();
        }

        private void BtnMenu_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
