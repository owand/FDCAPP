using System;
using System.IO;

[assembly: Xamarin.Forms.Dependency(typeof(FDCAPP.iOS.Services.SQLite_iOS))]
namespace FDCAPP.iOS.Services
{
    public class SQLite_iOS : FDCAPP.Services.ISQLite
    {
        public SQLite_iOS()
        {
        }

        public SQLite.SQLiteConnection DbConnection()
        {
            //string personalFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            //string libraryFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "..", "Library");
            string path = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "..", "Library"), App.dbName);

            return new SQLite.SQLiteConnection(path);
        }

        public void CreateDBAsync()
        {
            try
            {
                string path = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "..", "Library"), App.dbName);

                // копирование файла из папки Assets по пути path
                if (!File.Exists(path) || (File.Exists(path) && (GetCurrentDBVersion() < App.dbVersion)))
                {
                    File.Copy(App.dbName, path);
                }
            }
            catch (Exception)
            {
                return;
                //new UIKit.UIAlertView("Sorry!", ex.ToString(), null, "Cancel", null).Show();
            }
        }

        // Get Current Data Base Version
        public int GetCurrentDBVersion()
        {
            int currentDbVersion;

            try
            {
                string path = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "..", "Library"), App.dbName);
                if (File.Exists(path))
                {
                    SQLite.SQLiteConnection database = new SQLite.SQLiteConnection(path);
                    currentDbVersion = database.ExecuteScalar<int>("pragma user_version");
                    database.Close();
                    database.Dispose();
                }
                else
                {
                    currentDbVersion = 0;
                }
                return currentDbVersion;
            }
            catch (Exception)
            {
                currentDbVersion = 0;
                return currentDbVersion;
            }
        }
    }
}