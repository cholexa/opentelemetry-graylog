using System.Diagnostics;

namespace OpenTelemetry.Exporter.GrayLog.Tracing;

public class GrayLogBatchActivityExportProcessor(
    GraylogTraceExporter formatExporter,
    int maxQueueSize = 2048,
    int scheduledDelayMilliseconds = 5000,
    int exporterTimeoutMilliseconds = 30000,
    int maxExportBatchSize = 512)
    : BatchActivityExportProcessor(formatExporter, maxQueueSize, scheduledDelayMilliseconds, exporterTimeoutMilliseconds, maxExportBatchSize)
{
    protected override void OnExport(Activity data)
    {
        if (!data.Recorded)
        {
            return;
        }

        base.OnExport(data);
    }
}