using StockAnalysis.Model;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StockAnalysis.ViewModel
{
    public class VMSymbol : MSymbol, INotifyPropertyChanged
    {
        /// <summary>
        /// Constructor without params
        /// </summary>
        public VMSymbol()
        {

        }

        /// <summary>
        /// In case you want to make this younger
        /// </summary>
        /// <param name="mSymbol"></param>
        public VMSymbol(MSymbol mSymbol)
        {
            Name = mSymbol.Name;
            Size = mSymbol.Size;
            Data = mSymbol.Data;
        }


        new public string Name 
        {
            get
            {
                return base.Name;
            }
            set
            {
                base.Name = value;
                RaisePropertyChanged();
            }
        }
        new public int Size
        {
            get
            {
                return base.Size;
            }
            set
            {
                base.Size = value;
                RaisePropertyChanged();
            }
        }
        new public StockMoment[] Data
        {
            get
            {
                return base.Data;
            }
            set
            {
                base.Data = value;
                RaisePropertyChanged();
            }
        }

        public VMProgress DownloadProgress => downloadProgress ??= new VMProgress("Total Progress", downloader.AllProgress);
        private VMProgress downloadProgress;

        public VMProgress[] ToolTipProgresses => new VMProgress[]
        {
            new VMProgress("Downloader Progress", downloader.StockDownloaderProgress),
            new VMProgress("Saving Progress", downloader.StockSaverProgress),
            DownloadProgress,
        };

        private StockDownloader downloader => _downloader ??= new StockDownloader();
        private StockDownloader _downloader;

        public bool IsSelected
        {
            get
            {
                return isSelected;
            }
            set
            {
                isSelected = value;
                RaisePropertyChanged();
            }
        }
        public bool isSelected;

        public bool IsDownloaded
        {
            get
            {
                return isDownloaded;
            }
            set
            {
                isDownloaded = value;
                RaisePropertyChanged();
            }
        }
        private bool isDownloaded;

        public async void StartDownload()
        {
            RaisePropertyChanged(nameof(DownloadProgress));
            var data = await downloader.DownloadTwoYears(Name);
            Data = data.ToArray();
            
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
