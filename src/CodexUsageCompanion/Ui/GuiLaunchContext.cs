using CodexUsageCompanion.Lifecycle;

namespace CodexUsageCompanion.Ui;

internal static class GuiLaunchContext
{
    internal static ResidentLease? ResidentLease { get; set; }

    internal static bool LaunchedInBackground { get; set; }
}
