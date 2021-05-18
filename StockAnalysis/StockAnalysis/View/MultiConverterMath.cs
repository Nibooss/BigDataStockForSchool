using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace StockAnalysis.View
{
    class MultiConverterMathSimple : MarkupExtension, IMultiValueConverter
    {

        /// <summary>
        /// Provides a double that is used as initial offset in the calculator
        /// Set calls INotifyPropertyChanded helper function.
        /// </summary>
        public double Offset
        {
            get
            {
                return _Offset ??= 0.0;
            }
            set
            {
                _Offset = value;
            }
        }
        /// <summary>
        /// Holds a double that is used as initial offset in the calculator
        /// </summary>
        public double? _Offset;

        public string Calculation { get; set; }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // Precondition Checks
            if(Calculation == null)
            {
                return 0.0;
            }
            

            var result = Offset;
            var i = 0;
            while(true)
            {
                if(i >= values.Length)
                {
                    break;
                }
                if(i >= Calculation.Length)
                {
                    break;
                }
                if(values[i].GetType() != typeof(double))
                {
                    return result;
                }
                switch (Calculation[i])
                {
                    case '+':
                        result += (double)values[i];
                        break;
                    case '-':
                        result -= (double)values[i];
                        break;
                    case '*':
                        result *= (double)values[i];
                        break;
                    case '/':
                        result /= (double)values[i];
                        break;
                    default:
                        break;
                }
                i++;
            }
            while (true)
            {
                if(i >= Calculation.Length)
                {
                    break;
                }
                switch (Calculation[i])
                {
                    case '-':
                        result = -result;
                        break;
                    case 'p':
                        result = result < 0 ? result : 0;
                        break;
                    case 'n':
                        result = result < 0 ? 0 : result;
                        break;
                    case 'g':
                        return new GridLength(result, GridUnitType.Pixel);
                    case 'G':
                        return new GridLength(result, GridUnitType.Star);
                }
                i++;
            }
            switch(targetType.Name)
            {
                case nameof(GridLength):
                    return new GridLength(result, GridUnitType.Pixel);
                case nameof(Color):
                    return Color.FromRgb((byte)result, (byte)result, (byte)result);
                default:
                    return result;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
