using System.Net.Sockets;
using System.Text;
using CodexUsageCompanion.Diagnostics;
using CodexUsageCompanion.Platform;

namespace CodexUsageCompanion.Lifecycle;

public sealed class InstanceCoordinator
{
    public const string DefaultName = "claude-codex-usage-companion";

    private readonly string _socketPath;
    private readonly string _lockPath;

    public InstanceCoordinator(string? endpoint = null)
    {
        _socketPath = endpoint ?? LinuxPaths.InstanceSocketPath;
        _lockPath = $"{_socketPath}.lock";
    }

    public ResidentLease? TryAcquireResident()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_socketPath)!);
        var ownershipLock = TryAcquireOwnershipLock();
        if (ownershipLock is null)
        {
            return null;
        }

        Socket? socket = null;
        try
        {
            // The lock, rather than the socket pathname, is the authoritative
            // ownership token. A crashed process can leave a socket pathname
            // behind, while an unlinked live socket can no longer prevent a
            // second process from binding the same name.
            if (File.Exists(_socketPath))
            {
                // Preserve compatibility with an older resident that predates
                // the lock file but still has a functioning listener.
                if (Signal("ping"))
                {
                    ownershipLock.Dispose();
                    return null;
                }

                File.Delete(_socketPath);
            }

            socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
            socket.Bind(new UnixDomainSocketEndPoint(_socketPath));
            socket.Listen(8);
            TryRestrictSocketPermissions();
            return new ResidentLease(socket, ownershipLock);
        }
        catch
        {
            socket?.Dispose();
            ownershipLock.Dispose();
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

    private FileStream? TryAcquireOwnershipLock()
    {
        FileStream? stream = null;
        try
        {
            stream = new FileStream(
                _lockPath,
                FileMode.OpenOrCreate,
                FileAccess.ReadWrite,
                FileShare.ReadWrite | FileShare.Delete);
            if (OperatingSystem.IsMacOS())
            {
                stream.Dispose();
                return null;
            }

            stream.Lock(0, 1);
            return stream;
        }
        catch (Exception exception) when (
            exception is IOException or
            UnauthorizedAccessException or
            PlatformNotSupportedException)
        {
            stream?.Dispose();
            return null;
        }
    }
}

public sealed class ResidentLease : IDisposable
{
    private readonly Socket _listener;
    private readonly FileStream _ownershipLock;
    private readonly CancellationTokenSource _cancellation = new();
    private Task? _acceptTask;
    private bool _disposed;

    internal ResidentLease(Socket listener, FileStream ownershipLock)
    {
        _listener = listener;
        _ownershipLock = ownershipLock;
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
        _ownershipLock.Dispose();
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

            _ = HandleConnectionAsync(connection, cancellationToken);
        }
    }

    private async Task HandleConnectionAsync(
        Socket connection,
        CancellationToken cancellationToken)
    {
        using (connection)
        using (var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
        {
            timeout.CancelAfter(TimeSpan.FromSeconds(1));
            try
            {
                var buffer = new byte[64];
                var read = await connection.ReceiveAsync(buffer, SocketFlags.None, timeout.Token);
                if (read == 0)
                {
                    return;
                }

                var message = Encoding.UTF8.GetString(buffer, 0, read).Trim();
                if (message is "refresh" or "activate")
                {
                    MessageReceived?.Invoke(message);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (SocketException)
            {
            }
        }
    }
}
