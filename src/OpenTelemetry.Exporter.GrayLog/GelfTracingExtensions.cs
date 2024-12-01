using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Exporter.GrayLog.Abstractions;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace OpenTelemetry.Exporter.GrayLog;

public static class GelfTracingExtensions
{
    public static Dictionary<string, object> ToGelfFlattened(this Activity activity, string host, Resource resource)
    {
        ArgumentNullException.ThrowIfNull(activity);

        // Flatten the fields into a single dictionary
        var gelfPayload = new Dictionary<string, object>
                          {
                              { "version", "1.1" },
                              { "host", host },
                              { "short_message", $"Activity {activity.DisplayName} completed" },
                              { "timestamp", activity.StartTimeUtc },
                              { "level", 6 },
                              { "_traceId", activity.TraceId.ToString() },
                              { "_spanId", activity.SpanId.ToString() },
                              { "_displayName", activity.DisplayName },
                              { "_hasRemoteParent", activity.HasRemoteParent.ToString() },
                              { "_durationMs", activity.Duration.TotalMilliseconds },
                              { "_status", activity.Status.ToString() },
                              { "_sourceName", activity.Source.Name }
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
        foreach (var attribute in resource.Attributes)
        {
            gelfPayload[$"_resource_{attribute.Key}"] = attribute.Value;
        }

        // Flatten events
        var eventIndex = 0;
        foreach (var activityEvent in activity.Events)
        {
            var eventPrefix = $"_event_{eventIndex++}_";
            gelfPayload[$"{eventPrefix}name"] = activityEvent.Name;
            gelfPayload[$"{eventPrefix}timestamp"] = activityEvent.Timestamp.ToUnixTimeSeconds();

            // Flatten event attributes
            foreach (var tag in activityEvent.Tags)
            {
                if (tag.Value != null) gelfPayload[$"{eventPrefix}attribute_{tag.Key}"] = tag.Value;
            }
        }

        return gelfPayload;
    }

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