using OpenTelemetry.Exporter.GrayLog.Publishers;

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

    private GrayLogExportProtocol? _protocol;

    public GrayLogExportProtocol Protocol
    {
        get => _protocol ?? GrayLogExportProtocol.Udp;
        set => _protocol = value;
    }
}