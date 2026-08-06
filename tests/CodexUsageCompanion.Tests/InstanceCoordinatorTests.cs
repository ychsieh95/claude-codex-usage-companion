using CodexUsageCompanion.Lifecycle;
using Xunit;

namespace CodexUsageCompanion.Tests;

public sealed class InstanceCoordinatorTests
{
    [Fact]
    public void TryAcquireResidentAllowsOnlyOneOwner()
    {
        var path = SocketPath();
        var first = new InstanceCoordinator(path);
        var second = new InstanceCoordinator(path);

        using var firstLease = first.TryAcquireResident();
        firstLease?.Start();
        using var secondLease = second.TryAcquireResident();

        Assert.NotNull(firstLease);
        Assert.Null(secondLease);
    }

    [Fact]
    public async Task SignalRefreshWakesResidentOwner()
    {
        var path = SocketPath();
        var owner = new InstanceCoordinator(path);
        var sender = new InstanceCoordinator(path);
        using var lease = owner.TryAcquireResident();
        var received = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);

        Assert.NotNull(lease);
        lease.MessageReceived += message => received.TrySetResult(message);
        lease.Start();
        Assert.True(sender.SignalRefresh());
        Assert.Equal("refresh", await received.Task.WaitAsync(TimeSpan.FromSeconds(2)));
    }

    [Fact]
    public void SignalRefreshReturnsFalseWithoutResidentOwner()
    {
        var sender = new InstanceCoordinator(SocketPath());

        Assert.False(sender.SignalRefresh());
    }

    [Fact]
    public void DisposingResidentLeaseStopsListenerAndReleasesEndpoint()
    {
        var path = SocketPath();
        var coordinator = new InstanceCoordinator(path);
        var lease = coordinator.TryAcquireResident();

        Assert.NotNull(lease);
        lease.Start();
        lease.Dispose();

        using var replacement = coordinator.TryAcquireResident();
        Assert.NotNull(replacement);
    }

    private static string SocketPath() =>
        Path.Combine(Path.GetTempPath(), $"claude-codex-usage-test-{Guid.NewGuid():N}.sock");
}
