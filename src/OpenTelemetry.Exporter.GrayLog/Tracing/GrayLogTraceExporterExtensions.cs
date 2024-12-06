using OpenTelemetry.Trace;

namespace OpenTelemetry.Exporter.GrayLog.Tracing;

public static class GrayLogTraceExporterExtensions
{
    public static TracerProviderBuilder AddGrayLogExporter(this TracerProviderBuilder builder)
        => builder.AddGrayLogExporter(null);

    public static TracerProviderBuilder AddGrayLogExporter(this TracerProviderBuilder builder, Action<GrayLogExporterOptions>? configure)
    {
        var options = new GrayLogExporterOptions();
        configure?.Invoke(options);

        builder.AddProcessor(new GrayLogBatchActivityExportProcessor(new GraylogTraceExporter(GrayLogPublisherFactory.Create(options))));
        return builder;
    }
}