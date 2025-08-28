using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.Sqlite;
using System.Threading;
using System.Threading.Tasks;

namespace InventoryManagementApp.Services.Core
{
    public static class SqliteHelper
    {
        public static int ExecuteNonQuery(string connStr, string sql, SqliteParameter[]? parameters = null)
        {
            using var conn = new SqliteConnection(connStr);
            conn.Open();
            using var cmd = new SqliteCommand(sql, conn);
            if (parameters != null) cmd.Parameters.AddRange(parameters);
            return cmd.ExecuteNonQuery();
        }

        public static int ExecuteNonQuery(SqliteConnection conn, SqliteTransaction tx, string sql, SqliteParameter[]? parameters = null)
        {
            using var cmd = new SqliteCommand(sql, conn, tx);
            if (parameters != null) cmd.Parameters.AddRange(parameters);
            return cmd.ExecuteNonQuery();
        }

        public static int ExecuteNonQuery(SqliteConnection conn, string sql, SqliteParameter[]? parameters = null)
        {
            using var cmd = new SqliteCommand(sql, conn);
            if (parameters != null) cmd.Parameters.AddRange(parameters);
            return cmd.ExecuteNonQuery();
        }

        public static object? ExecuteScalar(string connStr, string sql, SqliteParameter[]? parameters = null)
        {
            using var conn = new SqliteConnection(connStr);
            conn.Open();
            using var cmd = new SqliteCommand(sql, conn);
            if (parameters != null) cmd.Parameters.AddRange(parameters);
            return cmd.ExecuteScalar();
        }

        public static object? ExecuteScalar(SqliteConnection conn, string sql, SqliteParameter[]? parameters = null)
        {
            using var cmd = new SqliteCommand(sql, conn);
            if (parameters != null) cmd.Parameters.AddRange(parameters);
            return cmd.ExecuteScalar();
        }

        public static List<T> ExecuteReader<T>(string connStr, string sql, Func<IDataRecord, T> map, SqliteParameter[]? parameters = null)
        {
            var list = new List<T>();
            using var conn = new SqliteConnection(connStr);
            conn.Open();
            using var cmd = new SqliteCommand(sql, conn);
            if (parameters != null) cmd.Parameters.AddRange(parameters);
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
                list.Add(map(rdr));
            return list;
        }

        public static List<T> ExecuteReader<T>(SqliteConnection conn, string sql, Func<IDataRecord, T> map, SqliteParameter[]? parameters = null)
        {
            var list = new List<T>();
            using var cmd = new SqliteCommand(sql, conn);
            if (parameters != null) cmd.Parameters.AddRange(parameters);
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
                list.Add(map(rdr));
            return list;
        }

        public static bool ColumnExists(string connStr, string table, string column)
        {
            using var conn = new SqliteConnection(connStr);
            conn.Open();
            using var cmd = new SqliteCommand($"PRAGMA table_info({table})", conn);
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
                if (string.Equals(rdr["name"].ToString(), column, StringComparison.OrdinalIgnoreCase))
                    return true;
            return false;
        }

        public static bool IndexExists(string connStr, string indexName)
        {
            using var conn = new SqliteConnection(connStr);
            conn.Open();
            return IndexExists(conn, indexName);
        }

        public static bool IndexExists(SqliteConnection conn, string indexName)
        {
            using var cmd = new SqliteCommand("SELECT name FROM sqlite_master WHERE type='index' AND name=@name", conn);
            cmd.Parameters.AddWithValue("@name", indexName);
            using var rdr = cmd.ExecuteReader();
            return rdr.Read();
        }

        public static async Task<int> ExecuteNonQueryAsync(string connStr, string sql, SqliteParameter[]? parameters = null, CancellationToken cancellationToken = default)
        {
            using var conn = new SqliteConnection(connStr);
            await conn.OpenAsync(cancellationToken);
            using var cmd = new SqliteCommand(sql, conn);
            if (parameters != null) cmd.Parameters.AddRange(parameters);
            return await cmd.ExecuteNonQueryAsync(cancellationToken);
        }

        public static async Task<int> ExecuteNonQueryAsync(SqliteConnection conn, SqliteTransaction tx, string sql, SqliteParameter[]? parameters = null, CancellationToken cancellationToken = default)
        {
            using var cmd = new SqliteCommand(sql, conn, tx);
            if (parameters != null) cmd.Parameters.AddRange(parameters);
            return await cmd.ExecuteNonQueryAsync(cancellationToken);
        }

        public static async Task<int> ExecuteNonQueryAsync(SqliteConnection conn, string sql, SqliteParameter[]? parameters = null, CancellationToken cancellationToken = default)
        {
            using var cmd = new SqliteCommand(sql, conn);
            if (parameters != null) cmd.Parameters.AddRange(parameters);
            return await cmd.ExecuteNonQueryAsync(cancellationToken);
        }

        public static async Task<object?> ExecuteScalarAsync(string connStr, string sql, SqliteParameter[]? parameters = null, CancellationToken cancellationToken = default)
        {
            using var conn = new SqliteConnection(connStr);
            await conn.OpenAsync(cancellationToken);
            using var cmd = new SqliteCommand(sql, conn);
            if (parameters != null) cmd.Parameters.AddRange(parameters);
            return await cmd.ExecuteScalarAsync(cancellationToken);
        }

        public static async Task<object?> ExecuteScalarAsync(SqliteConnection conn, string sql, SqliteParameter[]? parameters = null, CancellationToken cancellationToken = default)
        {
            using var cmd = new SqliteCommand(sql, conn);
            if (parameters != null) cmd.Parameters.AddRange(parameters);
            return await cmd.ExecuteScalarAsync(cancellationToken);
        }

        public static async Task<List<T>> ExecuteReaderAsync<T>(string connStr, string sql, Func<IDataRecord, T> map, SqliteParameter[]? parameters = null, CancellationToken cancellationToken = default)
        {
            var list = new List<T>();
            using var conn = new SqliteConnection(connStr);
            await conn.OpenAsync(cancellationToken);
            using var cmd = new SqliteCommand(sql, conn);
            if (parameters != null) cmd.Parameters.AddRange(parameters);
            using var rdr = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await rdr.ReadAsync(cancellationToken))
                list.Add(map(rdr));
            return list;
        }

        public static async Task<List<T>> ExecuteReaderAsync<T>(SqliteConnection conn, string sql, Func<IDataRecord, T> map, SqliteParameter[]? parameters = null, CancellationToken cancellationToken = default)
        {
            var list = new List<T>();
            using var cmd = new SqliteCommand(sql, conn);
            if (parameters != null) cmd.Parameters.AddRange(parameters);
            using var rdr = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await rdr.ReadAsync(cancellationToken))
                list.Add(map(rdr));
            return list;
        }
    }
}
