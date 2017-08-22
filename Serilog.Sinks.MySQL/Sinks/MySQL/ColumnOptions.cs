// Copyright (c) Adam Venezia. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
// Modified source from https://github.com/b00ted/serilog-sinks-postgresql to connect to MySQL instead of PostgreSQL.

using System.Collections.Generic;

namespace Serilog.Sinks.MySQL
{
    public static class ColumnOptions
    {
        public static IDictionary<string, ColumnWriterBase> Default => new Dictionary<string, ColumnWriterBase>
        {
            { DefaultColumnNames.Timestamp, new TimestampColumnWriter() },
            { DefaultColumnNames.Level, new LevelValueColumnWriter() },
            { DefaultColumnNames.RenderedMesssage, new RenderedMessageColumnWriter() },
            { DefaultColumnNames.Exception, new ExceptionColumnWriter() },
            { DefaultColumnNames.MessageTemplate, new MessageTemplateColumnWriter() },
            { DefaultColumnNames.Properties, new PropertiesColumnWriter() }
        };
    }

    public static class DefaultColumnNames
    {
        public const string Timestamp = "timestamp";
        public const string Level = "level";
        public const string RenderedMesssage = "message";
        public const string Exception = "exception";
        public const string MessageTemplate = "message_template";
        public const string Properties = "properties";
    }
}