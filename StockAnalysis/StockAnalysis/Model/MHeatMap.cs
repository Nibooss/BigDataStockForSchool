using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace StockAnalysis.Model
{

    public class HeatMapPixel<T> : INotifyPropertyChanged
    {
        public HeatMapPixel()
        {
        }

        public HeatMapPixel(Func<T, double> dimension, ObservableCollection<T> data, int departments, double lowerBoundry, double upperBoundry, int debth, int maxDebth, int inheritDataLength = -1)
        {
            Dimension = dimension;
            Data = data;
            Departments = departments;
            LowerBoundry = lowerBoundry;
            UpperBoundry = upperBoundry;
            Debth = debth;
            MaxDebth = maxDebth;

            InitialDataLenght = inheritDataLength;
        }

        public double Threshold { get; set; }
        public CancellationTokenSource CTS {get;set;}
        public double NextAvailable { get; set; }

        public int InitialDataLenght 
        {
            get
            {
                return _InitialDataLenght;
            }
            set
            {
                _InitialDataLenght = value;
            }
        }
        public int _InitialDataLenght;

        public int ChildrenCount { get; set; }

        public Func<T, double> Dimension { get; set; }

        public ObservableCollection<T> Data 
        {
            get
            {
                return _Data;
            }
            set
            {
                _Data = value;
                RaisePropertyChanged();
                RaisePropertyChanged(InitialDataLenght);
            }
        }
        public ObservableCollection<T> _Data;

        public HeatMapPixel<T>[] Areas
        {
            get
            {
                if (_Areas == null)
                {
                    Task.Run(() =>
                    {
                        if (CTS.Token.IsCancellationRequested)
                        {
                            return;
                        }

                        App.IncrementBusy();
                        HeatMapPixel<T>[] temp = CalculateAreas();
                        if(temp == null)
                        {
                            App.DecrementBusy();
                            return;
                        }
                        if (CTS.Token.IsCancellationRequested)
                        {
                            App.DecrementBusy();
                            return;
                        }
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            _Areas = temp; 
                            RaisePropertyChanged();
                            App.DecrementBusy();
                        }, System.Windows.Threading.DispatcherPriority.Background);
                    });
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
        public int MaxDebth { get; set; }

        private HeatMapPixel<T>[] CalculateAreas()
        {
            if (MaxDebth < Debth)
            {
                return null;
            }
            if(NextAvailable <= Threshold)
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

            // Determine Min and max in current department
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

            if(LowerBoundry == double.PositiveInfinity)
            {
                LowerBoundry = MinValue;
            }
            if(UpperBoundry == double.NegativeInfinity)
            {
                UpperBoundry = MaxValue;
            }

            double SizeOfDepartment = UpperBoundry - LowerBoundry;
            // Distance between Points
            double SizeOfSubDeparment = SizeOfDepartment / Departments;

            // Initialize retMe
            for (int i = 0; i < Departments; i++)
            {
                retMe[i] = new HeatMapPixel<T>(
                    dimension: Dimension,
                    data: new ObservableCollection<T>(),
                    departments: Departments,
                    lowerBoundry: LowerBoundry + i * SizeOfSubDeparment,
                    upperBoundry: LowerBoundry + (i + 1) * SizeOfSubDeparment,
                    debth: Debth + 1,
                    maxDebth: MaxDebth,
                    inheritDataLength: InitialDataLenght
                    );
                retMe[i].Threshold = Threshold;
                retMe[i].CTS = CTS;
            }

            // Distribute points into right collection
            foreach (T Point in Data)
            {
                // Get Value
                var tempValue = Dimension.Invoke(Point);

                // Subtract minimum
                tempValue -= LowerBoundry;

                // Devide by distance to get index
                tempValue /= SizeOfSubDeparment;

                // Hack: Is this correct?
                // Clamp
                if(tempValue >= retMe.Length)
                {
                    tempValue -= 1;
                }

                // Add to right Collection
                retMe[(int)tempValue].Data.Add(Point);
            }

            // Tell them if they sould start calulate more
            for (int i = 0; i < Departments; i++)
            {
                retMe[i].NextAvailable = ((double)retMe[i].Data.Count / (double)Data.Count);
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
