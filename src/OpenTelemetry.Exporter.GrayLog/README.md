# OpenTelemetry GrayLog Exporter

![GitLab license](https://img.shields.io/badge/license-Apache--2.0-blue)

## Overview

OpenTelemetry tracing exporter for GrayLog.

## Features

- Seamless integration with OpenTelemetry
- Supports GELF format for GrayLog
- Lightweight and easy-to-use configuration
- Compatible with .NET 8.0

### Basic Example

Here’s how you can configure the GrayLog exporter to send telemetry data:

```csharp
var tracerProvider = Sdk.CreateTracerProviderBuilder()
    .AddSource("YourApplicationSource")
    .AddGraylogExporter(options =>
    {
        options.Endpoint = [new Uri("http://localhost:12201")]; // Replace with your Graylog endpoint
        options.Protocol = GrayLogExportProtocol.Tcp;
    })
    .Build();

var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddOpenTelemetry(options =>
    {
        options.AddGraylogExporter(exporterOptions =>
        {
            exporterOptions.Endpoint = [new Uri("http://localhost:12201")]; // Replace with your Graylog endpoint
        options.Protocol = GrayLogExportProtocol.Tcp;
        });
    });
});

var logger = loggerFactory.CreateLogger<Program>();
logger.LogInformation("This is a test log message.");
```