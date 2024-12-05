using OpenTelemetry.Logs;

namespace OpenTelemetry.Exporter.GrayLog;

public static class GrayLogLogExporterExtensions
{
    public static OpenTelemetryLoggerOptions AddGrayLogExporter(this OpenTelemetryLoggerOptions otLoggerOptions)
        => otLoggerOptions.AddGrayLogExporter(null);

    public static OpenTelemetryLoggerOptions AddGrayLogExporter(this OpenTelemetryLoggerOptions otLoggerOptions, Action<GrayLogExporterOptions>? configure)
    {
        var options = new GrayLogExporterOptions();
        configure?.Invoke(options);

        otLoggerOptions.AddProcessor(sp => new BatchLogRecordExportProcessor(new GrayLogLogExporter(GrayLogPublisherFactory.Create(options))));
        return otLoggerOptions;
    }
}