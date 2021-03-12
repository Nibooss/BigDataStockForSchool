using Microsoft.Data.Sqlite;
using System;
using System.Collections.ObjectModel;
using System.IO;

namespace StockAnalysis.Model
{
    public class ToSQLite
    {
        FileInfo DBFile => _DBFile ??= new FileInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\StockAnalysis\\sqliteSample.db");
        FileInfo _DBFile;
        public void InitializeDatabase(string symbol)
        {
            if (false == DBFile.Exists)
            {
                var di = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
                di = di.CreateSubdirectory("StockAnalysis");
                DBFile.Create();
            }
            using (SqliteConnection db = new SqliteConnection($"Filename={DBFile.FullName}"))
            {
                db.Open();

                string tableCommand = 
                    $"CREATE TABLE IF NOT EXISTS {symbol}_min" + 
                    $"(Primary_Key INTEGER PRIMARY KEY, " +
                    $"Text_Entry NVARCHAR(2048))";

                SqliteCommand createTable = new SqliteCommand(tableCommand, db);

                createTable.ExecuteReader();
                db.Close();
            }
        }

        public void AddData(string symbol, string inputText)
        {
            using (SqliteConnection db = new SqliteConnection($"Filename={DBFile.FullName}"))
            {
                db.Open();

                SqliteCommand insertCommand = new SqliteCommand();
                insertCommand.Connection = db;

                // Use parameterized query to prevent SQL injection attacks
                insertCommand.CommandText = $"INSERT INTO {symbol + "_Min"} VALUES (NULL, @Entry);";
                insertCommand.Parameters.AddWithValue("@Entry", inputText);

                insertCommand.ExecuteReader();

                db.Close();
            }
        }

        public Collection<string> ReadFromDB(string symbol)
        {
            Collection<string> Entries = new Collection<string>();

            using (SqliteConnection db = new SqliteConnection($"Filename={DBFile.FullName}"))
            {
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand($"SELECT Text_Entry from {symbol + "_Min"}", db);

                SqliteDataReader query = selectCommand.ExecuteReader();

                while (query.Read())
                {
                    Entries.Add(query.GetString(0));
                }

                db.Close();
            }

            return Entries;
        }
    }


}
