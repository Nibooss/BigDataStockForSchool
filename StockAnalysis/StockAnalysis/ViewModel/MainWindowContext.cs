using StockAnalysis.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace StockAnalysis.ViewModel
{
    partial class MainWindowContext : INotifyPropertyChanged
    {
        public ObservableCollection<Symbol> DownloadedFiles => downloadedFiles ??= initDownlaodedFiles();
        private ObservableCollection<Symbol> downloadedFiles;
        private ObservableCollection<Symbol> initDownlaodedFiles()
        {
            var oc = new ObservableCollection<Symbol>();

            Task.Run(ToSQLite.GetAllSymbols().ToList).ContinueWith(t => 
            { 
                foreach(var symbol in t.Result)
                {
                    oc.Add(symbol);
                }
            }, App.DispatcherScheduler);

            return oc;
        }
        StockDownloader Downloader = new StockDownloader();

        public string SymbolToDownload { get; set; }

        public ICommand DownloadCommand => downloadCommand ??= CommandHelper.Create(p =>
        {
            // Just in case entry gets modified while we are downloading
            var BackupName = SymbolToDownload;

            if(SymbolToDownload == null || SymbolToDownload == string.Empty)
            {
                // TODO: Do some error handling
                return;
            }

            // Task chain definition:
            // Start downloading, Decode and save two last years
            Task.Run(() =>
            {
                var arr = Downloader.DownloadTwoYears(SymbolToDownload).ToArray();
                return arr;
            }).ContinueWith(t=>
            {
                // Set it on MainThread Context
                var s = new Symbol()
                {
                    Name = BackupName,
                    Data = t.Result,
                    Size = t.Result.Length,
                };
                DownloadedFiles.Add(s);
            }, App.DispatcherScheduler);
        });
        private ICommand downloadCommand;

        public ICommand RemoveCommand => removeCommand ??= CommandHelper.Create(p =>
        {
            if(p is Symbol S)
            {
                DownloadedFiles.Remove(S);
                ToSQLite.RemoveData(S);
            }
            else
            {
                // TODO: Do some Error handling
            }
        });
        private ICommand removeCommand;

        public ICommand ToggleBusy => toggleBusy ??= CommandHelper.Create(p =>
        {
            App.app.IsBusy = !App.app.IsBusy;
        });
        private ICommand toggleBusy;

        public Progress[] Progresses => new Progress[]
        {
            Downloader.StockDownloaderProgress,
            Downloader.StockDecoderProgress,
            Downloader.StockSaverProgress,
            Downloader.AllProgress,
            ToSQLite.CurrentProgress,
        };

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
