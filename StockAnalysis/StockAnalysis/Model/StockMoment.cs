using System;
using System.IO;

namespace StockAnalysis.Model
{
    public class StockMoment
    {
        // Info AboutStock
        public DateTime Time { get; set; } 

        public double Open { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Close { get; set; }
        public double Volume { get; set; }
    }
}
