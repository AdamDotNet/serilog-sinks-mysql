// Copyright (c) Adam Venezia. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
// Modified source from https://github.com/b00ted/serilog-sinks-postgresql to connect to MySQL instead of PostgreSQL.

using Serilog.Events;
using Serilog.Formatting.Json;
using System;
using System.Data;
using System.Text;

namespace Serilog.Sinks.MySQL
{
    public abstract class ColumnWriterBase
    {
        /// <summary>
        /// Column type
        /// </summary>
        public DbType DbType { get; }

        protected ColumnWriterBase(DbType dbType)
        {
            DbType = dbType;
        }

        /// <summary>
        /// Gets part of log event to write to the column
        /// </summary>
        /// <param name="logEvent"></param>
        /// <param name="formatProvider"></param>
        /// <returns></returns>
        public abstract object GetValue(LogEvent logEvent, IFormatProvider formatProvider = null);
    }

    /// <summary>
    /// Writes timestamp part
    /// </summary>
    public class TimestampColumnWriter : ColumnWriterBase
    {
        public TimestampColumnWriter(DbType dbType = DbType.DateTimeOffset)
            : base(dbType) { }

        public override object GetValue(LogEvent logEvent, IFormatProvider formatProvider = null)
        {
            return logEvent.Timestamp;
        }
    }

    /// <summary>
    /// Writes message part
    /// </summary>
    public class RenderedMessageColumnWriter : ColumnWriterBase
    {
        public RenderedMessageColumnWriter(DbType dbType = DbType.String)
            : base(dbType) { }

        public override object GetValue(LogEvent logEvent, IFormatProvider formatProvider = null)
        {
            return logEvent.RenderMessage(formatProvider);
        }
    }

    /// <summary>
    /// Writes non rendered message
    /// </summary>
    public class MessageTemplateColumnWriter : ColumnWriterBase
    {
        public MessageTemplateColumnWriter(DbType dbType = DbType.String)
            : base(dbType) { }

        public override object GetValue(LogEvent logEvent, IFormatProvider formatProvider = null)
        {
            return logEvent.MessageTemplate.Text;
        }
    }

    /// <summary>
    /// Writes log level as a number
    /// </summary>
    public class LevelValueColumnWriter : ColumnWriterBase
    {
        public LevelValueColumnWriter(DbType dbType = DbType.Int32)
            : base(dbType) { }

        public override object GetValue(LogEvent logEvent, IFormatProvider formatProvider = null)
        {
            return (int)logEvent.Level;
        }
    }

    /// <summary>
    /// Writes log level as text
    /// </summary>
    public class LevelTextColumnWriter : ColumnWriterBase
    {
        public LevelTextColumnWriter(DbType dbType = DbType.String)
            : base(dbType) { }

        public override object GetValue(LogEvent logEvent, IFormatProvider formatProvider = null)
        {
            return logEvent.Level.ToString();
        }
    }

    /// <summary>
    /// Writes exception (just calls ToString())
    /// </summary>
    public class ExceptionColumnWriter : ColumnWriterBase
    {
        public ExceptionColumnWriter(DbType dbType = DbType.String)
            : base(dbType) { }

        public override object GetValue(LogEvent logEvent, IFormatProvider formatProvider = null)
        {
            return logEvent.Exception == null ? (object)DBNull.Value : logEvent.Exception.ToString();
        }
    }

    /// <summary>
    /// Writes all event properties as json
    /// </summary>
    public class PropertiesColumnWriter : ColumnWriterBase
    {
        public PropertiesColumnWriter(DbType dbType = DbType.String)
            : base(dbType) { }

        public override object GetValue(LogEvent logEvent, IFormatProvider formatProvider = null)
        {
            return PropertiesToJson(logEvent);
        }

        private object PropertiesToJson(LogEvent logEvent)
        {
            if (logEvent.Properties.Count == 0)
                return "{}";

            var valuesFormatter = new JsonValueFormatter();

            var sb = new StringBuilder();

            sb.Append("{");

            using (var writer = new System.IO.StringWriter(sb))
            {
                foreach (var logEventProperty in logEvent.Properties)
                {
                    sb.Append($"\"{logEventProperty.Key}\":");

                    valuesFormatter.Format(logEventProperty.Value, writer);

                    sb.Append(", ");
                }
            }

            sb.Remove(sb.Length - 2, 2);
            sb.Append("}");

            return sb.ToString();
        }
    }

    /// <summary>
    /// Writes log event as json
    /// </summary>
    public class LogEventSerializedColumnWriter : ColumnWriterBase
    {
        public LogEventSerializedColumnWriter(DbType dbType = DbType.String)
            : base(dbType) { }

        public override object GetValue(LogEvent logEvent, IFormatProvider formatProvider = null)
        {
            return LogEventToJson(logEvent, formatProvider);
        }

        private object LogEventToJson(LogEvent logEvent, IFormatProvider formatProvider)
        {
            var jsonFormatter = new JsonFormatter(formatProvider: formatProvider);

            var sb = new StringBuilder();
            using (var writer = new System.IO.StringWriter(sb))
                jsonFormatter.Format(logEvent, writer);
            return sb.ToString();
        }
    }

    /// <summary>
    /// Write single event property
    /// </summary>
    public class SinglePropertyColumnWriter : ColumnWriterBase
    {
        public string Name { get; }
        public PropertyWriteMethod WriteMethod { get; }
        public string Format { get; }

        public SinglePropertyColumnWriter(string propertyName, PropertyWriteMethod writeMethod = PropertyWriteMethod.ToString, 
                                            DbType dbType = DbType.String, string format = null)
            : base(dbType)
        {
            Name = propertyName;
            WriteMethod = writeMethod;
            Format = format;
        }

        public override object GetValue(LogEvent logEvent, IFormatProvider formatProvider = null)
        {
            if (!logEvent.Properties.ContainsKey(Name))
            {
                return null;
            }

            switch (WriteMethod)
            {
                case PropertyWriteMethod.Raw:
                    return logEvent.Properties[Name];
                case PropertyWriteMethod.Json:
                    var valuesFormatter = new JsonValueFormatter();

                    var sb = new StringBuilder();

                    using (var writer = new System.IO.StringWriter(sb))
                    {
                        valuesFormatter.Format(logEvent.Properties[Name], writer);
                    }

                    return sb.ToString();

                default:
                    return logEvent.Properties[Name].ToString(Format, formatProvider);
            }

        }
    }

    public enum PropertyWriteMethod
    {
        Raw,
        ToString,
        Json
    }
}