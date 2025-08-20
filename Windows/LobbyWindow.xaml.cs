using DungeonGameWpf.Models;
using System;
using System.Windows;

namespace DungeonGameWpf.Windows
{
    public partial class LobbyWindow : Window
    {
        public LobbyWindow() { InitializeComponent(); }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            string p1 = string.IsNullOrWhiteSpace(TbP1.Text) ? "P1" : TbP1.Text.Trim();
            string p2 = string.IsNullOrWhiteSpace(TbP2.Text) ? "P2" : TbP2.Text.Trim();
            if (!int.TryParse(TbM.Text, out int m) || m < 1) m = 3;
            if (!int.TryParse(TbN.Text, out int n) || n < 1) n = 3;
            if (!int.TryParse(TbMin.Text, out int vmin)) vmin = -10;
            if (!int.TryParse(TbMax.Text, out int vmax)) vmax = 10;
            if (vmin > vmax) (vmin, vmax) = (vmax, vmin);

            var dungeon = Dungeon.GenerateRandom(m, n, vmin, vmax);
            var w = new GameWindow(GameWindow.GameMode.LocalMultiplayer, dungeon, p1, p2);
            w.ShowDialog();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => Close();
    }
}
