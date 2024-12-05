# OpenTelemetry Graylog Exporter

[![NuGet](https://img.shields.io/nuget/v/GrayLog.OpenTelemetry.Exporter.svg)](https://www.nuget.org/packages/GrayLog.OpenTelemetry.Exporter)
![GitLab license](https://img.shields.io/badge/license-Apache--2.0-blue)

This repository provides a custom exporter for integrating OpenTelemetry with Graylog. It enables the transmission of telemetry data from .NET applications to Graylog for enhanced observability.

## Installation

To include the OpenTelemetry Graylog Exporter in your .NET project, add the NuGet package reference:

```bash
dotnet add package GrayLog.OpenTelemetry.Exporter
```
### Basic Example

Here’s how you can configure the GrayLog exporter to send telemetry data:

```csharp
var tracerProvider = Sdk.CreateTracerProviderBuilder()
    .AddSource("YourApplicationSource")
    .AddGraylogExporter(options =>
    {
        options.Endpoint = new Uri("http://localhost:12201"); // Replace with your Graylog endpoint
        options.Protocol = GrayLogExportProtocol.Tcp;
    })
    .Build();

var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddOpenTelemetry(options =>
    {
        options.AddGraylogExporter(exporterOptions =>
        {
            exporterOptions.Endpoint = new Uri("http://localhost:12201"); // Replace with your Graylog endpoint
        options.Protocol = GrayLogExportProtocol.Tcp;
        });
    });
});

var logger = loggerFactory.CreateLogger<Program>();
logger.LogInformation("This is a test log message.");
```