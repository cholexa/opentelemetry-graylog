using OpenTelemetry.Exporter.GrayLog.Abstractions;

namespace OpenTelemetry.Exporter.GrayLog;

internal static class GrayLogPublisherFactory
{
    internal static IGrayLogPublisher Create(GrayLogExporterOptions options)
    {
        switch (options.Protocol)
        {
            case GrayLogExportProtocol.Tcp:
                return new TcpGrayLogPublisher(options.Endpoint.Host, options.Endpoint.Port);
            case GrayLogExportProtocol.Udp:
            default:
                throw new ArgumentOutOfRangeException(nameof(options.Protocol), options.Protocol, null);
        }
    }
}