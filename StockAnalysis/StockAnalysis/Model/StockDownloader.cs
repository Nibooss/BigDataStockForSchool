using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StockAnalysis.Model
{
    public class StockDownloader
    {
        public Progress StockDownloaderProgress  { get; set; } = new Progress() { Name = "Download:" };
        public Progress StockDecoderProgress     { get; set; } = new Progress() { Name = "Decoder:" };
        public Progress StockSaverProgress       { get; set; } = new Progress() { Name = "Saver:" };
        public Progress AllProgress              { get; set; } = new Progress() { Name = "All:" };

        private static Task LastDBTask = Task.CompletedTask;
        private static Task<string> LastDonloadTask = Task.Run(() => { return string.Empty; });
        private static Task LastToListTask = Task.CompletedTask;

        private static HttpClient Client = new HttpClient();

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

        public List<StockMoment> DownloadTwoYears(string symbol)
        {
            // Precondition
            if(symbol == null)
            {
                return null;
            }

            int WaitSteps = 52;

            StockDownloaderProgress.Init(Slices.Length * WaitSteps);
            StockDecoderProgress.Init(Slices.Length);
            StockSaverProgress.Init(Slices.Length);
            AllProgress.Init(Slices.Length * 3 + Slices.Length * WaitSteps);

            // Variables
            
            var retValue = new List<StockMoment>();
            // Download
            for (int i = 0; i < Slices.Length; i++)
            {
                int ThisLoopsSlice = i;
                LastDonloadTask = LastDonloadTask.ContinueWith(t =>
                {
                    var DLStart = DateTime.Now;
                    var ret = DownlaodToString(symbol, ThisLoopsSlice);
                    for(int i = 0; i < WaitSteps; i++)
                    {
                        AllProgress.NotifyProgress();
                        StockDownloaderProgress.NotifyProgress();
                        if((i * 250) > (DateTime.Now - DLStart).TotalMilliseconds)
                        {
                            Task.Delay(250).Wait();
                        }
                    }
                    return ret;
                });

                var TDecoder = LastDonloadTask.ContinueWith(t =>
                {
                    StockDecoderProgress.NotifyProgress();
                    AllProgress.NotifyProgress();
                    return CSVDecoderNew(t.Result);
                });

                LastToListTask = Task.WhenAll(new Task[] { TDecoder, LastToListTask }).ContinueWith(t =>
                {
                    lock (retValue)
                    {
                        foreach (var sm in TDecoder.Result)
                        {
                            retValue.Add(sm);
                        }
                    }
                });

                LastDBTask = Task.WhenAll(new Task[] { TDecoder, LastDBTask }).ContinueWith(t =>
                {
                    ToSQLite.AddData(symbol, TDecoder.Result);
                    StockSaverProgress.NotifyProgress();
                    AllProgress.NotifyProgress();
                });
            }


            Task.WaitAll(new Task[] { LastDBTask, LastDonloadTask, LastToListTask });

            StockDownloaderProgress.Done();
            StockDecoderProgress.Done();
            StockSaverProgress.Done();
            AllProgress.Done();

            return retValue;
        }

        

        public string DownlaodToString(string symbol, int slice = 0)
        {
            var stream = DownlaodToStream(symbol, slice);
            var sr = new StreamReader(stream);
            var retString = sr.ReadToEnd();
            sr.Dispose();
            stream.Dispose();
            return retString;
        }

        public Stream DownlaodToStream(string symbol, int slice = 0)
        {
            StringBuilder ApiCommand = new StringBuilder();
            ApiCommand.Append($"https://www.alphavantage.co/query?");       // Adress start of query
            ApiCommand.Append($"function=TIME_SERIES_INTRADAY_EXTENDED");   // Function
            ApiCommand.Append($"&symbol={symbol}");                         // Symbol
            ApiCommand.Append($"&interval=1min");                           // interval
            ApiCommand.Append($"&slice={Slices[slice]}");                   // slice
            ApiCommand.Append($"&adjusted=false");                          // not adjusted
            ApiCommand.Append($"&apikey={App.APIKEY}");                     // API Key

            var x = Client.GetAsync(ApiCommand.ToString());
            var s = x.Result.Content.ReadAsStreamAsync();

            return s.Result;
        }

        public static List<StockMoment> CSVDecoderNew(Stream s)
        {
            var sr = new StreamReader(s);
            return CSVDecoderNew(sr.ReadToEnd());
        }

        public static List<StockMoment> CSVDecoderNew(string s)
        {
            // Precondition checks
            if(s == null)
            {
                return null;
            }

            // Create variables
            // TODO: Move somewhere else
            // For the Extended history files. Should be somewhere else
            var regexPattern = "([0-9- :.]+),([0-9.]+),([0-9.]+),([0-9.]+),([0-9.]+),([0-9.]+)";
            var m = Regex.Match(s, regexPattern);

            var ReturnMoments = new List<StockMoment>();
            while (m.Success)
            {
                var sm = new StockMoment();
                sm.Time     = DateTime.Parse(m.Groups[1].Value);
                sm.Open     = double.Parse(m.Groups[2].Value, CultureInfo.InvariantCulture);
                sm.High     = double.Parse(m.Groups[3].Value, CultureInfo.InvariantCulture);
                sm.Low      = double.Parse(m.Groups[4].Value, CultureInfo.InvariantCulture);
                sm.Close    = double.Parse(m.Groups[5].Value, CultureInfo.InvariantCulture);
                sm.Volume   = double.Parse(m.Groups[6].Value, CultureInfo.InvariantCulture);
                ReturnMoments.Add(sm);
                m = m.NextMatch();
            } 

            return ReturnMoments;
        }

        public static StockMoment[] CSVDecoder(Stream s)
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
                            Time    = new DateTime(QuickDT[0], QuickDT[1], QuickDT[2], QuickDT[3], QuickDT[4], QuickDT[5]),
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
