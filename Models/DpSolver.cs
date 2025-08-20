namespace DungeonGameWpf.Models
{
    public static class DpSolver
    {
        // dp[i,j] = minimal HP needed to enter (i,j) to guarantee survive to goal
        public static int[,] Build(int[,] dungeon)
        {
            int m = dungeon.GetLength(0), n = dungeon.GetLength(1);
            var dp = new int[m, n];
            dp[m - 1, n - 1] = System.Math.Max(1, 1 - dungeon[m - 1, n - 1]);

            for (int j = n - 2; j >= 0; j--)
            {
                int need = dp[m - 1, j + 1] - dungeon[m - 1, j];
                dp[m - 1, j] = System.Math.Max(1, need);
            }
            for (int i = m - 2; i >= 0; i--)
            {
                int need = dp[i + 1, n - 1] - dungeon[i, n - 1];
                dp[i, n - 1] = System.Math.Max(1, need);
            }
            for (int i = m - 2; i >= 0; i--)
                for (int j = n - 2; j >= 0; j--)
                {
                    int next = System.Math.Min(dp[i + 1, j], dp[i, j + 1]);
                    int need = next - dungeon[i, j];
                    dp[i, j] = System.Math.Max(1, need);
                }
            return dp;
        }
    }
}
