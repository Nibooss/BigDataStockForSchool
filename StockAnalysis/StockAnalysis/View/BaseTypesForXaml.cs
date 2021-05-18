using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace StockAnalysis.View
{
    public class BaseTypesForXaml : MarkupExtension
    {
        public Type Type { get; set; }
        public string String { get; set; }
        public double? Double { get; set; }
        public int? Int { get; set; }
        public byte? Byte { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if(Type != null)
            {
                return Activator.CreateInstance(Type);
            }
            if (String != null)
            {
                return String;
            }
            if (Double != null)
            {
                return Double.Value;
            }
            if (Int != null)
            {
                return Int.Value;
            }
            if (Byte != null)
            {
                return Byte.Value;
            }
            return null;
        }
    }

    public class btfx : BaseTypesForXaml
    {

    }
}
