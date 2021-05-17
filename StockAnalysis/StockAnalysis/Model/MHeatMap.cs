using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace StockAnalysis.Model
{

    public class HeatMapPixel<T> : INotifyPropertyChanged
    {
        public int ChildrenCount { get; set; }

        public Func<T, double> Dimension { get; set; }

        public Collection<T> Data { get; set; }

        private HeatMapPixel<T>[] Areas
        {
            get
            {
                if (_Areas == null)
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
            if (MaxDebth < Debth)
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
                foreach (T Point in Data)
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
            for (int i = 0; i < Departments; i++)
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
}
