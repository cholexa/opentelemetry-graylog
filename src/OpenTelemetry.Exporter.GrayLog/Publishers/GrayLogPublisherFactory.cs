using OpenTelemetry.Exporter.GrayLog.Abstractions;

namespace OpenTelemetry.Exporter.GrayLog.Publishers;

internal static class GrayLogPublisherFactory
{
    internal static IGrayLogPublisher Create(GrayLogExporterOptions options)
    {
        return options.Protocol switch
               {
                   GrayLogExportProtocol.Tcp => new TcpGrayLogPublisher(options.Endpoint.Host, options.Endpoint.Port),
                   GrayLogExportProtocol.Udp => new UdpGrayLogPublisher(options.Endpoint.Host, options.Endpoint.Port),
                   _ => throw new ArgumentOutOfRangeException(nameof(options.Protocol), options.Protocol, null)
               };
    }
}