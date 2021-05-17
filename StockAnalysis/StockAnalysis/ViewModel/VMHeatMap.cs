using StockAnalysis.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace StockAnalysis.ViewModel
{
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
            set => _Symbol.Value = value;
        }
        private Updateable<VMSymbol> _Symbol = new Updateable<VMSymbol>();


        public StockMoment[] Data
        {
            get => (_Data ??= new Updateable<StockMoment[]>(_Symbol)).Value;
            set => _Data.Value = value;
        }
        private Updateable<StockMoment[]> _Data;


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
        public Updateable()
        {

        }
        public Updateable(IUpdateable parent)
        {
            Parent = parent;
        }

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
