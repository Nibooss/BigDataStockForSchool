using StockAnalysis.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace StockAnalysis.ViewModel
{
    public class DummyHeatMapPixel : HeatMapPixel<object>
    {

    }

    public static class IEnumExtensions
    {
        public static IEnumerable<Tto> ConvertWith<Tto,Tfrom>(this IEnumerable<Tfrom> ts, Func<Tfrom, Tto> func)
        {
            foreach(Tfrom o in ts)
            {
                var temp = func.Invoke(o);
                if (null != temp)
                {
                    yield return temp;
                }
            }
        }
    }

    public class VMHeatMap : INotifyPropertyChanged
    {

        /// <summary>
        /// Provides the threshold
        /// Set calls INotifyPropertyChanded helper function.
        /// </summary>
        public double Threshold
        {
            get
            {
                return _Threshold;
            }
            set
            {
                _Threshold = value;
                IsDirty = true;
                RaisePropertyChanged();
            }
        }
        /// <summary>
        /// Holds the threshold
        /// </summary>
        public double _Threshold = 0.05;


        /// <summary>
        /// Provides the number of devisions per level
        /// Set calls INotifyPropertyChanded helper function.
        /// </summary>
        public int Divisions
        {
            get
            {
                return _Divisions;
            }
            set
            {
                _Divisions = value;
                IsDirty = true;
                RaisePropertyChanged();
            }
        }
        /// <summary>
        /// Holds the number of devisions per level
        /// </summary>
        public int _Divisions = 2;


        /// <summary>
        /// Provides max Debth
        /// Set calls INotifyPropertyChanded helper function.
        /// </summary>
        public int MaxDebth
        {
            get
            {
                return _MaxDebth;
            }
            set
            {
                _MaxDebth = value;
                IsDirty = true;
                RaisePropertyChanged();
            }
        }
        /// <summary>
        /// Holds max Debth
        /// </summary>
        public int _MaxDebth = 13;


        /// <summary>
        /// Provides an indicator indicating changed data
        /// Set calls INotifyPropertyChanded helper function.
        /// </summary>
        public bool IsDirty
        {
            get
            {
                return _IsDirty;
            }
            set
            {
                _IsDirty = value;
                RaisePropertyChanged();
            }
        }
        /// <summary>
        /// Holds an indicator indicating changed data
        /// </summary>
        public bool _IsDirty;

        /// <summary>
        /// Provides all symbols that can be selected
        /// Set calls INotifyPropertyChanded helper function.
        /// </summary>
        public List<VMSymbol> Symbols
        {
            get
            {
                return _Symbols ??= ToSQLite.GetAllSymbols().ConvertWith(s => new VMSymbol(s)).ToList();
            }
            set
            {
                _Symbols = value;
                RaisePropertyChanged();
            }
        }
        /// <summary>
        /// Holds all symbols that can be selected
        /// </summary>
        private List<VMSymbol> _Symbols;


        public VMSymbol Symbol
        {
            get => _Symbol.Value;
            set
            {
                _Symbol.Value = value;
                IsDirty = true;
                RaisePropertyChanged(nameof(Data));
            }
        }
        private Updateable<VMSymbol> _Symbol = new Updateable<VMSymbol>();


        public StockMoment[] Data
        {
            get => (_Data ??= new Updateable<StockMoment[]>(_Symbol, ()=>
            {
                StockMoment[] data = new StockMoment[0];
                Task.Run(() => {
                    return ToSQLite.ReadFromDB(Symbol);
                }).ContinueWith(t =>
                {
                    Data = t.Result;
                }, App.DispatcherScheduler);
                return data;
            })).Value;
            set
            {
                _Data.Value = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(HeatMap));
            }
        }
        private Updateable<StockMoment[]> _Data;


        /// <summary>
        /// Provides data that has been clicked on
        /// Set calls INotifyPropertyChanded helper function.
        /// </summary>
        public Collection<StockMoment> SelectedData
        {
            get
            {
                return _SelectedData;
            }
            set
            {
                _SelectedData = value;
                RaisePropertyChanged();
            }
        }
        /// <summary>
        /// Holds data that has been clicked on
        /// </summary>
        public Collection<StockMoment> _SelectedData;


        public ICommand SelectData => CommandHelper.Create((p) =>
        { 
            if(p is Collection<StockMoment> IESM)
            {
                SelectedData = IESM;
            }
        });

        /// <summary>
        /// Provides the heatMap
        /// </summary>
        public HeatMapPixel<StockMoment> HeatMap
        {
            get
            {
                return _HeatMap ??= initHeatMap();
            }
            set
            {
                _HeatMap = value;
                RaisePropertyChanged();
            }
        }
        private HeatMapPixel<StockMoment> _HeatMap;
        private HeatMapPixel<StockMoment> initHeatMap()
        {
            // Precondition Check
            if(Data == null)
            {
                return null;
            }

            var returnvalue = new HeatMapPixel<StockMoment>();
            returnvalue.Dimension = sm => sm.Volume;
            returnvalue.Departments = Divisions;
            returnvalue.Data = new ObservableCollection<StockMoment>(Data);
            returnvalue.InitialDataLenght = returnvalue.Data.Count;
            returnvalue.Threshold = Threshold;
            returnvalue.NextAvailable = 1.0;
            returnvalue.MaxDebth = MaxDebth;
            return returnvalue;
        }

        public ICommand UpdateHeatMap => CommandHelper.Create(() =>
        {
            HeatMap = initHeatMap();
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

    // TODO: Move somewhere else

    public interface IUpdateable : INotifyPropertyChanged
    {
        public DateTime LastUpdated { get; set; }
    }

    public class Updateable<T> : IUpdateable
    {
        public Updateable()
        {

        }
        public Updateable(IUpdateable parent)
        {
            Parent = parent;
        }
        public Updateable(IUpdateable parent, Func<T> updateFunc)
        {
            Parent = parent;
            UpdateFunc = updateFunc;
        }

        public T Value
        {
            get
            {
                if(IsLazy && !IsUpToDate && UpdateFunc != null)
                {
                    _Value = UpdateFunc.Invoke();
                }
                return _Value;
            }
            set
            {
                _Value = value;
                LastUpdated = Parent?.LastUpdated ?? DateTime.Now;
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
            if (false == IsLazy)
            {
                UpdateFunc?.Invoke();
            }
            RaisePropertyChanged(nameof(IsUpToDate));
        }

        public Func<T> UpdateFunc { get; set; }

        /// <summary>
        /// false = Updates as soon as parent updates.
        /// true  = Updates when it is read.
        /// </summary>
        public bool IsLazy { get; set; }

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
