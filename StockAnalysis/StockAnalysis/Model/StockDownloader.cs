using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StockAnalysis.Model
{
    public class StockDownloader
    {
        public MProgress StockDownloaderProgress { get; set; } = new MProgress() { Name = "Download:" };
        public MProgress StockDecoderProgress { get; set; } = new MProgress() { Name = "Decoder:" };
        public MProgress StockSaverProgress { get; set; } = new MProgress() { Name = "Saver:" };
        public MProgress AllProgress { get; set; } = new MProgress() { Name = "All:" };

        private static Task LastDBTask = Task.CompletedTask;
        private static Task<string> LastDonloadTask = Task.Run(() => { return string.Empty; });
        private static Task LastToListTask = Task.CompletedTask;

        private static HttpClient Client = new HttpClient();

        static readonly string[] Slices = new string[]
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

        static readonly TimeSpan TS_WAIT_TIME = TimeSpan.FromSeconds(13);

        public async Task<List<StockMoment>> DownloadTwoYears(string symbolName)
        {
            // Precondition
            if(symbolName == null)
            {
                return null;
            }

            StockDownloaderProgress.Start(Slices.Length);
            StockDecoderProgress.Start(Slices.Length);
            StockSaverProgress.Start(Slices.Length);
            AllProgress.Start(Slices.Length * 4);

            var retValue = new List<StockMoment>();
            // Download
            for (int i = 0; i < Slices.Length; i++)
            {
                int ThisLoopsSlice = i;
                LastDonloadTask = LastDonloadTask.ContinueWith(t =>
                {
                    var DLEnd = DateTime.Now + TS_WAIT_TIME;

                    var WaitProgress = 0.0;
                    var DownloadProgress = 0.0;
                    var ReturnTask = Task.Run(() =>
                    {
                        return DownlaodToString(symbolName, i, (dwp) =>
                        {
                            DownloadProgress = dwp;
                        });
                    });

                    while (DLEnd > DateTime.Now)
                    {
                        Task.Delay(30).Wait();

                        WaitProgress = DLEnd.Subtract(DateTime.Now).TotalSeconds / TS_WAIT_TIME.TotalSeconds;
                        StockDownloaderProgress.SetProgress(i + (WaitProgress > DownloadProgress ? DownloadProgress : WaitProgress));
                    }
                    return ReturnTask.Result;
                });

                var TDecoder = LastDonloadTask.ContinueWith(t =>
                {
                    StockDecoderProgress.Advance();
                    AllProgress.Advance();
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
                    ToSQLite.AddData(symbolName, TDecoder.Result);
                    StockSaverProgress.Advance();
                    AllProgress.Advance();
                });
            }


            Task.WaitAll(new Task[] { LastDBTask, LastDonloadTask, LastToListTask });

            StockDownloaderProgress.Done();
            StockDecoderProgress.Done();
            StockSaverProgress.Done();
            AllProgress.Done();

            return retValue;
        }
        
        
        public static string DownlaodToString(string symbol, int slice = 0, Action<double> ProgressCallback = null)
        {
            string ApiCommand =
            $"https://www.alphavantage.co/query?" +     // Adress start of query
            $"function=TIME_SERIES_INTRADAY_EXTENDED" + // Function
            $"&symbol={symbol}" +                       // Symbol
            $"&interval=1min" +                         // interval
            $"&slice={Slices[slice]}" +                 // slice
            $"&adjusted=false" +                        // not adjusted
            $"&apikey={App.APIKEY}";                    // API Key

            var response = Client.GetAsync(ApiCommand.ToString(), HttpCompletionOption.ResponseHeadersRead).Result;
            response.EnsureSuccessStatusCode();
            var FileLength = response.Content.Headers.ContentLength;

            var stream = response.Content.ReadAsStreamAsync().Result;

            var totalBytesRead = 0L;
            var readCount = 0L;
            var buffer = new byte[8192];
            var isMoreToRead = true;
            var sb = new StringBuilder();


            while (isMoreToRead)
            {
                var bytesRead = stream.ReadAsync(buffer, 0, buffer.Length).Result;
                if (bytesRead == 0)
                {
                    // Downlaod is done
                    isMoreToRead = false;
                    continue;
                }

                sb.Append(Encoding.Default.GetString(buffer));

                totalBytesRead += bytesRead;
                readCount += 1;

                if (readCount % 100 == 0)
                {
                    ProgressCallback?.Invoke((double)bytesRead / (double)FileLength);
                }
            }
            return sb.ToString();
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

    }
}
