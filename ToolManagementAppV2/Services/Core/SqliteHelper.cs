using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Threading;
using System.Threading.Tasks;

namespace ToolManagementAppV2.Services.Core
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

        public static int ExecuteNonQuery(SQLiteConnection conn, string sql, SQLiteParameter[] parameters = null)
        {
            using var cmd = new SQLiteCommand(sql, conn);
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

        public static object ExecuteScalar(SQLiteConnection conn, string sql, SQLiteParameter[] parameters = null)
        {
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

        public static List<T> ExecuteReader<T>(SQLiteConnection conn, string sql, SQLiteParameter[] parameters, Func<IDataRecord, T> map)
        {
            var list = new List<T>();
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

        public static bool IndexExists(string connStr, string indexName)
        {
            using var conn = new SQLiteConnection(connStr);
            conn.Open();
            return IndexExists(conn, indexName);
        }

        public static bool IndexExists(SQLiteConnection conn, string indexName)
        {
            using var cmd = new SQLiteCommand("SELECT name FROM sqlite_master WHERE type='index' AND name=@name", conn);
            cmd.Parameters.AddWithValue("@name", indexName);
            using var rdr = cmd.ExecuteReader();
            return rdr.Read();
        }

        public static async Task<int> ExecuteNonQueryAsync(string connStr, string sql, SQLiteParameter[] parameters = null, CancellationToken cancellationToken = default)
        {
            using var conn = new SQLiteConnection(connStr);
            await conn.OpenAsync(cancellationToken);
            using var cmd = new SQLiteCommand(sql, conn);
            if (parameters != null) cmd.Parameters.AddRange(parameters);
            return await cmd.ExecuteNonQueryAsync(cancellationToken);
        }

        public static async Task<int> ExecuteNonQueryAsync(SQLiteConnection conn, SQLiteTransaction tx, string sql, SQLiteParameter[] parameters, CancellationToken cancellationToken = default)
        {
            using var cmd = new SQLiteCommand(sql, conn, tx);
            if (parameters != null) cmd.Parameters.AddRange(parameters);
            return await cmd.ExecuteNonQueryAsync(cancellationToken);
        }

        public static async Task<int> ExecuteNonQueryAsync(SQLiteConnection conn, string sql, SQLiteParameter[] parameters = null, CancellationToken cancellationToken = default)
        {
            using var cmd = new SQLiteCommand(sql, conn);
            if (parameters != null) cmd.Parameters.AddRange(parameters);
            return await cmd.ExecuteNonQueryAsync(cancellationToken);
        }

        public static async Task<object> ExecuteScalarAsync(string connStr, string sql, SQLiteParameter[] parameters = null, CancellationToken cancellationToken = default)
        {
            using var conn = new SQLiteConnection(connStr);
            await conn.OpenAsync(cancellationToken);
            using var cmd = new SQLiteCommand(sql, conn);
            if (parameters != null) cmd.Parameters.AddRange(parameters);
            return await cmd.ExecuteScalarAsync(cancellationToken);
        }

        public static async Task<object> ExecuteScalarAsync(SQLiteConnection conn, string sql, SQLiteParameter[] parameters = null, CancellationToken cancellationToken = default)
        {
            using var cmd = new SQLiteCommand(sql, conn);
            if (parameters != null) cmd.Parameters.AddRange(parameters);
            return await cmd.ExecuteScalarAsync(cancellationToken);
        }

        public static async Task<List<T>> ExecuteReaderAsync<T>(string connStr, string sql, SQLiteParameter[] parameters, Func<IDataRecord, T> map, CancellationToken cancellationToken = default)
        {
            var list = new List<T>();
            using var conn = new SQLiteConnection(connStr);
            await conn.OpenAsync(cancellationToken);
            using var cmd = new SQLiteCommand(sql, conn);
            if (parameters != null) cmd.Parameters.AddRange(parameters);
            using var rdr = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await rdr.ReadAsync(cancellationToken))
                list.Add(map(rdr));
            return list;
        }

        public static async Task<List<T>> ExecuteReaderAsync<T>(SQLiteConnection conn, string sql, SQLiteParameter[] parameters, Func<IDataRecord, T> map, CancellationToken cancellationToken = default)
        {
            var list = new List<T>();
            using var cmd = new SQLiteCommand(sql, conn);
            if (parameters != null) cmd.Parameters.AddRange(parameters);
            using var rdr = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await rdr.ReadAsync(cancellationToken))
                list.Add(map(rdr));
            return list;
        }
    }
}
