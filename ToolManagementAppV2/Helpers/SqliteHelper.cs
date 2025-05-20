using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;

namespace ToolManagementAppV2.Helpers
{
    public static class SqliteHelper
    {
        public static int ExecuteNonQuery(string connStr, string sql, SQLiteParameter[] parameters = null)
        {
            using var conn = new SQLiteConnection(connStr);
            conn.Open();
            using var cmd = new SQLiteCommand(sql, conn);
            if (parameters != null) cmd.Parameters.AddRange(parameters);
            return cmd.ExecuteNonQuery();
        }

        public static int ExecuteNonQuery(SQLiteConnection conn, SQLiteTransaction tx, string sql, SQLiteParameter[] parameters)
        {
            using var cmd = new SQLiteCommand(sql, conn, tx);
            if (parameters != null) cmd.Parameters.AddRange(parameters);
            return cmd.ExecuteNonQuery();
        }

        public static object ExecuteScalar(string connStr, string sql, SQLiteParameter[] parameters = null)
        {
            using var conn = new SQLiteConnection(connStr);
            conn.Open();
            using var cmd = new SQLiteCommand(sql, conn);
            if (parameters != null) cmd.Parameters.AddRange(parameters);
            return cmd.ExecuteScalar();
        }

        public static List<T> ExecuteReader<T>(string connStr, string sql, SQLiteParameter[] parameters, Func<IDataRecord, T> map)
        {
            var list = new List<T>();
            using var conn = new SQLiteConnection(connStr);
            conn.Open();
            using var cmd = new SQLiteCommand(sql, conn);
            if (parameters != null) cmd.Parameters.AddRange(parameters);
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
                list.Add(map(rdr));
            return list;
        }

        public static bool ColumnExists(string connStr, string table, string column)
        {
            using var conn = new SQLiteConnection(connStr);
            conn.Open();
            using var cmd = new SQLiteCommand($"PRAGMA table_info({table})", conn);
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
                if (string.Equals(rdr["name"].ToString(), column, StringComparison.OrdinalIgnoreCase))
                    return true;
            return false;
        }
    }
}
