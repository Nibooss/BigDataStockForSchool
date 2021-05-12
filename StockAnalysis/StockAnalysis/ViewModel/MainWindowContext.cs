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
        public ObservableCollection<VMSymbol> DownloadedFiles => downloadedFiles ??= initDownlaodedFiles();
        private ObservableCollection<VMSymbol> downloadedFiles;
        private ObservableCollection<VMSymbol> initDownlaodedFiles()
        {
            var oc = new ObservableCollection<VMSymbol>();

            Task.Run(ToSQLite.GetAllSymbols().ToList).ContinueWith(t => 
            { 
                foreach(MSymbol symbol in t.Result)
                {
                    oc.Add(new VMSymbol(symbol));
                }
            }, App.DispatcherScheduler);

            return oc;
        }
        StockDownloader Downloader = new StockDownloader();

        public string SymbolToDownload { get; set; }

        public ICommand AddSymbolCommand => addSymbolCommand ??= CommandHelper.Create(p =>
        {
            // Just in case entry gets modified while we are downloading
            var SymbolName = p as string;

            if(SymbolName == null || SymbolName == string.Empty)
            {
                // TODO: Do some error handling
                return;
            }

            var newsymbol = new VMSymbol() { Name = SymbolName };
            DownloadedFiles.Add(newsymbol);
            Task.Run(() =>
            {
                newsymbol.StartDownload();
            });
        });
        private ICommand addSymbolCommand;

        public ICommand RemoveCommand => removeCommand ??= CommandHelper.Create(p =>
        {
            if(p is VMSymbol S)
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
            App.app.IsBusy = App.app.IsBusy == 0 ? 1 : 0;
        });
        private ICommand toggleBusy;

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
