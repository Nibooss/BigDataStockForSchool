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
            // sd.DownlaodToArray(symbol: "IBM");
            sql.InitializeDatabase(symbol: "IBM");
            sql.InitializeDatabase(symbol: "AAPL");
            sql.AddData(symbol:"IBM", inputText:"HelloWorld");
            sql.AddData(symbol:"IBM", inputText:"I am cereal Hello");
            sql.AddData(symbol:"IBM", inputText:"hello again");
            sql.AddData(symbol:"AAPL", inputText:"HelloWorld");
            sql.AddData(symbol:"AAPL", inputText:"HelloWorld");
            var x = sql.ReadFromDB(symbol: "IBM");
            var y = sql.ReadFromDB(symbol: "AAPL");
        }

        public string APIKEY { get; set; }
    }
}