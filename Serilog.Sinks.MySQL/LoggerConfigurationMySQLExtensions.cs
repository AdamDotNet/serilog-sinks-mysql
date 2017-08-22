// Copyright (c) Adam Venezia. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
// Modified source from https://github.com/b00ted/serilog-sinks-postgresql to connect to MySQL instead of PostgreSQL.

using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.MySQL;
using System;
using System.Collections.Generic;

namespace Serilog
{
    public static class LoggerConfigurationMySQLExtensions
    {
        /// <summary>
        /// Default time to wait between checking for event batches.
        /// </summary>
        public static readonly TimeSpan DefaultPeriod = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Adds a sink which writes to MySQL table
        /// </summary>
        /// <param name="sinkConfiguration">The logger configuration.</param>
        /// <param name="connectionString">The connection string to the database where to store the events.</param>
        /// <param name="tableName">Name of the table to store the events in.</param>
        /// <param name="columnOptions">Table columns writers</param>
        /// <param name="restrictedToMinimumLevel">The minimum log event level required in order to write an event to the sink.</param>
        /// <param name="period">The time to wait between checking for event batches.</param>
        /// <param name="formatProvider">Supplies culture-specific formatting information, or null.</param>
        /// <param name="batchSizeLimit">The maximum number of events to include to single batch.</param>
        /// <param name="queueLimit">Maximum number of events in the queue.</param>
        /// <param name="levelSwitch">A switch allowing the pass-through minimum level to be changed at runtime.</param>
        /// <returns>Logger configuration, allowing configuration to continue.</returns>
        public static LoggerConfiguration MySQL(this LoggerSinkConfiguration sinkConfiguration,
            string connectionString,
            string tableName,
            IDictionary<string, ColumnWriterBase> columnOptions = null,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            TimeSpan? period = null,
            IFormatProvider formatProvider = null,
            int batchSizeLimit = MySQLSink.DefaultBatchSizeLimit,
            int queueLimit = MySQLSink.DefaultQueueLimit,
            LoggingLevelSwitch levelSwitch = null)
        {
            if (sinkConfiguration == null)
            {
                throw new ArgumentNullException(nameof(sinkConfiguration));
            }


            period = period ?? DefaultPeriod;

            return sinkConfiguration.Sink(new MySQLSink(connectionString,
                                                                tableName,
                                                                period.Value,
                                                                formatProvider,
                                                                columnOptions,
                                                                batchSizeLimit,
                                                                queueLimit),
                                                            restrictedToMinimumLevel, levelSwitch);
        }

    }
}
