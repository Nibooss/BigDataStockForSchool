using StockAnalysis.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace StockAnalysis.ViewModel
{
    public class VMHeatMap : INotifyPropertyChanged
    {

        public string SymbolName
        {
            get { return symbolName; }
            set { symbolName = value; }
        }
        private string symbolName;

        public StockMoment[] Data { get; set; }


        /// <summary>
        /// Provides the heatMap
        /// </summary>
        public HeatMapPixel<StockMoment> HeatMap => _HeatMap ??= initHeatMap();
        private HeatMapPixel<StockMoment> _HeatMap;
        private HeatMapPixel<StockMoment> initHeatMap()
        {
            var returnvalue = new HeatMapPixel<StockMoment>();
            returnvalue.Dimension = sm => sm.Volume;
            returnvalue.MaxDebth = 10;
            return returnvalue;
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

    // TODO: Move somewhere else

    public interface IUpdateable : INotifyPropertyChanged
    {
        public DateTime LastUpdated { get; set; }
    }

    public class Updateable<T> : IUpdateable
    {
        public T Value
        {
            get
            {
                return _Value;
            }
            set
            {
                _Value = value;
                LastUpdated = DateTime.Now;
                RaisePropertyChanged();
            }
        }
        private T _Value;

        public DateTime LastUpdated
        {
            get
            {
                return _LastUpdated;
            }
            set
            {
                _LastUpdated = value;
            }
        }
        private DateTime _LastUpdated;

        public IUpdateable Parent
        {
            get
            {
                return _Parent;
            }
            set
            {
                if (_Parent != null)
                {
                    _Parent.PropertyChanged -= ParentPropertyChanged;
                }
                if (value != null)
                {
                    _Parent = value;
                    _Parent.PropertyChanged += ParentPropertyChanged;
                }
            }
        }
        private IUpdateable _Parent;

        public bool IsUpToDate => Parent.LastUpdated == this.LastUpdated;

        private void ParentPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(IsUpToDate));
        }

        public Func<T> UpdateFunc { get; set; }

        public bool AutoUpdate { get; set; }

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
            if (AutoUpdate)
            {
                UpdateFunc?.Invoke();
            }
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
