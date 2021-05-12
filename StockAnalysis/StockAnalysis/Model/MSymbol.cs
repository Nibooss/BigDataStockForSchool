using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalysis.Model
{
    public class MSymbol
    {
        public string Name { get; set; }
        public int Size { get; set; }
        public StockMoment[] Data { get; set; }
    }
}
