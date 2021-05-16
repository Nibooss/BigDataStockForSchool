using StockAnalysis.Model;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StockAnalysis.ViewModel
{
    public class ClusterFinder : INotifyPropertyChanged
    {
        private long[] densities;

        public string SymbolName
        {
            get { return symbolName; }
            set { symbolName = value; }
        }
        private string symbolName;

        public StockMoment[] Data
        {
            get { return data; }
            set 
            {
                data = value;
                DataChangedTimeStamp = DateTime.Now;
            }
        }
        private StockMoment[] data;
        private DateTime DataChangedTimeStamp;

        public void RecalculateDensities()
        {
            densities = new long[DensityArraySize];

        }

        /// <summary>
        /// Returns the next higher power of 2
        /// </summary>
        /// <param name="dataLenght"></param>
        private int DensityArraySize
        {
            get
            {
                if(DensityTimeStamp != DataChangedTimeStamp)
                {
                    densityArraySize = (int)Math.Pow(2, (int)Math.Log2(Data.Length) + 1);
                    DensityTimeStamp = DataChangedTimeStamp;
                }
                return densityArraySize;
            }
        }
        private int densityArraySize;
        private DateTime DensityTimeStamp;


        private int DensityArrayIndex(int debth)
        {
            var totalDebth = (int)Math.Log2(densities.Length);
            return (int)Math.Pow(2, totalDebth - debth);
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
