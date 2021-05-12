using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

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
                }
                i++;
            }
            return result;
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
