using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;

namespace StockAnalysis.Model
{
    public class ToSQLite
    {
        const string DT_FORMAT = "yyyy-MM-ddTHH:mm:ssZ";

        private static SqliteConnection OpenDatabase
        {
            get
            {
                openDatabase ??= InitOpenDatabase();
                if(openDatabase.State == System.Data.ConnectionState.Closed)
                {
                    openDatabase.Open();
                }
                return openDatabase;
            }
        }
        private static SqliteConnection openDatabase;
        private static SqliteConnection InitOpenDatabase()
        {
            var DBFile = new FileInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\StockAnalysis\\sqliteSample.db");
            if (false == DBFile.Exists)
            {
                var di = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
                di = di.CreateSubdirectory("StockAnalysis");
                DBFile.Create();
            }
            var csb = new SqliteConnectionStringBuilder();
            csb.DataSource = DBFile.FullName;
            var db = new SqliteConnection(csb.ConnectionString);
            db.Open();
            return db;
        }

        public void CloseDatabase()
        {
            if(OpenDatabase.State == System.Data.ConnectionState.Open)
            {
                openDatabase.Close();
            }
        }

        public void InitializeDatabase(string symbol)
        {
            string tableCommand =
                $"CREATE TABLE IF NOT EXISTS {symbol}_Min" +
                $"(" +
                $"Primary_Key INTEGER PRIMARY KEY, " +
                $"Time Text," +
                $"Open REAL," +
                $"High REAL," +
                $"Low REAL," +
                $"Close REAL," +
                $"Volume REAL" +
                $")";
                SqliteCommand createTable = new SqliteCommand(tableCommand, OpenDatabase);
                createTable.ExecuteReader();
            CloseDatabase();
        }
        public void RemoveData(string symbol)
        {
                string tableCommand = $"DROP TABLE [IF EXISTS] {symbol}_Min";

                SqliteCommand createTable = new SqliteCommand(tableCommand, OpenDatabase);

                createTable.ExecuteReader();
            CloseDatabase();
        }

        public void AddData(string symbol, StockMoment input)
        {
            SqliteCommand insertCommand = new SqliteCommand();
            insertCommand.Connection = OpenDatabase;

            insertCommand.CommandText = $"INSERT INTO {symbol + "_Min"} VALUES (NULL, @Time, @Open, @High, @Low, @Close, @Volume);";
            insertCommand.Parameters.AddWithValue("@Time",      input.Time.ToString(DT_FORMAT));
            insertCommand.Parameters.AddWithValue("@Open",      input.Open);
            insertCommand.Parameters.AddWithValue("@High",      input.High);
            insertCommand.Parameters.AddWithValue("@Low",       input.Low);
            insertCommand.Parameters.AddWithValue("@Close",     input.Close);
            insertCommand.Parameters.AddWithValue("@Volume",    input.Volume);

            insertCommand.ExecuteReader();
            CloseDatabase();
        }

        public void AddData(string symbol, IEnumerable<StockMoment> input)
        {
            SqliteCommand insertCommand = new SqliteCommand();
            insertCommand.CommandText = $"INSERT INTO {symbol + "_Min"} VALUES (NULL, @Time, @Open, @High, @Low, @Close, @Volume);";
            foreach(var sm in input)
            {
                insertCommand.Connection = OpenDatabase;
                insertCommand.Parameters.AddWithValue("@Time",      sm.Time.ToString(DT_FORMAT));
                insertCommand.Parameters.AddWithValue("@Open",      sm.Open);
                insertCommand.Parameters.AddWithValue("@High",      sm.High);
                insertCommand.Parameters.AddWithValue("@Low",       sm.Low);
                insertCommand.Parameters.AddWithValue("@Close",     sm.Close);
                insertCommand.Parameters.AddWithValue("@Volume",    sm.Volume);

                insertCommand.ExecuteReader();
                CloseDatabase();
            }            
        }


        public IEnumerable<StockMoment> ReadFromDB(string symbol)
        {                
            SqliteCommand selectCommand = new SqliteCommand($"SELECT * from {symbol + "_Min"}", OpenDatabase);

            SqliteDataReader query = selectCommand.ExecuteReader();

            while (query.Read())
            {
                var sm      = new StockMoment();
                //sm.Time     = DateTime.Parse((string)query.GetValue(0));
                sm.Open     = (double)query.GetValue(1);
                sm.High     = (double)query.GetValue(2);
                sm.Low      = (double)query.GetValue(3);
                sm.Close    = (double)query.GetValue(4);
                sm.Volume   = (double)query.GetValue(5);
                yield return sm;
            }
            CloseDatabase();
        }
    }
}
