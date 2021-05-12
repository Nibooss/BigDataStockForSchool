using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace StockAnalysis.View
{
    class MoreTags : DependencyObject
    {
        public static double GetDoubleTag_A(DependencyObject obj)
        {
            return (double)obj.GetValue(DoubleTag_AProperty);
        }

        public static void SetDoubleTag_A(DependencyObject obj, double value)
        {
            obj.SetValue(DoubleTag_AProperty, value);
        }

        // Using a DependencyProperty as the backing store for DoubleTag_A.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DoubleTag_AProperty =
            DependencyProperty.RegisterAttached("DoubleTag_A", typeof(double), typeof(MoreTags), new PropertyMetadata(0.0));


    }
}
