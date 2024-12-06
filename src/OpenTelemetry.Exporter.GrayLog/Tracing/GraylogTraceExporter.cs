using System.Diagnostics;
using OpenTelemetry.Exporter.GrayLog.Abstractions;
using OpenTelemetry.Resources;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace OpenTelemetry.Exporter.GrayLog.Tracing;

public class GraylogTraceExporter(IGrayLogPublisher grayLogPublisher) : BaseExporter<Activity>
{
    private Resource? _processResource;
    private Resource ProcessResource => _processResource ??= ParentProvider.GetResource();

    public override ExportResult Export(in Batch<Activity> batch)
    {
        if (!grayLogPublisher.Connected)
        {
            return ExportResult.Failure;
        }

        foreach (var activity in batch)
        {
            var gelfJsons = JsonSerializer.Serialize(activity.ToGelfFlattened(Environment.MachineName, ProcessResource));
            if (!grayLogPublisher.Publish(gelfJsons))
            {
                return ExportResult.Failure;
            }
        }

        return ExportResult.Success;
    }
}