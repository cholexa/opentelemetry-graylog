namespace OpenTelemetry.Exporter.GrayLog.Abstractions;

public interface IGrayLogPublisher : IDisposable
{
    bool Connected { get; }
    public bool Publish(string message);
}