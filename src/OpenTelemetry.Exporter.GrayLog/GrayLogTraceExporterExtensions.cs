using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Exporter.GrayLog.Abstractions;
using OpenTelemetry.Trace;

namespace OpenTelemetry.Exporter.GrayLog;

public static class GrayLogTraceExporterExtensions
{
    public static TracerProviderBuilder AddGrayLogExporter(this TracerProviderBuilder builder)
        => builder.AddGrayLogExporter(null);

    public static TracerProviderBuilder AddGrayLogExporter(this TracerProviderBuilder builder, Action<GrayLogExporterOptions>? configure)
    {
        var options = new GrayLogExporterOptions();
        configure?.Invoke(options);

        builder.ConfigureServices(sc => sc.AddSingleton(GrayLogPublisherFactory.Create(options)));
        builder.AddProcessor(sp => new GrayLogBatchActivityExportProcessor(new GelfFormatExporter(sp.GetRequiredService<IGrayLogPublisher>())));

        return builder;
    }
}