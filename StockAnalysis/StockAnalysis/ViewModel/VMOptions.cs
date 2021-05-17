using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StockAnalysis.ViewModel
{
    public class VMOptions : INotifyPropertyChanged
    {



        /// <summary>
        /// Provides the API Key for the app
        /// Set calls INotifyPropertyChanded helper function.
        /// </summary>
        public string APIKey
        {
            get
            {
                return App.APIKEY;
            }
            set
            {
                App.APIKEY = value;
                RaisePropertyChanged();
            }
        }

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
