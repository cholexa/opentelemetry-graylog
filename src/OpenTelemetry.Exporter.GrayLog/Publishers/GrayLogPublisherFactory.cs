using OpenTelemetry.Exporter.GrayLog.Abstractions;

namespace OpenTelemetry.Exporter.GrayLog.Publishers;

internal static class GrayLogPublisherFactory
{
    internal static IGrayLogPublisher[] Create(GrayLogExporterOptions options)
    {
        return options.Protocol switch
               {
                   GrayLogExportProtocol.Tcp => options.Endpoints.Select(endpoint => new TcpGrayLogPublisher(endpoint.Host, endpoint.Port)).ToArray<IGrayLogPublisher>(),
                   GrayLogExportProtocol.Udp => options.Endpoints.Select(endpoint => new UdpGrayLogPublisher(endpoint.Host, endpoint.Port)).ToArray<IGrayLogPublisher>(),
                   _ => throw new ArgumentOutOfRangeException(nameof(options.Protocol), options.Protocol, null)
               };
    }
}