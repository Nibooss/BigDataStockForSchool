using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace StockAnalysis.View
{
    class Double2Color : MarkupExtension, IValueConverter
    {
        public Color ColorToMultiply { get; set; }

        private double temp;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            temp = (double)value;

            temp = 1.0 - 1.0 / (1.0 + Math.Pow(temp / (1.0 - temp), -2.0));

            return Color.FromRgb(
                (byte)(ColorToMultiply.R * (double)value),
                (byte)(ColorToMultiply.G * (double)value),
                (byte)(ColorToMultiply.B * (double)value)
                );
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
