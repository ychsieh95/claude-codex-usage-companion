using System.Net.Sockets;
using System.Text;
using CodexUsageCompanion.Diagnostics;
using CodexUsageCompanion.Platform;

namespace CodexUsageCompanion.Lifecycle;

public sealed class InstanceCoordinator
{
    public const string DefaultName = "claude-codex-usage-companion";

    private readonly string _socketPath;

    public InstanceCoordinator(string? endpoint = null)
    {
        _socketPath = endpoint ?? LinuxPaths.InstanceSocketPath;
    }

    public ResidentLease? TryAcquireResident()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_socketPath)!);
        var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
        try
        {
            socket.Bind(new UnixDomainSocketEndPoint(_socketPath));
            socket.Listen(8);
            TryRestrictSocketPermissions();
            return new ResidentLease(socket, _socketPath);
        }
        catch (SocketException) when (File.Exists(_socketPath))
        {
            socket.Dispose();
            if (Signal("ping"))
            {
                return null;
            }

            try
            {
                File.Delete(_socketPath);
            }
            catch (IOException)
            {
                return null;
            }
            catch (UnauthorizedAccessException)
            {
                return null;
            }

            socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
            try
            {
                socket.Bind(new UnixDomainSocketEndPoint(_socketPath));
                socket.Listen(8);
                TryRestrictSocketPermissions();
                return new ResidentLease(socket, _socketPath);
            }
            catch
            {
                socket.Dispose();
                return null;
            }
        }
        catch
        {
            socket.Dispose();
            return null;
        }
    }

    public bool SignalRefresh() => Signal("refresh");

    public bool Signal(string message)
    {
        if (string.IsNullOrWhiteSpace(message) || message.Length > 48)
        {
            return false;
        }

        try
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromMilliseconds(500));
            using var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
            socket.ConnectAsync(new UnixDomainSocketEndPoint(_socketPath), timeout.Token)
                .AsTask().GetAwaiter().GetResult();
            var payload = Encoding.UTF8.GetBytes(message + "\n");
            socket.SendAsync(payload, SocketFlags.None, timeout.Token)
                .AsTask().GetAwaiter().GetResult();
            return true;
        }
        catch (Exception exception) when (
            exception is SocketException or OperationCanceledException or IOException)
        {
            return false;
        }
    }

    private void TryRestrictSocketPermissions()
    {
        if (!OperatingSystem.IsLinux() && !OperatingSystem.IsMacOS())
        {
            return;
        }

        try
        {
            File.SetUnixFileMode(
                _socketPath,
                UnixFileMode.UserRead | UnixFileMode.UserWrite);
        }
        catch (PlatformNotSupportedException)
        {
        }
        catch (IOException exception)
        {
            CompanionLog.Shared.Write("instance-permissions", exception);
        }
        catch (UnauthorizedAccessException exception)
        {
            CompanionLog.Shared.Write("instance-permissions", exception);
        }
    }
}

public sealed class ResidentLease : IDisposable
{
    private readonly Socket _listener;
    private readonly string _socketPath;
    private readonly CancellationTokenSource _cancellation = new();
    private Task? _acceptTask;
    private bool _disposed;

    internal ResidentLease(Socket listener, string socketPath)
    {
        _listener = listener;
        _socketPath = socketPath;
    }

    public event Action<string>? MessageReceived;

    public void Start()
    {
        _acceptTask ??= AcceptLoopAsync(_cancellation.Token);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _cancellation.Cancel();
        _listener.Dispose();
        if (_acceptTask is not null)
        {
            try
            {
                _acceptTask.GetAwaiter().GetResult();
            }
            catch (OperationCanceledException)
            {
            }
            catch (SocketException)
            {
            }
        }

        _cancellation.Dispose();
        try
        {
            File.Delete(_socketPath);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    private async Task AcceptLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            Socket connection;
            try
            {
                connection = await _listener.AcceptAsync(cancellationToken);
            }
            catch (ObjectDisposedException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (SocketException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            using (connection)
            {
                var buffer = new byte[64];
                var read = await connection.ReceiveAsync(buffer, SocketFlags.None, cancellationToken);
                if (read == 0)
                {
                    continue;
                }

                var message = Encoding.UTF8.GetString(buffer, 0, read).Trim();
                if (message is "refresh" or "activate")
                {
                    MessageReceived?.Invoke(message);
                }
            }
        }
    }
}
