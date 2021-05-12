using System.ComponentModel;
using System.Windows.Input;

namespace StockAnalysis.ViewModel
{
    partial class MainWindowContext : INotifyPropertyChanged
    {
        /// <summary>
        /// Command that closes the main Window
        /// </summary>
        public ICommand CloseWindowCommand => closeWindowCommand ??= CommandHelper.Create(() =>
        {
            App.Current.MainWindow.Close();
        });
        private ICommand closeWindowCommand;

        /// <summary>
        /// Command that minimizes the MainWindow
        /// </summary>
        public ICommand MinimizeWindowCommand => minimizeWindowCommand ??= CommandHelper.Create(() =>
        {
            App.Current.MainWindow.WindowState = System.Windows.WindowState.Minimized;
        });
        private ICommand minimizeWindowCommand;

        /// <summary>
        /// Command that toggles WindowMode
        /// </summary>
        public ICommand ToggleWindowCommand => toggleWindowCommand ??= CommandHelper.Create(() =>
        {
            if (App.Current.MainWindow.WindowState == System.Windows.WindowState.Normal)
            {
                App.Current.MainWindow.WindowState = System.Windows.WindowState.Maximized;
            }
            else
            {
                App.Current.MainWindow.WindowState = System.Windows.WindowState.Normal;
            }
        });
        private ICommand toggleWindowCommand;
    }
}
