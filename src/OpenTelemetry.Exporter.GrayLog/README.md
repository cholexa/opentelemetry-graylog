# OpenTelemetry GrayLog Exporter

![GitLab license](https://img.shields.io/badge/license-Apache--2.0-blue)

## Overview

OpenTelemetry tracing exporter for GrayLog.

## Features

- Seamless integration with OpenTelemetry
- Supports GELF format for GrayLog
- Lightweight and easy-to-use configuration
- Compatible with .NET 8.0

### **3. Basic Usage Example**
Provide a simple, self-contained example of how to use the library/tool. This should be concise but functional.

### Basic Example

Here’s how you can configure the GrayLog exporter to send telemetry data:

```csharp
using OpenTelemetry.Trace;
using OpenTelemetry.Exporter.GrayLog;

var tracerProvider = Sdk.CreateTracerProviderBuilder()
    .AddGrayLogExporter(options =>
                       {
                           options.Endpoint = new Uri("http://your-graylog-host:port");
                           options.Protocol = GrayLogExportProtocol.Tcp;
                       })
    .Build();
```