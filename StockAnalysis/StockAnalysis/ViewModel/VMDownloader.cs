using StockAnalysis.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace StockAnalysis.ViewModel
{
    public class VMDownloader : INotifyPropertyChanged
    {
        // TODO: Add Symbol Search function


        /// <summary>
        /// Provides a string representing the symbol that can be downloaded
        /// Set calls INotifyPropertyChanded helper function.
        /// </summary>
        public string SymbolName
        {
            get
            {
                return _SymbolName;
            }
            set
            {
                _SymbolName = value;
                RaisePropertyChanged();
            }
        }
        /// <summary>
        /// Holds a string representing the symbol that can be downloaded
        /// </summary>
        public string _SymbolName;


        /// <summary>
        /// Provides not downloaded symbols
        /// Set calls INotifyPropertyChanded helper function.
        /// </summary>
        public ObservableCollection<VMSymbol> WaitingSymbols
        {
            get
            {
                return _WaitingSymbols ??= new ObservableCollection<VMSymbol>();
            }
            set
            {
                _WaitingSymbols = value;
                RaisePropertyChanged();
            }
        }
        /// <summary>
        /// Holds not downloaded symbols
        /// </summary>
        public ObservableCollection<VMSymbol> _WaitingSymbols;



        public ICommand StartDownload => CommandHelper.Create(() => 
        {
            var newSymbol = new VMSymbol(SymbolName);
            Task.Run(() =>
            {
                newSymbol.StartDownload();
            });
            WaitingSymbols.Add(newSymbol);
        });





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
