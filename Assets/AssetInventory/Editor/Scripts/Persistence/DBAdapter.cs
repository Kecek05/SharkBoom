using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SQLite;

namespace AssetInventory
{
    public static class DBAdapter
    {
        public const string DB_NAME = "AssetInventory.db";

        public static SQLiteConnection DB
        {
            get
            {
                if (_db == null) InitDB();
                return _db;
            }
        }

        private static SQLiteConnection _db;

        private static void InitDB()
        {
            _db = new SQLiteConnection(GetDBPath(), SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.FullMutex);
            _db.BusyTimeout = TimeSpan.FromSeconds(10);

            //_db.Trace = true;
            //_db.Tracer += s => Debug.Log(s);

            _db.ExecuteScalar<string>("PRAGMA journal_mode=" + AI.Config.dbJournalMode);
            _db.Execute("PRAGMA case_sensitive_like = false");

            _db.CreateTable<Asset>();
            _db.CreateTable<AssetFile>();
            _db.CreateTable<AssetMedia>();
            _db.CreateTable<AppProperty>();
            _db.CreateTable<Tag>();
            _db.CreateTable<TagAssignment>();
            _db.CreateTable<RelativeLocation>();
            _db.CreateTable<SystemData>();

            _db.CreateIndex("AssetFile", new[] {"Type", "PreviewState", "Path"});
            _db.CreateIndex("Asset", new[] {"Exclude", "AssetSource"});
        }

        public static long GetDBSize()
        {
            return new FileInfo(GetDBPath()).Length;
        }

        public static bool ColumnExists(string tableName, string columnName)
        {
            List<SQLiteConnection.ColumnInfo> cols = DB.GetTableInfo(tableName);
            return cols.Any(c => c.Name == columnName);
        }

        public static long Compact()
        {
            long original = new FileInfo(GetDBPath()).Length;

            DB.Execute("vacuum;");

            return original - new FileInfo(GetDBPath()).Length;
        }

        public static string GetDBPath()
        {
            return IOUtils.PathCombine(AI.GetStorageFolder(), DB_NAME);
        }

        public static bool IsDBOpen()
        {
            return _db != null;
        }

        public static void Close()
        {
            if (_db == null) return;
            _db.Close();
            _db = null;
        }

        public static bool DeleteDB()
        {
            if (IsDBOpen()) Close();
            try
            {
                File.Delete(GetDBPath());
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}