using System.Diagnostics;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;

namespace OpenTelemetry.Exporter.GrayLog;

public static class GelfFormatExtensions
{
    private const string EmptyTraceId = "00000000000000000000000000000000";
    private const string EmptySpanId = "0000000000000000";

    public static Dictionary<string, object> ToGelfFlattened(this Activity activity, string host, Resource resource)
    {
        ArgumentNullException.ThrowIfNull(activity);

        // Flatten the fields into a single dictionary
        var gelfPayload = new Dictionary<string, object>
                          {
                              { "version", "1.1" },
                              { "host", host },
                              { "timestamp", activity.StartTimeUtc.ToUnixTimeSecondsWithOptionalDecimalMilliseconds() },
                              { "short_message", $"Activity: {activity.DisplayName}" },
                              { "level", LogLevel.Information },
                              { "_traceId", activity.TraceId.ToString() },
                              { "_spanId", activity.SpanId.ToString() },
                              { "_activity", activity.DisplayName },
                              { "_activity_source", activity.Source.Name },
                              { "_hasRemoteParent", activity.HasRemoteParent.ToString() },
                              { "_durationMs", activity.Duration.TotalMilliseconds },
                              { "_status", activity.Status.ToString() }
                          };

        if (activity.RootId != null) gelfPayload.Add("_rootId", activity.RootId);
        if (activity.ParentId != null) gelfPayload.Add("_parentId", activity.ParentId);

        // Flatten tag objects
        foreach (var tag in activity.TagObjects)
        {
            if (tag.Value != null) gelfPayload[$"_tag_{tag.Key}"] = tag.Value;
        }

        // Flatten links
        var linkIndex = 0;
        foreach (var link in activity.Links)
        {
            var linkPrefix = $"_link_{linkIndex++}_";
            gelfPayload[$"{linkPrefix}traceId"] = link.Context.TraceId.ToString();
            gelfPayload[$"{linkPrefix}spanId"] = link.Context.SpanId.ToString();

            // Flatten link attributes
            if (link.Tags == null) continue;
            foreach (var tag in link.Tags)
            {
                if (tag.Value != null) gelfPayload[$"{linkPrefix}attribute_{tag.Key}"] = tag.Value;
            }
        }

        // Flatten baggage
        foreach (var baggage in activity.Baggage)
        {
            if (baggage.Value != null) gelfPayload[$"_baggage_{baggage.Key}"] = baggage.Value;
        }

        // Flatten resource attributes
        foreach (var attribute in resource.Attributes.Where(x => !x.Key.Contains("telemetry")))
        {
            gelfPayload[$"_resource_{attribute.Key}"] = attribute.Value;
        }

        // Flatten events
        var eventIndex = 0;
        foreach (var activityEvent in activity.Events)
        {
            var eventPrefix = $"_event_{eventIndex++}_";
            gelfPayload[$"{eventPrefix}name"] = activityEvent.Name;
            gelfPayload[$"{eventPrefix}timestamp"] = activityEvent.Timestamp.ToUnixTimeSecondsWithOptionalDecimalMilliseconds();

            // Flatten event attributes
            foreach (var tag in activityEvent.Tags)
            {
                if (tag.Value != null) gelfPayload[$"{eventPrefix}attribute_{tag.Key}"] = tag.Value;
            }
        }

        return gelfPayload;
    }

    public static Dictionary<string, object> ToGelfFlattened(this LogRecord logRecord, string host, Resource resource)
    {
        ArgumentNullException.ThrowIfNull(logRecord);

        // Flatten the fields into a single dictionary
        var gelfPayload = new Dictionary<string, object>
                          {
                              { "version", "1.1" },
                              { "host", host },
                              { "timestamp", logRecord.Timestamp.ToUnixTimeSecondsWithOptionalDecimalMilliseconds() },
                              { "short_message", logRecord.CategoryName ?? "N/A" },
                              { "level", logRecord.LogLevel }
                          };

        var formattedMessage = logRecord.FormattedMessage;
        if (string.IsNullOrEmpty(formattedMessage))
        {
            formattedMessage = logRecord.Body;

            if (logRecord.Attributes != null && logRecord.Attributes.Any())
            {
                var attributes = string.Join(", ", logRecord.Attributes.Where(x => x.Value?.ToString() != formattedMessage).Select(kvp => $"{kvp.Key}: {kvp.Value}"));
                formattedMessage = $"{formattedMessage ?? "N/A"} | Attributes: {attributes}";
            }
        }

        gelfPayload["full_message"] = formattedMessage ?? "N/A";

        var traceId = logRecord.TraceId.ToString();
        var spanId = logRecord.SpanId.ToString();

        if (!string.IsNullOrEmpty(traceId) && traceId != EmptyTraceId)
            gelfPayload["_traceId"] = traceId;
        if (!string.IsNullOrEmpty(spanId) && spanId != EmptySpanId)
            gelfPayload["_spanId"] = spanId;

        // Flatten resource attributes
        foreach (var attribute in resource.Attributes.Where(x => !x.Key.Contains("telemetry")))
        {
            gelfPayload[$"_resource_{attribute.Key}"] = attribute.Value;
        }

        return gelfPayload;
    }

    private static double ToUnixTimeSecondsWithOptionalDecimalMilliseconds(this DateTime dateTime, bool includeMilliseconds = true)
    {
        return ToUnixTimeSecondsWithOptionalDecimalMilliseconds(new DateTimeOffset(dateTime), includeMilliseconds);
    }

    private static double ToUnixTimeSecondsWithOptionalDecimalMilliseconds(this DateTimeOffset dateTime, bool includeMilliseconds = true)
    {
        double activityStartUnixSeconds = dateTime.ToUnixTimeSeconds();
        if (includeMilliseconds)
        {
            activityStartUnixSeconds += dateTime.Millisecond / 1000.0f;
        }

        return activityStartUnixSeconds;
    }
}