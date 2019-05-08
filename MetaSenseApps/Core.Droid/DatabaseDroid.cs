using System;
using System.IO;
using Core.Droid;
using NodeLibrary.Native;
using SQLite;
using Xamarin.Forms;

[assembly: Dependency(typeof(DatabaseDroid))]
namespace Core.Droid
{
    internal sealed class DatabaseDroid : IDatabase
    {
        private static readonly SQLiteConnection StatiConnection;
        static DatabaseDroid()
        {
            const string sqliteFilename = "DataAllSQLite.db3";
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal); // Documents folder
            var path = Path.Combine(documentsPath, sqliteFilename);
            // Create the connection
            StatiConnection = new SQLiteConnection(path);
        }
        public SQLiteConnection Connection => StatiConnection;
    }
}