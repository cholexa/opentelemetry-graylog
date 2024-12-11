using System.Net.Sockets;
using System.Text;
using OpenTelemetry.Exporter.GrayLog.Abstractions;

namespace OpenTelemetry.Exporter.GrayLog.Publishers;

public class TcpGrayLogPublisher : IGrayLogPublisher
{
    private const int RetryCount = 5;

    private readonly string _host;
    private readonly int _port;
    private TcpClient _tcpClient;
    private readonly object _syncLock = new();

    public TcpGrayLogPublisher(string host, int port)
    {
        _host = host;
        _port = port;
        _tcpClient = new TcpClient();
        _tcpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
    }

    public bool Connected
    {
        get
        {
            try
            {
                lock (_syncLock)
                {
                    return _tcpClient.Connected && !_tcpClient.Client.Poll(1, SelectMode.SelectRead) &&
                           _tcpClient.Client.Available == 0;
                }
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

            var messageBytes = Encoding.UTF8.GetBytes($"{message}\0");
            if (messageBytes.Length >= 1048576)
            {
                Console.Error.WriteLine("Warning while publishing: Message size exceeds the allowed limit!");
            }

            lock (_syncLock)
            {
                _tcpClient.GetStream().Write(messageBytes, 0, messageBytes.Length);
                _tcpClient.GetStream().Flush();
            }
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
        lock (_syncLock)
        {
            DisposeClient();

            for (var attempt = 1; attempt <= RetryCount; attempt++)
            {
                try
                {
                    _tcpClient = new TcpClient();
                    _tcpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                    _tcpClient.Connect(_host, _port);
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
    }

    private void DisposeClient()
    {
        try
        {
            if (_tcpClient.Connected)
            {
                _tcpClient.Close();
            }
        }
        catch
        {
            // Ignored
        }
        finally
        {
            _tcpClient.Dispose();
        }
    }

    public void Dispose()
    {
        DisposeClient();
    }
}