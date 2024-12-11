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

    private Uri? _endpoint;

    public Uri Endpoint
    {
        get => _endpoint == null ? _defaultEndpoints[Protocol] : _endpoint;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            _endpoint = value;
        }
    }

    public GrayLogExportProtocol Protocol { get; set; } = GrayLogExportProtocol.Udp;

    public string Host { get; set; } = Environment.MachineName;

    public BatchExportProcessorOptions<Activity> BatchExportProcessorOptions { get; set; } = new BatchExportActivityProcessorOptions();
}