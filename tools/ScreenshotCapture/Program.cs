using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using Avalonia.Threading;
using Avalonia.VisualTree;
using CodexUsageCompanion.Configuration;
using CodexUsageCompanion.Localization;
using CodexUsageCompanion.Lifecycle;
using CodexUsageCompanion.RateLimits;
using CodexUsageCompanion.Ui;

if (args.Length != 1)
    throw new ArgumentException("Pass the screenshot output directory.");
Directory.CreateDirectory(args[0]);

AppBuilder.Configure<Application>()
    .UseSkia()
    .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false })
    .SetupWithoutStarting();
Application.Current!.Styles.Add(new FluentTheme());
Application.Current.RequestedThemeVariant = ThemeVariant.Dark;

// Fixed sample data: no account credentials or live provider requests.
var updated = new DateTimeOffset(2026, 10, 12, 15, 29, 0, TimeSpan.FromHours(8));
var weeklyReset = new DateTimeOffset(2026, 10, 19, 1, 1, 0, TimeSpan.FromHours(8)).ToUnixTimeSeconds();
foreach (var (language, filename) in new[]
{
    (UiLanguage.English, "overlay-en.png"),
    (UiLanguage.TraditionalChinese, "overlay-zh-tw.png"),
    (UiLanguage.SimplifiedChinese, "overlay-zh-cn.png")
})
{
    var window = new UsageOverlayWindow(new CompanionSettings
    {
        Theme = UiThemeOptions.Dark,
        ShowCodexFiveHour = false
    }, UiText.For(language));
    window.Width = 460;
    window.UpdateUsage(UsageProvider.Claude, null);
    window.UpdateUsage(UsageProvider.Codex, new RateLimitState(
        null, new(87, 10080, weeklyReset), null)
    {
        CreditBalance = "0.00",
        AutomaticReloadEnabled = false
    });
    var error = language switch
    {
        UiLanguage.TraditionalChinese => "Claude 工作階段已過期。請執行 'claude' 重新登入。",
        UiLanguage.SimplifiedChinese => "Claude 会话已过期。请运行 'claude' 重新登录。",
        _ => "Claude session expired. Run 'claude' to refresh your session."
    };
    window.SetStatus(null, error, UsageProvider.Claude);
    window.SetStatus(updated, null, UsageProvider.Codex);
    window.Show();
    Dispatcher.UIThread.RunJobs();
    AvaloniaHeadlessPlatform.ForceRenderTimerTick();
    using var frame = window.CaptureRenderedFrame()
        ?? throw new InvalidOperationException("Screenshot rendering failed.");
    using var output = File.Create(Path.Combine(args[0], filename));
    frame.Save(output, PngBitmapEncoderOptions.Default);
    Console.WriteLine($"Captured {filename} ({frame.PixelSize})");
    window.Close();

    var text = UiText.For(language);
    var settings = new SettingsWindow(new CompanionSettings
    {
        Theme = UiThemeOptions.Dark,
        SettingsSections = new SettingsSectionState
        {
            Usage = false,
            Appearance = false,
            Window = false,
            Notifications = false,
            DateTime = false,
            Logging = false,
            Updates = false
        }
    }, text);
    settings.Show();
    Capture(settings, filename.Replace("overlay-", "settings-"));
    var resetButton = settings.GetVisualDescendants().OfType<Button>()
        .Single(button => Equals(button.Content, text.ResetDefaultsAction));
    resetButton.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
    Dispatcher.UIThread.RunJobs();
    var confirmation = settings.OwnedWindows.Single();
    Capture(confirmation, filename.Replace("overlay-", "reset-confirmation-"));
    confirmation.Close(false);
    settings.Close();
}

void Capture(Window target, string filename)
{
    // Modal controls may need another layout pass after the window opens.
    for (var pass = 0; pass < 3; pass++)
    {
        Dispatcher.UIThread.RunJobs();
        AvaloniaHeadlessPlatform.ForceRenderTimerTick();
    }
    using var frame = target.CaptureRenderedFrame()
        ?? throw new InvalidOperationException("Screenshot rendering failed.");
    using var output = File.Create(Path.Combine(args[0], filename));
    frame.Save(output, PngBitmapEncoderOptions.Default);
    Console.WriteLine($"Captured {filename} ({frame.PixelSize})");
}
