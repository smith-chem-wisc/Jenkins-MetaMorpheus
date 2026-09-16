using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Auditor
{
    public class SqliteRunStore : IDisposable
    {
        private const int SchemaVersion = 1;
        private readonly SQLiteConnection connection;

        private sealed class MetricColumn
        {
            public string Header;
            public string SqlName;
            public string SqlType;
            public PropertyInfo Property;
        }

        private static readonly MetricColumn[] MetricColumns = BuildMetricColumns();

        public SqliteRunStore(string databasePath)
        {
            string directory = Path.GetDirectoryName(Path.GetFullPath(databasePath));
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            connection = new SQLiteConnection(string.Format(CultureInfo.InvariantCulture,
                "Data Source={0};Version=3;Pooling=True;", databasePath));
            connection.Open();

            using (var pragma = connection.CreateCommand())
            {
                pragma.CommandText = "PRAGMA journal_mode = WAL;";
                pragma.ExecuteNonQuery();
            }
            using (var pragma = connection.CreateCommand())
            {
                pragma.CommandText = "PRAGMA busy_timeout = 5000;";
                pragma.ExecuteNonQuery();
            }

            EnsureSchema();
        }

        private static MetricColumn[] BuildMetricColumns()
        {
            return typeof(MetaMorpheusRunResult)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.GetCustomAttributes(typeof(IgnoreAttribute), false).Length == 0)
                .Where(p => p.Name != nameof(MetaMorpheusRunResult.DateTime))
                .Select(p =>
                {
                    NameAttribute name = (NameAttribute)p.GetCustomAttributes(typeof(NameAttribute), false).FirstOrDefault();
                    string header = name != null ? name.Names[0] : p.Name;
                    string sqlType;
                    if (p.PropertyType == typeof(int?))
                        sqlType = "INTEGER";
                    else if (p.PropertyType == typeof(double?))
                        sqlType = "REAL";
                    else
                        sqlType = "TEXT";
                    return new MetricColumn { Header = header, SqlName = Sanitize(header), SqlType = sqlType, Property = p };
                })
                .ToArray();
        }

        private static string Sanitize(string name)
        {
            return string.Concat(name.Select(c => char.IsLetterOrDigit(c) ? c : '_'));
        }

        private void EnsureSchema()
        {
            string metricColumns = string.Join(", ", MetricColumns
                .Select(m => string.Format(CultureInfo.InvariantCulture, "\"{0}\" {1}", m.SqlName, m.SqlType)));

            using (var command = connection.CreateCommand())
            {
                command.CommandText = string.Format(CultureInfo.InvariantCulture, @"
CREATE TABLE IF NOT EXISTS runs (
    date TEXT PRIMARY KEY,
    created_at TEXT,
    updated_at TEXT,
    parse_ok INTEGER,
    {0}
);", metricColumns);
                command.ExecuteNonQuery();
            }

            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
CREATE TABLE IF NOT EXISTS run_sources (
    run_id INTEGER PRIMARY KEY AUTOINCREMENT,
    date TEXT NOT NULL,
    label TEXT NOT NULL,
    folder_name TEXT,
    raw_allresults TEXT,
    mm_version TEXT,
    parse_ok INTEGER NOT NULL DEFAULT 1,
    warnings TEXT,
    UNIQUE(date, label)
);
CREATE INDEX IF NOT EXISTS idx_sources_label ON run_sources(label);";
                command.ExecuteNonQuery();
            }

            using (var command = connection.CreateCommand())
            {
                command.CommandText = string.Format(CultureInfo.InvariantCulture,
                    "PRAGMA user_version = {0};", SchemaVersion);
                command.ExecuteNonQuery();
            }
        }

        public void UpsertRuns(IEnumerable<MetaMorpheusRunResult> results)
        {
            foreach (var run in results.Where(r => r != null))
            {
                using (SQLiteTransaction transaction = connection.BeginTransaction())
                {
                    string dateKey = GetRunDateKey(run);
                    UpsertRun(run, dateKey, transaction);
                    UpsertSources(run, dateKey, transaction);
                    transaction.Commit();
                }
            }
        }

        private static string GetRunDateKey(MetaMorpheusRunResult run)
        {
            FileInfo file = run.AllResultsTexts.Values.FirstOrDefault(f => f != null);
            if (file != null && file.Directory != null)
            {
                string folderName = file.Directory.Name;
                int start = folderName.IndexOf('[');
                if (start >= 0)
                {
                    int end = folderName.LastIndexOf(']');
                    string timestamp = end > start
                        ? folderName.Substring(start + 1, end - start - 1)
                        : folderName.Substring(start + 1);
                    if (timestamp.Length > 0)
                        return timestamp;
                }
            }
            return run.DateTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        }

        private void UpsertRun(MetaMorpheusRunResult run, string date, SQLiteTransaction transaction)
        {
            string now = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
            int parseOk = run.AllResultsTexts.Any(p => p.Value != null) ? 1 : 0;

            string setClause = string.Join(", ", MetricColumns.Select(m =>
                string.Format(CultureInfo.InvariantCulture, "\"{0}\" = excluded.\"{0}\"", m.SqlName)));

            string sql = string.Format(CultureInfo.InvariantCulture, @"
INSERT INTO runs (date, created_at, updated_at, parse_ok, {0})
VALUES (@date, @created_at, @updated_at, @parse_ok, {1})
ON CONFLICT(date) DO UPDATE SET
    updated_at = excluded.updated_at,
    parse_ok = excluded.parse_ok,
    {2};",
                string.Join(", ", MetricColumns.Select(m => string.Format(CultureInfo.InvariantCulture, "\"{0}\"", m.SqlName))),
                string.Join(", ", MetricColumns.Select((m, i) => "@p" + i)),
                setClause);

            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = sql;
                command.Parameters.AddWithValue("@date", date);
                command.Parameters.AddWithValue("@created_at", now);
                command.Parameters.AddWithValue("@updated_at", now);
                command.Parameters.AddWithValue("@parse_ok", parseOk);
                for (int i = 0; i < MetricColumns.Length; i++)
                    command.Parameters.AddWithValue("@p" + i, (object)MetricColumns[i].Property.GetValue(run) ?? DBNull.Value);
                command.ExecuteNonQuery();
            }
        }

        private void UpsertSources(MetaMorpheusRunResult run, string date, SQLiteTransaction transaction)
        {
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = @"
INSERT INTO run_sources (date, label, folder_name, raw_allresults, parse_ok, warnings)
VALUES (@date, @label, @folder_name, @raw_allresults, @parse_ok, @warnings)
ON CONFLICT(date, label) DO UPDATE SET
    folder_name = excluded.folder_name,
    raw_allresults = excluded.raw_allresults,
    parse_ok = excluded.parse_ok,
    warnings = excluded.warnings;";

                foreach (var labelToFile in run.AllResultsTexts)
                {
                    string label = labelToFile.Key;
                    FileInfo file = labelToFile.Value;

                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@date", date);
                    command.Parameters.AddWithValue("@label", label);
                    command.Parameters.AddWithValue("@folder_name",
                        file != null ? file.Directory.Name : (object)DBNull.Value);
                    command.Parameters.AddWithValue("@raw_allresults",
                        file != null ? File.ReadAllText(file.FullName) : (object)DBNull.Value);
                    command.Parameters.AddWithValue("@parse_ok", file != null ? 1 : 0);
                    command.Parameters.AddWithValue("@warnings", file != null ? DBNull.Value : (object)"allResults.txt not found");
                    command.ExecuteNonQuery();
                }
            }
        }

        public void Dispose()
        {
            if (connection != null)
                connection.Dispose();
        }
    }
}