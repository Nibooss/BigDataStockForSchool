using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalysis.Model
{
    public class Symbol
    {
        public string Name { get; set; }
        public int Size { get; set; }
        public double Downloadprogress { get; set; }
        public double DecodeProgress { get; set; }
        public bool IsWorking { get; set; }
        public bool IsShown { get; set; }
        public StockMoment[] Data { get; set; }
    }
}
