using System.Diagnostics;
using OpenTelemetry.Exporter.GrayLog.Abstractions;
using OpenTelemetry.Resources;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace OpenTelemetry.Exporter.GrayLog.Tracing;

public class GraylogTraceExporter(IGrayLogPublisher[] publishers, string host) : BaseExporter<Activity>
{
    private Resource? _processResource;
    private Resource ProcessResource => _processResource ??= ParentProvider.GetResource();

    private int _publisherIndex = Random.Shared.Next(0, publishers.Length);

    public override ExportResult Export(in Batch<Activity> batch)
    {
        if (publishers.Length == 0) return ExportResult.Success;

        foreach (var activity in batch)
        {
            if (_publisherIndex >= publishers.Length)
            {
                _publisherIndex = 0;
            }

            var gelfJsons = JsonSerializer.Serialize(activity.ToGelfFlattened(host, ProcessResource));
            if (!publishers[_publisherIndex].Publish(gelfJsons))
            {
                return ExportResult.Failure;
            }

            _publisherIndex++;
        }

        return ExportResult.Success;
    }
}