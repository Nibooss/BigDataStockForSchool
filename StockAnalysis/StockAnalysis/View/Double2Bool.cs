using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;

namespace StockAnalysis.View
{
    class Double2Bool : MarkupExtension, IValueConverter
    {

        /// <summary>
        /// Provides a Cutoff Value. If value is greater than this. Converter will return true;
        /// Set calls INotifyPropertyChanded helper function.
        /// </summary>
        public double Cutoff
        {
            get
            {
                return _Cutoff;
            }
            set
            {
                _Cutoff = value;
            }
        }
        /// <summary>
        /// Holds a Cutoff Value. If value is greater than this. Converter will return true;
        /// </summary>
        public double _Cutoff;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (double)value > Cutoff;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? Cutoff : 0.0;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
