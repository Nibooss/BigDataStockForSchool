using StockAnalysis.Model;
using StockAnalysis.View;
using StockAnalysis.ViewModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace StockAnalysis
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, INotifyPropertyChanged
    {
        public static App app = Application.Current as App;
        public static new App Current => Application.Current as App;
        public App()
        {
            this.Resources = new GeneralResources();

            ToSQLite.InitializeDatabase();
            //InitializeComponent(); // Stupid WPF bug. Not calling this function makes the xaml file pointles :/
            Current.MainWindow = new VMainWindow();
            Current.MainWindow.DataContext = new MainWindowContext();
            Current.MainWindow.Show();
        }

        public static SynchronizationContext DispatcherSyncroContext => dispatcherSyncroContext ??= initDispatcherSyncroContext();
        public static SynchronizationContext dispatcherSyncroContext;
        public static SynchronizationContext initDispatcherSyncroContext()
        {
            SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(App.Current.Dispatcher));
            return SynchronizationContext.Current;
        }
        public static TaskScheduler DispatcherScheduler => dispatcherScheduler ??= initDispatcherScheduler();
        public static TaskScheduler dispatcherScheduler;
        public static TaskScheduler initDispatcherScheduler()
        {
            _ = DispatcherSyncroContext;
            return TaskScheduler.FromCurrentSynchronizationContext();
        }

        public int IsBusy
        {
            get
            {
                return isBusy;
            }
            set
            {
                isBusy = value;
                RaisePropertyChanged();
            }
        }
        private int isBusy;
        public static string APIKEY { get; set; } = "WEWF8LKFM1UXNU6X";


        /// <summary>
        /// EventHandler for INotifyPropertyChanged interface
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Function to simplify the INotifyPropertyChanged interface
        /// </summary>
        public void RaisePropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        /// <summary>
        /// Function to simplify the INotifyPropertyChanged interface
        /// </summary>
        public void RaisePropertyChanged(object caller, [CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(caller ?? this, new PropertyChangedEventArgs(propName));
        }

    }
}