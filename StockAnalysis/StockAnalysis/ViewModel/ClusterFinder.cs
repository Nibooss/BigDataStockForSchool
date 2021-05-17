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

    public class HeatMapPixel<T> : INotifyPropertyChanged
    {
        public int ChildrenCount { get; set; }

        public Func<T, double> Dimension { get; set; }

        public Collection<T> Data { get; set; }

        private HeatMapPixel<T>[] Areas 
        {
            get
            {
                if(_Areas == null)
                {
                    Task.Run(() =>
                    {
                        return CalculateAreas();
                    }).ContinueWith(t =>
                    {
                        _Areas = t.Result;
                        RaisePropertyChanged();
                    }, App.DispatcherScheduler);
                }
                return _Areas;
            }
            set
            {
                _Areas = value;
                RaisePropertyChanged();
            }
        }
        private HeatMapPixel<T>[] _Areas;

        public int AreaIndex { get; set; }
        public int Departments { get; set; }

        public double LowerBoundry { get; set; } = double.PositiveInfinity;
        public double UpperBoundry { get; set; } = double.NegativeInfinity;

        public int Debth { get; set; }
        public int MaxDebth { get; set; } = 25;

        private HeatMapPixel<T>[] CalculateAreas()
        {
            if(MaxDebth < Debth)
            {
                return null;
            }

            // Precon check
            if (Departments < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(Departments), nameof(Departments) + " can not be lower than 1");
            }

            // Declare Variables
            double MaxValue = double.NegativeInfinity;
            double MinValue = double.PositiveInfinity;
            var retMe = new HeatMapPixel<T>[Departments];

            // Both min and max have to be set
            if (LowerBoundry == double.PositiveInfinity || UpperBoundry == double.NegativeInfinity)
            {
                foreach(T Point in Data)
                {
                    var tempValue = Dimension.Invoke(Point);
                    if (tempValue > MaxValue)
                    {
                        MaxValue = tempValue;
                    }
                    if (tempValue < MinValue)
                    {
                        MinValue = tempValue;
                    }
                }
            }

            // Distance between Points
            double Distance = (MaxValue + MinValue) / Departments;

            // Initialize retMe
            for(int i = 0; i < Departments; i++)
            {
                retMe[i] = new HeatMapPixel<T>()
                {
                    LowerBoundry = LowerBoundry + i * Distance,
                    UpperBoundry = LowerBoundry + (i + 1) * Distance,
                    Debth = Debth + 1,
                    MaxDebth = MaxDebth,
                };
            }

            // Distribute points into right collection
            foreach (T Point in Data)
            {
                // Get Value
                var tempValue = Dimension.Invoke(Point);

                // Subtract minimum
                tempValue -= MinValue;

                // Devide by distance to get index
                tempValue /= Distance;

                // Add to right Collection
                retMe[(int)tempValue].Data.Add(Point);
            }

            return retMe;
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
