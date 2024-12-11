using System.Text.Json;
using OpenTelemetry.Exporter.GrayLog.Abstractions;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;

namespace OpenTelemetry.Exporter.GrayLog.Logging;

public class GrayLogLogExporter(IGrayLogPublisher[] publishers, string host) : BaseExporter<LogRecord>
{
    private Resource? _processResource;
    private Resource ProcessResource => _processResource ??= ParentProvider.GetResource();

    private int _publisherIndex = Random.Shared.Next(0, publishers.Length);

    public override ExportResult Export(in Batch<LogRecord> batch)
    {
        if (publishers.Length == 0) return ExportResult.Success;

        foreach (var logRecord in batch)
        {
            try
            {
                if (_publisherIndex >= publishers.Length)
                {
                    _publisherIndex = 0;
                }

                var gelfJsons = JsonSerializer.Serialize(logRecord.ToGelfFlattened(host, ProcessResource));
                publishers[_publisherIndex].Publish(gelfJsons);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GrayLogLogExporter: {ex}");
            }
            finally
            {
                _publisherIndex++;
            }
        }

        return ExportResult.Success;
    }
}