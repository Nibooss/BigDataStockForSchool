using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace StockAnalysis.Model
{
    // TODO put into ENUM
    // 0 = Open // 1 = High // 2 = Low // 3 = Close // 4 = Volume

    public static class ToSQLite
    {
        const string DT_FORMAT = "yyyy-MM-ddTHH:mm:ssZ";

        private static SqliteConnection OpenDatabase
        {
            get
            {
                openDatabase ??= InitOpenDatabase();
                if (openDatabase.State == System.Data.ConnectionState.Closed)
                {
                    for (int ErrorCount = 0; ErrorCount < 5; ErrorCount++)
                    {
                        try
                        {
                            openDatabase.Open();
                        }
                        catch
                        {
                            Task.Delay(250).Wait();
                        }
                    }
                }
                return openDatabase;
            }
        }
        private static SqliteConnection openDatabase;
        private static SqliteConnection InitOpenDatabase()
        {
            var DBFile = new FileInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\StockAnalysis\\stockanalasys.db");
            if (false == DBFile.Exists)
            {
                var di = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
                di = di.CreateSubdirectory("StockAnalysis");
                DBFile.Create();
            }
            var csb = new SqliteConnectionStringBuilder();
            csb.DataSource = DBFile.FullName;
            var db = new SqliteConnection(csb.ConnectionString);
            for (int ErrorCount = 0; ErrorCount < 5; ErrorCount++)
            {
                try
                {
                    db.Open();
                }
                catch
                {
                    Task.Delay(250).Wait();
                }
            }
            return db;
        }

        public static void CloseDatabase()
        {
            if (OpenDatabase.State == System.Data.ConnectionState.Open)
            {
                openDatabase.Close();
            }
        }

        public static void InitializeDatabase()
        {
            string tableCommand =
                $"CREATE TABLE IF NOT EXISTS MetaTable" +
                $"(" +
                $"Primary_Key INTEGER PRIMARY KEY, " +
                $"Symbol TEXT," + 
                $"Size INTEGER" +
                $")";
            SqliteCommand createTable = new SqliteCommand(tableCommand, OpenDatabase);
            createTable.ExecuteReader();
            CloseDatabase();
        }
        public static void RemoveData(string symbol)
        {
            string tableCommand = $"DROP TABLE [IF EXISTS] {symbol}";

            SqliteCommand createTable = new SqliteCommand(tableCommand, OpenDatabase);

            createTable.ExecuteReader();
            CloseDatabase();
        }
        public static void RemoveData(Symbol symbol)
        {
            RemoveData(symbol.Name);
        }

        public static void AddData(string symbol, StockMoment[] input)
        {
            if(input == null)
            {
                return;
            }

            // Does entry allready exists?
            var selectCommand = new SqliteCommand($"SELECT * from MetaTable WHERE Symbol LIKE '{symbol}'", OpenDatabase);

            SqliteDataReader query = selectCommand.ExecuteReader();

            // If it allready Existed. Remove its table
            if (query.HasRows)
            {
                // Drop actual data
                var removeCommand = new SqliteCommand();
                removeCommand.CommandText = $"DROP TABLE [IF EXISTS] {symbol}";
                removeCommand.ExecuteReader();
            }

            var createTable = new SqliteCommand($"CREATE TABLE IF NOT EXISTS {symbol}" +
                $"(" +
                $"Time TEXT," +
                $"Open REAL," +
                $"High REAL," +
                $"Low REAL," +
                $"Close REAL," +
                $"Volume REAL" +
                $")", OpenDatabase);
            createTable.ExecuteNonQuery();


            SqliteCommand insertCommand = new SqliteCommand();
            insertCommand.Connection = OpenDatabase;
            insertCommand.CommandText = $"INSERT INTO {symbol} (Time, Open, High, Low, Close, Volume) VALUES (@t,@o,@h,@l,@c,@v)";

            // Count how many are inserted
            int DataRowCount = 0;

            foreach(var inp in input)
            {
                if(inp == null)
                {
                    continue;
                }
                // If all those are 0 data is actually empty so do not save it!
                if(0 == inp.Open + inp.High + inp.Low + inp.Close + inp.Volume)
                {
                    continue;
                }
                insertCommand.Parameters.Clear();
                insertCommand.Parameters.AddWithValue("@t",  inp.Time.ToString(DT_FORMAT));
                insertCommand.Parameters.AddWithValue("@o",  inp.Open);
                insertCommand.Parameters.AddWithValue("@h",  inp.High);
                insertCommand.Parameters.AddWithValue("@l",  inp.Low);
                insertCommand.Parameters.AddWithValue("@c",  inp.Close);
                insertCommand.Parameters.AddWithValue("@v",  inp.Volume);
                insertCommand.ExecuteNonQuery();
                DataRowCount++;
            }

            // Override in MetaTable
            var replaceCommand = new SqliteCommand();
            replaceCommand.CommandText = $"REPLACE INTO MetaTable (Symbol, Size) VALUES (@sy,@si)";
            replaceCommand.Parameters.AddWithValue("@sy", symbol);
            replaceCommand.Parameters.AddWithValue("@si", DataRowCount);

            CloseDatabase();
        }
        public static IEnumerable<Symbol> GetAllSymbols()
        {
            var selectCommand = new SqliteCommand($"SELECT * from MetaTable", OpenDatabase);
            var selectQuery = selectCommand.ExecuteReader();

            while (selectQuery.Read())
            {
                yield return new Symbol()
                {
                    Name = selectQuery.GetString(0),
                    Size = selectQuery.GetInt32(1)
                };
            }
        }
        public static Symbol GetSpecificSymbol(string Name)
        {
            var selectCommand = new SqliteCommand($"SELECT * from MetaTable WHERE Name LIKE {Name}", OpenDatabase);
            var selectQuery = selectCommand.ExecuteReader();

            selectQuery.Read();
            return new Symbol()
            {
                Name = selectQuery.GetString(0),
                Size = selectQuery.GetInt32(1)
            };
        }

        public static StockMoment[] ReadFromDB(string symbol)
        {
            // Meta Command
            var MetaCommand = new SqliteCommand($"SELECT * from MetaTable WHERE Symbol LIKE '{symbol}'", OpenDatabase);
            var MetaQuery = MetaCommand.ExecuteReader();
            int NumOfData = MetaQuery.GetInt32(1);

            var Buffer = new StockMoment[NumOfData];

            SqliteCommand selectCommand = new SqliteCommand($"SELECT * from {symbol}", OpenDatabase);

            SqliteDataReader query = selectCommand.ExecuteReader();

            for(int i = 0; i < NumOfData; i++)
            {
                var sm = new StockMoment();
                sm.Time     = DateTime.Parse(query.GetString(0));
                sm.Open = (double)query.GetValue(1);
                sm.High = (double)query.GetValue(2);
                sm.Low = (double)query.GetValue(3);
                sm.Close = (double)query.GetValue(4);
                sm.Volume = (double)query.GetValue(5);

                Buffer[i] = sm;
            }
            return Buffer;
            CloseDatabase();
        }
    }
    
}
