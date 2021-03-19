using StockAnalysis.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace StockAnalysis
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static App app => App.Current as App;
        StockDownloader sd = new StockDownloader();
        ToSQLite sql = new ToSQLite();
        public App()
        {
            InitializeComponent();
            var sma = sd.DownlaodToArray("IBM");
            sql.InitializeDatabase("IBM");
            sql.AddData("IBM", sma);
            var x = sql.ReadFromDB("IBM");
        }

        public string APIKEY { get; set; } = "WEWF8LKFM1UXNU6X";
    }
}