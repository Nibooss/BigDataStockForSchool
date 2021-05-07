using StockAnalysis.Model;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace StockAnalysis.ViewModel
{
    class MainWindowContext
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
            StockDownloader.DownloadTwoYears(SymbolToDownload).ContinueWith(t =>
            {
                // Read the symbol back to ( get size of the symbol mostly )
                return ToSQLite.GetSpecificSymbol(BackupName);            
            }).ContinueWith(t=>
            {
                // Set it on MainThread Context
                downloadedFiles.Add(t.Result);
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
    }
}
