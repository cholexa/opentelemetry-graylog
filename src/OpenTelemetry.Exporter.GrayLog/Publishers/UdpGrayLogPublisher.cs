using System.Net.Sockets;
using System.Text;
using OpenTelemetry.Exporter.GrayLog.Abstractions;

namespace OpenTelemetry.Exporter.GrayLog.Publishers;

public class UdpGrayLogPublisher : IGrayLogPublisher
{
    private const int RetryCount = 5;

    private readonly string _host;
    private readonly int _port;
    private UdpClient _udpClient;

    public UdpGrayLogPublisher(string host, int port)
    {
        _host = host;
        _port = port;
        _udpClient = new UdpClient();
        EnsureConnected();
    }

    public bool Connected
    {
        get
        {
            try
            {
                return _udpClient.Client.Connected;
            }
            catch
            {
                return false;
            }
        }
    }

    public bool Publish(string message)
    {
        try
        {
            var connected = EnsureConnected();
            if (!connected)
                return false;

            var messageBytes = Encoding.UTF8.GetBytes(message);
            if (messageBytes.Length >= 1048576)
            {
                Console.Error.WriteLine("Warning while publishing: Message size exceeds the allowed limit!");
            }

            _udpClient.Send(messageBytes, messageBytes.Length);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error while publishing: {ex.Message}");
            Reconnect();
        }

        return true;
    }

    private bool EnsureConnected()
    {
        var connected = Connected;
        return connected ? connected : Reconnect();
    }

    private bool Reconnect()
    {
        DisposeClient();

        for (var attempt = 1; attempt <= RetryCount; attempt++)
        {
            try
            {
                _udpClient = new UdpClient();
                _udpClient.Connect(_host, _port);
                Console.WriteLine("Reconnected successfully.");
                return true;
            }
            catch
            {
                Console.WriteLine($"Reconnection attempt {attempt} failed. Retrying...");
                Thread.Sleep(TimeSpan.FromSeconds(Math.Pow(2, attempt))); // Exponential backoff
            }
        }

        Console.WriteLine("Failed to reconnect after multiple attempts.");
        return false;
    }

    private void DisposeClient()
    {
        try
        {
            _udpClient.Close();
        }
        catch
        {
            // Ignored
        }
        finally
        {
            _udpClient?.Dispose();
        }
    }

    public void Dispose()
    {
        DisposeClient();
    }
}