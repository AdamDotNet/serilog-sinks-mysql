// Copyright (c) Adam Venezia. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
// Modified source from https://github.com/b00ted/serilog-sinks-postgresql to connect to MySQL instead of PostgreSQL.

using MySql.Data.MySqlClient;
using Serilog.Events;
using Serilog.Sinks.PeriodicBatching;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Serilog.Sinks.MySQL
{
    public class MySQLSink : PeriodicBatchingSink
    {
        private readonly string _connectionString;
        private readonly string _tableName;
        private readonly IDictionary<string, ColumnWriterBase> _columnOptions;
        private readonly IFormatProvider _formatProvider;

        internal const int DefaultBatchSizeLimit = 30;
        internal const int DefaultQueueLimit = Int32.MaxValue;
        private const char parameterPrefix = '@';

        public MySQLSink(string connectionString,
            string tableName,
            TimeSpan period,
            IFormatProvider formatProvider = null,
            IDictionary<string, ColumnWriterBase> columnOptions = null,
            int batchSizeLimit = DefaultBatchSizeLimit,
            int queueLimit = DefaultQueueLimit)
            : base(batchSizeLimit, period, queueLimit)
        {
            _connectionString = connectionString;
            _tableName = tableName;
            _formatProvider = formatProvider;
            _columnOptions = columnOptions ?? ColumnOptions.Default;
        }

        protected override async Task EmitBatchAsync(IEnumerable<LogEvent> events)
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync().ConfigureAwait(false);
                using (MySqlTransaction transaction = await connection.BeginTransactionAsync().ConfigureAwait(false))
                {
                    await ProcessEventsByInsertStatementsAsync(events, connection).ConfigureAwait(false);
                    await transaction.CommitAsync().ConfigureAwait(false);
                }
            }
        }

        private async Task ProcessEventsByInsertStatementsAsync(IEnumerable<LogEvent> events, MySqlConnection connection)
        {
            using (MySqlCommand command = (MySqlCommand)connection.CreateCommand())
            {
                command.CommandText = GetInsertQuery();
                foreach (LogEvent logEvent in events)
                {
                    command.Parameters.Clear();
                    command.Parameters.AddRange(_columnOptions.Select(kvp => CreateParameter(kvp.Key, kvp.Value.DbType, kvp.Value.GetValue(logEvent, _formatProvider))).ToArray());
                    await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            }
        }

        private string GetInsertQuery()
        {
            string columns = String.Join(", ", _columnOptions.Keys);
            string parameters = String.Join(", ", _columnOptions.Keys.Select(cn => $"{parameterPrefix}{cn}"));
            return $"INSERT INTO {_tableName} ({columns}) VALUES ({parameters})";
        }

        private MySqlParameter CreateParameter(string columnName, DbType dbType, object value) => new MySqlParameter
        {
            DbType = dbType,
            ParameterName = $"{parameterPrefix}{columnName}",
            Value = value
        };
    }
}