using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalysis.Model
{
    public class StockDownloader
    {
        private HttpClient Client = new HttpClient();

        public static string[] Slices => new string[]
        {
            "year1month1",
            "year1month2",
            "year1month3",
            "year1month4",
            "year1month5",
            "year1month6",

            "year1month7",
            "year1month8",
            "year1month9",
            "year1month10",
            "year1month11",
            "year1month12",
            
            "year2month1",
            "year2month2",
            "year2month3",
            "year2month4",
            "year2month5",
            "year2month6",
                 
            "year2month7",
            "year2month8",
            "year2month9",
            "year2month10",
            "year2month11",
            "year2month12",
        };

        public async Task DownloadTwoYears(string symbol)
        {
            /*
            // Step 0 | Download First
            var Stream_00 = DownlaodToArray(symbol, 0).Result;

            // Step 1 | Download Second Convert First
            var Stream_01 = DownlaodToArray(symbol, 1).Result;
            var Array_00 = CSVDecoder(Stream_00);

            // Step 2 | Download Third, Convert Second, Save Third
            BinaryFileOperations.SaveToBinaryFile(Array_00, symbol);
            var Stream_02 = DownlaodToArray(symbol, 2);
            var Array_01 = CSVDecoder(Stream_01);
            */

            // Local Variables
            Stream ConvertNext = null;
            StockMoment[] SaveNext = null;
            
            // Actual Function
            for (int i = 0; i < Slices.Length; i++)
            {
                var T = new Task<object>[3];

                // Download
                T[0] = Task.Run(() => 
                {
                    if (symbol == null)
                    {
                        return null;
                    }
                    return (object)DownlaodToArray(symbol, i).Result; ;
                });

                // CSV Decode
                T[1] = Task<object>.Run(() => 
                {
                    lock (ConvertNext) 
                    {
                        if (ConvertNext == null)
                        {
                            return null;
                        }
                        // Return SaveNextArray
                        return (object)CSVDecoder(ConvertNext); 
                    }
                });

                // Save To Binary
                T[2] = Task<object>.Run(() => 
                { 
                    lock (SaveNext)
                    {
                        if(SaveNext == null)
                        {
                            return null;
                        }
                        BinaryFileOperations.SaveToBinaryFile(SaveNext, symbol);
                        
                        // Does not return anything. Create new "object" so we dont return null;
                        return new object(); 
                    }
                });

                // Wait until all Tasks are done.
                await Task.WhenAll(T);

                // Reasign Variables for next loop
                var NoNull = false; // Stays false if all are null
                if (T[0].Result != null)
                {
                    ConvertNext = (Stream)T[0].Result;
                    NoNull = true;
                }
                if (T[1].Result != null)
                {
                    SaveNext = (StockMoment[])T[1].Result;
                    NoNull = true;
                }
                if(T[2].Result != null)
                {
                    NoNull = true;
                }
                if(NoNull == false)
                {
                    break;
                }
            }
            // 3
        }

        public async Task<Stream> DownlaodToArray(string symbol, int slice = 0)
        {
            StringBuilder ApiCommand = new StringBuilder();
            ApiCommand.Append($"https://www.alphavantage.co/query?");       // Adress start of query
            ApiCommand.Append($"function=TIME_SERIES_INTRADAY_EXTENDED");   // Function
            ApiCommand.Append($"&symbol={symbol}");                         // Symbol
            ApiCommand.Append($"&interval=1min");                           // interval
            ApiCommand.Append($"&slice={Slices[slice]}");                   // slice
            ApiCommand.Append($"&adjusted=false");                          // not adjusted
            ApiCommand.Append($"&apikey={App.APIKEY}");                     // API Key

            var x = await Client.GetAsync(ApiCommand.ToString());
            var s = await x.Content.ReadAsStreamAsync();

            return s;
        }

        public StockMoment[] CSVDecoder(Stream s)
        {
            // Initialize Some Variables
            int commaCount = 0;
            int dayCounter = 0;
            byte[] buffer = new byte[1];
            byte YMD = 0;
            int[] QuickDT = new int[6];

            bool Decimal = false;
            double[,] values = new double[6,2];

            // Determine size
            s.Position = 0;
            int NumberOfEntries = 0;
            while (s.Read(buffer, 0, 1) > 0)
            {
                if (buffer[0] == '\n')
                {
                    NumberOfEntries++;
                }
            }
            s.Position = 33;

            StockMoment[] Moments = new StockMoment[NumberOfEntries];

            int DecodedMoments = 0;

            while(s.Read(buffer, 0, 1) > 0)
            {
                if(buffer[0] == ',')
                {
                    // Next Parameter
                    commaCount++;
                    Decimal = false;
                }
                else if(buffer[0] == '\n')
                {
                    // Next Day
                    commaCount = 0;
                    YMD = 0;
                    try
                    {
                        Moments[dayCounter] = new StockMoment() {
                            Time     = ((DateTimeOffset)new DateTime(QuickDT[0], QuickDT[1], QuickDT[2], QuickDT[3], QuickDT[4], QuickDT[5])).ToUnixTimeSeconds(),
                            Open    = values[1, 0] + values[1, 1] / 10,
                            High    = values[2, 0] + values[2, 1] / 10,
                            Low     = values[3, 0] + values[3, 1] / 10,
                            Close   = values[4, 0] + values[4, 1] / 10,
                            Volume  = values[5, 0] + values[5, 1] / 10,
                        };
                        DecodedMoments++;
                    }
                    catch
                    {
                        break;
                    }
                    dayCounter++;
                    values = new double[6,2];
                    QuickDT = new int[6];
                }
                else
                {
                    // Decode Day
                    if(commaCount == 0)
                    {
                        // Decode DateTime
                        if(buffer[0] < '0' || buffer[0] > '9')
                        {
                            YMD++;
                        }
                        else
                        {
                            QuickDT[YMD] *= 10;
                            QuickDT[YMD] += buffer[0] - 48;
                        }
                    }
                    else
                    {
                        // DecodeDouble
                        if (buffer[0] == '.')
                        {
                            Decimal = true;
                        }
                        else
                        {
                            if(buffer[0] >= '0' && buffer[0] <= '9')
                            {
                                if (Decimal == false)
                                {
                                    values[commaCount, 0] *= 10;
                                    values[commaCount, 0] += buffer[0] - 48;
                                }
                                else
                                {
                                    values[commaCount, 1] /= 10;
                                    values[commaCount, 1] += buffer[0] - 48;
                                }
                            }
                        }
                    }
                }
            }
            return Moments;
        }
    }
}
