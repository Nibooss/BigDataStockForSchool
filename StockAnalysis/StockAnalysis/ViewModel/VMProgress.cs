using StockAnalysis.Model;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StockAnalysis.ViewModel
{
    public class VMProgress : INotifyPropertyChanged
    {
        public VMProgress()
        {
        }
        public VMProgress(string name)
        {
            Name = name;
        }
        public VMProgress(MProgress mProgress)
        {
            LinkToModel = mProgress;
        }
        public VMProgress(string name, MProgress mProgress)
        {
            Name = name;
            LinkToModel = mProgress;
        }
        ~VMProgress()
        {
            // Setter Unsibscribes from old model. Just to be safe :)
            LinkToModel = null;
        }

        public MProgress LinkToModel
        {
            get
            {
                return linkToModel;
            }
            set
            {
                if(linkToModel != null)
                {
                    linkToModel.OnStarted   -= OnModelStarted;
                    linkToModel.OnProgress  -= OnModelChanged;
                    linkToModel.OnFinished  -= OnModelFinished;
                }
                if(value != null)
                {
                    linkToModel = value;
                    linkToModel.OnStarted   += OnModelStarted;
                    linkToModel.OnProgress  += OnModelChanged;
                    linkToModel.OnFinished  += OnModelFinished;
                }
            }
        }
        private MProgress linkToModel;

        public void OnModelStarted(object sender, MProgressEventArgs e)
        {
            App.Current.Dispatcher.Invoke(() => 
            { 
                TotalProgress = 0.0;
            });
        }
        public void OnModelChanged(object sender, MProgressEventArgs e)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                TotalProgress = e.Current;
            });
        }
        public void OnModelFinished(object sender, MProgressEventArgs e)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                TotalProgress = 1.0;
            });
        }

        public bool IsWaiting
        {
            get
            {
                return TotalProgress == 0.0;
            }
        }

        public double TotalProgress
        {
            get
            {
                return totalProgress;
            }
            set
            {
                totalProgress = value;
                RaisePropertyChanged();
            }
        }
        private double totalProgress;

        public string Name 
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
                RaisePropertyChanged();
            }
        }
        private string name;

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
