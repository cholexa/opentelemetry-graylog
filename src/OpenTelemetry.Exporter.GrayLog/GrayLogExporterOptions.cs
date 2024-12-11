using System.Diagnostics;
using OpenTelemetry.Exporter.GrayLog.Publishers;
using OpenTelemetry.Trace;

namespace OpenTelemetry.Exporter.GrayLog;

public record GrayLogExporterOptions
{
    private readonly Dictionary<GrayLogExportProtocol, Uri> _defaultEndpoints = new()
                                                                                {
                                                                                    { GrayLogExportProtocol.Udp, new Uri("http://localhost:12201") },
                                                                                    { GrayLogExportProtocol.Tcp, new Uri("http://localhost:12201") }
                                                                                };

    private Uri[]? _endpoints;

    public Uri[] Endpoints
    {
        get => _endpoints ?? [_defaultEndpoints[Protocol]];
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            _endpoints = value;
        }
    }

    public GrayLogExportProtocol Protocol { get; set; } = GrayLogExportProtocol.Udp;

    public string Host { get; set; } = Environment.MachineName;

    public BatchExportProcessorOptions<Activity> BatchExportProcessorOptions { get; set; } = new BatchExportActivityProcessorOptions();
}