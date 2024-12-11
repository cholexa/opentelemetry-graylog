using OpenTelemetry.Exporter.GrayLog.Publishers;
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
        options.BatchExportProcessorOptions ??= new BatchExportActivityProcessorOptions();

        builder.AddProcessor(new GrayLogBatchActivityExportProcessor(new GraylogTraceExporter(GrayLogPublisherFactory.Create(options), options.Host),
                                                                     options.BatchExportProcessorOptions.MaxQueueSize,
                                                                     options.BatchExportProcessorOptions.ScheduledDelayMilliseconds,
                                                                     options.BatchExportProcessorOptions.ExporterTimeoutMilliseconds,
                                                                     options.BatchExportProcessorOptions.MaxExportBatchSize));
        return builder;
    }
}