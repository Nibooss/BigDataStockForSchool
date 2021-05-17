using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StockAnalysis.ViewModel
{
    public class MainWindowContext : INotifyPropertyChanged
    {
        public Selectable[] SelectableViewModels => _SelectableViewModels ??= new Selectable[]
        {
            new SelectableMenuEntry(){Name="Main Menu",   Value=typeof(VMMainMenu),     Context=this},
            new SelectableMenuEntry(){Name="Downloader",  Value=typeof(VMDownloader),   Context=new VMDownloader()},
            new SelectableMenuEntry(){Name="Graph",       Value=typeof(VMGraphViewer),  Context=typeof(VMStockDataProvider)},
            new SelectableMenuEntry(){Name="Table",       Value=typeof(VMTableViewer),  Context=typeof(VMStockDataProvider)},
            new SelectableMenuEntry(){Name="Options",     Value=typeof(VMOptions),      Context=new VMOptions()},
        };
        Selectable[] _SelectableViewModels;
        public Selectable SelectedItem
        {
            get
            {
                return _SelectedItem ??= SelectableViewModels[0];
            }
            set
            {
                if(_SelectedItem != null)
                {
                    _SelectedItem.IsSelected = false;
                }
                if(value != null)
                {
                    _SelectedItem = value;
                    _SelectedItem.IsSelected = true;
                }
                RaisePropertyChanged();
            }
        }
        private Selectable _SelectedItem;

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

    public class SelectableMenuEntry : Selectable
    {
        /// <summary>
        /// Provides the DataContext for the <see cref="Value"/>
        /// In case a type is given this allways creates a new instance of type
        /// Set calls INotifyPropertyChanded helper function.
        /// </summary>
        public object Context
        {
            get
            {
                if(_Context is Type t)
                {
                    return Activator.CreateInstance(t);
                }
                return _Context;
            }
            set
            {
                _Context = value;
                RaisePropertyChanged();
            }
        }
        /// <summary>
        /// Holds the DataContext for the <see cref="Value"/>
        /// </summary>
        private object _Context;
    }

    public class Selectable : INotifyPropertyChanged
    {
        /// <summary>
        /// Constructor taking no argument
        /// </summary>
        public Selectable()
        {
        }

        /// <summary>
        /// Provides the value of this selectable object
        /// Set calls INotifyPropertyChanded helper function.
        /// </summary>
        public object Value
        {
            get
            {
                return _Value;
            }
            set
            {
                _Value = value;
                RaisePropertyChanged();
            }
        }
        /// <summary>
        /// Holds the value of this selectable object
        /// </summary>
        protected object _Value;

        /// <summary>
        /// Provides an indicator to this objects selected state
        /// Set calls INotifyPropertyChanded helper function.
        /// </summary>
        public bool IsSelected
        {
            get
            {
                return _IsSelected;
            }
            set
            {
                _IsSelected = value;
                RaisePropertyChanged();
            }
        }
        /// <summary>
        /// Holds an indicator to this objects selected state
        /// </summary>
        protected bool _IsSelected;


        /// <summary>
        /// Provides a name
        /// Set calls INotifyPropertyChanded helper function.
        /// </summary>
        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                _Name = value;
                RaisePropertyChanged();
            }
        }
        /// <summary>
        /// Holds a name
        /// </summary>
        protected string _Name;



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
