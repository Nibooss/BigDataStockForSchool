using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalysis.Model
{
    public class Progress : INotifyPropertyChanged
    {
        public string Name 
        { 
            get => name;
            set
            {
                name = value;
                RaisePropertyChanged();
            }
        }
        private string name;

        public void Init(int NumberOfItems)
        {
            App.Current.Dispatcher.InvokeAsync(() =>
            {
                NumObjects = NumberOfItems;
                IsBusy = true;
                ProcessedItems = 0;
            });
        }
        public void Done()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                IsBusy = false;
            });
        }
        public void NotifyProgress(int processedItems = -1)
        {
            App.Current.Dispatcher.InvokeAsync(() =>
            {
                if (-1 == processedItems)
                {
                    ProcessedItems++;
                }
                else
                {
                    ProcessedItems = processedItems;
                }
            });
        }


        public bool IsBusy
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
        private bool isBusy;
        public int NumObjects
        {
            get
            {
                return numObjects;
            }
            set
            {
                numObjects = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(Current));
                RaisePropertyChanged(nameof(CurrentString));
            }
        }
        private int numObjects;
        public int ProcessedItems
        {
            get
            {
                return processedItems == 0 ? 1 : processedItems;
            }
            set
            {
                processedItems = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(Current));
                RaisePropertyChanged(nameof(CurrentString));
            }
        }
        private int processedItems;
        public double Current => (double)ProcessedItems / (double)NumObjects;
        public string CurrentString => $"{ProcessedItems}/{NumObjects}";

        public void RaisePropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
