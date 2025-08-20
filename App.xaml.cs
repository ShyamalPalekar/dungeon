using System;
using System.Windows;

namespace DungeonGameWpf
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                System.Console.WriteLine("WPF Application Starting...");
                base.OnStartup(e);
                System.Console.WriteLine("Base startup completed");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error starting application: {ex.Message}");
                System.Console.WriteLine($"StackTrace: {ex.StackTrace}");
                MessageBox.Show($"Error starting application: {ex.Message}", "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            System.Console.WriteLine($"Unhandled exception: {e.Exception.Message}");
            MessageBox.Show($"An unexpected error occurred: {e.Exception.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }
    }
}
