using DungeonGameWpf.AI;
using DungeonGameWpf.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DungeonGameWpf.Windows
{
    public partial class TrainWindow : Window
    {
        public TrainWindow() { InitializeComponent(); DrawAxes(); }

        void DrawAxes()
        {
            DrawAxis(LossCanvas, "Loss");
            DrawAxis(AccCanvas, "Accuracy");
        }

        void DrawAxis(System.Windows.Controls.Canvas c, string label)
        {
            c.Children.Clear();
            c.Children.Add(new Line { X1 = 30, Y1 = c.ActualHeight - 20, X2 = c.ActualWidth - 10, Y2 = c.ActualHeight - 20, Stroke = Brushes.SlateGray, StrokeThickness = 1 });
            c.Children.Add(new Line { X1 = 30, Y1 = 10, X2 = 30, Y2 = c.ActualHeight - 20, Stroke = Brushes.SlateGray, StrokeThickness = 1 });
            c.Loaded += (_, __) => { DrawAxis(c, label); };
            c.SizeChanged += (_, __) => { DrawAxis(c, label); };
            var tb = new System.Windows.Controls.TextBlock { Text = label, Margin = new Thickness(4) };
            c.Children.Add(tb);
        }

        private async void BtnTrain_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(TbEpisodes.Text, out int episodes)) episodes = 500;
            if (!double.TryParse(TbEps.Text, out double eps)) eps = 0.15;
            if (!double.TryParse(TbAlpha.Text, out double alpha)) alpha = 0.2;
            if (!double.TryParse(TbGamma.Text, out double gamma)) gamma = 0.95;

            var agent = QLearningAgent.Shared;
            agent.Epsilon = eps; agent.Alpha = alpha; agent.Gamma = gamma;

            // latih di beberapa dungeon acak kecil agar generalisasi
            var rnd = new Random();
            var losses = new double[episodes];
            var accs = new double[episodes];

            await Task.Run(() =>
            {
                for (int ep = 0; ep < episodes; ep++)
                {
                    var m = rnd.Next(3, 6);
                    var n = rnd.Next(3, 6);
                    var d = Dungeon.GenerateRandom(m, n, -10, 10);
                    var dp = DpSolver.Build(d.Grid);
                    var loss = agent.TrainEpisode(d, dp);
                    losses[ep] = loss;
                    accs[ep] = agent.Accuracy;
                }
            });

            Plot(LossCanvas, losses, isAccuracy: false);
            Plot(AccCanvas, accs, isAccuracy: true);
            TxtSummary.Text = $"Done. Episodes total: {agent.Episodes}. Accuracy: {agent.Accuracy:P1}. Last Loss: {agent.LastLoss:F3}";
        }

        void Plot(System.Windows.Controls.Canvas c, double[] ys, bool isAccuracy)
        {
            DrawAxis(c, isAccuracy ? "Accuracy" : "Loss");
            if (ys.Length == 0) return;

            double w = c.ActualWidth - 40;
            double h = c.ActualHeight - 30;
            double max = isAccuracy ? 1.0 : Math.Max(1e-6, ys.Max());
            var poly = new Polyline { Stroke = Brushes.DeepSkyBlue, StrokeThickness = 2 };
            for (int i = 0; i < ys.Length; i++)
            {
                double x = 30 + w * i / (ys.Length - 1.0);
                double y = (c.ActualHeight - 20) - h * (ys[i] / max);
                poly.Points.Add(new System.Windows.Point(x, y));
            }
            c.Children.Add(poly);
        }
    }
}
