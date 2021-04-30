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

    class ToSQLITE
    {
        public class ToSQLite
        {
            const string DT_FORMAT = "yyyy-MM-ddTHH:mm:ssZ";

            private static SqliteConnection OpenDatabase
            {
                get
                {
                    openDatabase ??= InitOpenDatabase();
                    if (openDatabase.State == System.Data.ConnectionState.Closed)
                    {
                        openDatabase.Open();
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
                db.Open();
                return db;
            }

            public void CloseDatabase()
            {
                if (OpenDatabase.State == System.Data.ConnectionState.Open)
                {
                    openDatabase.Close();
                }
            }

            public void InitializeDatabase(string symbol)
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
            public void RemoveData(string symbol)
            {
                string tableCommand = $"DROP TABLE [IF EXISTS] {symbol}";

                SqliteCommand createTable = new SqliteCommand(tableCommand, OpenDatabase);

                createTable.ExecuteReader();
                CloseDatabase();
            }

            public void AddData(string symbol, StockMoment[] input)
            {
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
                    $"Primary_Key INTEGER PRIMARY KEY, " +
                    $"Time TEXT," +
                    $"Open BLOB" +
                    $"High BLOB," +
                    $"Low BLOB," +
                    $"Close BLOB," +
                    $"Volume BLOB" +
                    $")", OpenDatabase);
                createTable.ExecuteReader();


                SqliteCommand insertCommand = new SqliteCommand();
                insertCommand.Connection = OpenDatabase;
                insertCommand.CommandText = $"INSERT INTO {symbol} VALUES (NULL, @Time, @Open, @High, @Low, @Close, @Volume);";

                // Count how many are inserted
                int DataRowCount = 0;

                foreach(var inp in input)
                {
                    // If all those are 0 data is actually empty so do not save it!
                    if(0 == inp.Open + inp.High + inp.Low + inp.Close + inp.Volume)
                    {
                        continue;
                    }

                    insertCommand.Parameters.AddWithValue("@Time",      inp.Time.ToString(DT_FORMAT));
                    insertCommand.Parameters.AddWithValue("@Open",      inp.Open);
                    insertCommand.Parameters.AddWithValue("@High",      inp.High);
                    insertCommand.Parameters.AddWithValue("@Low",       inp.Low);
                    insertCommand.Parameters.AddWithValue("@Close",     inp.Close);
                    insertCommand.Parameters.AddWithValue("@Volume",    inp.Volume);
                    insertCommand.ExecuteReader();
                    DataRowCount++;
                }

                // Override in MetaTable
                var replaceCommand = new SqliteCommand();
                replaceCommand.CommandText = $"REPLACE INTO MetaTable VALUES(@Symbol, @Size)";
                replaceCommand.Parameters.AddWithValue("@Symbol", symbol);
                replaceCommand.Parameters.AddWithValue("@Size", DataRowCount);

                CloseDatabase();
            }

            public StockMoment[] ReadFromDB(string symbol)
            {
                // Meta Command
                var MetaCommand = new SqliteCommand($"SELECT * from MetaTable WHERE Symbol LIKE '{symbol}'", OpenDatabase);
                MetaCommand.ExecuteReader();
                var MetaQuery = MetaCommand.ExecuteReader();
                int NumOfData = MetaQuery.GetInt32(1);

                var Buffer = new StockMoment[NumOfData];

                SqliteCommand selectCommand = new SqliteCommand($"SELECT * from {symbol}", OpenDatabase);

                SqliteDataReader query = selectCommand.ExecuteReader();

                for(int i = 0; i < NumOfData; i++)
                {
                    var sm = new StockMoment();
                    //sm.Time     = DateTime.Parse((string)query.GetValue(0));
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
}
