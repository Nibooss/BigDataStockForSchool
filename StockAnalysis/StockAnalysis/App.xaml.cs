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
        /// <summary>
        /// Just shortening a name. Xaml cant work with the new Current property :(
        /// </summary>
        public static App app = Application.Current as App;

        /// <summary>
        /// Just casts the Application current Application to this class.
        /// </summary>
        public static new App Current => (App)Application.Current;

        /// <summary>
        /// Constructor
        /// 
        /// * Opens MainWindow
        /// * Initializes database
        /// </summary>
        public App()
        {
            this.Resources = new GeneralResources();

            ToSQLite.InitializeDatabase();

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

        /// <summary>
        /// Provides an indicator for the app. True if it is busy right now.
        /// Dont set it directly. Use the functions to interact with this. I tried to make the functions threadsave. But I cant promis they are. 
        /// <see cref="IncrementBusy"/>
        /// <see cref="DecrementBusy"/>
        /// </summary>
        public bool IsBusy
        {
            get
            {
                return _IsBusy;
            }
            set
            {
                _IsBusy = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Holds an indicator for the app. True if it is busy right now.
        /// Dont set it directly. Use the functions to interact with this. I tried to make the functions threadsave. But I cant promis they are. 
        /// <see cref="IncrementBusy"/>
        /// <see cref="DecrementBusy"/>
        /// </summary>
        private bool _IsBusy;

        /// <summary>
        /// Holds an internal counter.
        /// </summary>
        private int isBusy;

        /// <summary>
        /// Provides a (hopefully) threadsafe way to interact with the Busy indicator.
        /// Call this function if you start a (long running) Task.
        /// </summary>
        public static void IncrementBusy()
        {
            var result = Interlocked.Increment(ref App.Current.isBusy);
            if (true == App.Current._IsBusy)
            {
                return;
            }
            if(result == 0)
            {
                return;
            }
            App.Current.Dispatcher.BeginInvoke(() =>
            {
                App.Current.IsBusy = true;
            }, DispatcherPriority.Send);
        }

        /// <summary>
        /// Provides a (hopefully) threadsafe way to interact with the Busy indicator.
        /// Call this function if a (long running) Task ends.
        /// </summary>
        public static void DecrementBusy()
        {
            var result = Interlocked.Decrement(ref App.Current.isBusy);
            if (false == App.Current._IsBusy)
            {
                return;
            }
            if(result != 0)
            {
                return;
            }
            App.Current.Dispatcher.BeginInvoke(() =>
            {
                App.Current.IsBusy = false;
            }, DispatcherPriority.ApplicationIdle);
        }

        public static string APIKEY { get; set; } = string.Empty; // "WEWF8LKFM1UXNU6X"; Yes this is my key. I should not have put it here. But I did...


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