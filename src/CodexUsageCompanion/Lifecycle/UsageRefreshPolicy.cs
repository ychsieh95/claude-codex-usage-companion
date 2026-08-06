namespace CodexUsageCompanion.Lifecycle;

public static class UsageRefreshPolicy
{
    public static bool ShouldShowUnavailable(bool hasUsageState) => !hasUsageState;

    public static bool ShouldRefreshForInstanceMessage(string message) =>
        message == "refresh";
}
