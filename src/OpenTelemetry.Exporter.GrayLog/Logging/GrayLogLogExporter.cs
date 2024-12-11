using System.Text.Json;
using OpenTelemetry.Exporter.GrayLog.Abstractions;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;

namespace OpenTelemetry.Exporter.GrayLog.Logging;

public class GrayLogLogExporter(IGrayLogPublisher grayLogPublisher, string host) : BaseExporter<LogRecord>
{
    private Resource? _processResource;
    private Resource ProcessResource => _processResource ??= ParentProvider.GetResource();

    public override ExportResult Export(in Batch<LogRecord> batch)
    {
        foreach (var logRecord in batch)
        {
            try
            {
                var gelfJsons = JsonSerializer.Serialize(logRecord.ToGelfFlattened(host, ProcessResource));
                grayLogPublisher.Publish(gelfJsons);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GrayLogLogExporter: {ex}");
            }
        }

        return ExportResult.Success;
    }
}