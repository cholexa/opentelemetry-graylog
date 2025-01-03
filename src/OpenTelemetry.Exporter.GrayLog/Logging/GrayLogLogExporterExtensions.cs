using OpenTelemetry.Exporter.GrayLog.Publishers;
using OpenTelemetry.Logs;
using OpenTelemetry.Trace;

namespace OpenTelemetry.Exporter.GrayLog.Logging;

public static class GrayLogLogExporterExtensions
{
    public static OpenTelemetryLoggerOptions AddGrayLogExporter(this OpenTelemetryLoggerOptions otLoggerOptions)
        => otLoggerOptions.AddGrayLogExporter(null);

    public static OpenTelemetryLoggerOptions AddGrayLogExporter(this OpenTelemetryLoggerOptions otLoggerOptions, Action<GrayLogExporterOptions>? configure)
    {
        var options = new GrayLogExporterOptions();
        configure?.Invoke(options);

        otLoggerOptions.AddProcessor(new BatchLogRecordExportProcessor(new GrayLogLogExporter(GrayLogPublisherFactory.Create(options), options.Host),
                                                                       options.BatchExportProcessorOptions.MaxQueueSize,
                                                                       options.BatchExportProcessorOptions.ScheduledDelayMilliseconds,
                                                                       options.BatchExportProcessorOptions.ExporterTimeoutMilliseconds,
                                                                       options.BatchExportProcessorOptions.MaxExportBatchSize));
        return otLoggerOptions;
    }
}