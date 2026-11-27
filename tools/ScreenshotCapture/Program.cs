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

// Sample data relative to the capture time, so the latest design shows live countdowns.
// No account credentials or live provider requests.
var now = DateTimeOffset.Now;
var updated = new DateTimeOffset(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, now.Offset);
var claudeSessionReset = updated.AddHours(2).AddMinutes(31).ToUnixTimeSeconds();
var codexFiveHourReset = updated.AddHours(4).AddMinutes(1).ToUnixTimeSeconds();
var weeklyReset = updated.AddDays(4).AddHours(9).AddMinutes(32).ToUnixTimeSeconds();
foreach (var (language, filename) in new[]
{
    (UiLanguage.English, "overlay-en.png"),
    (UiLanguage.TraditionalChinese, "overlay-zh-tw.png"),
    (UiLanguage.SimplifiedChinese, "overlay-zh-cn.png")
})
foreach (var (design, overlayFilename) in new[]
{
    (OverlayDesignOptions.Latest, filename),
    (OverlayDesignOptions.Previous, filename.Replace("overlay-", "overlay-previous-")),
    (OverlayDesignOptions.Rings, filename.Replace("overlay-", "overlay-rings-"))
})
{
    var window = new UsageOverlayWindow(new CompanionSettings
    {
        Theme = UiThemeOptions.Dark,
        ShowCodexFiveHour = true,
        OverlayDesign = design
    }, UiText.For(language));
    // Rings keeps its own narrower fitted width.
    if (design != OverlayDesignOptions.Rings)
        window.Width = 460;
    window.UpdateUsage(UsageProvider.Claude, new RateLimitState(
        new(72, 300, claudeSessionReset), new(64, 10080, weeklyReset), null)
    {
        ExtraUsage = new RateLimitExtraUsageState(true, 3.20m, 20.00m, "USD")
    });
    window.UpdateUsage(UsageProvider.Codex, new RateLimitState(
        new(91, 300, codexFiveHourReset), new(87, 10080, weeklyReset), null)
    {
        CreditBalance = "0.00",
        AutomaticReloadEnabled = false
    });
    window.SetStatus(updated, null, UsageProvider.Claude);
    window.SetStatus(updated, null, UsageProvider.Codex);
    window.Show();
    Capture(window, overlayFilename, transparent: true);
    window.Close();
    if (design != OverlayDesignOptions.Latest)
        continue;

    var text = UiText.For(language);
    var settings = new SettingsWindow(new CompanionSettings
    {
        Theme = UiThemeOptions.Dark
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

void Capture(Window target, string filename, bool transparent = false)
{
    // Modal controls may need another layout pass after the window opens.
    for (var pass = 0; pass < 3; pass++)
    {
        Dispatcher.UIThread.RunJobs();
        AvaloniaHeadlessPlatform.ForceRenderTimerTick();
    }
    // The headless platform has no window transparency, so the window template paints a
    // black fallback behind the overlay's rounded corners and shadow. Hide it and render to
    // an alpha bitmap, since the headless frame buffer is opaque.
    if (transparent)
    {
        foreach (var fallback in target.GetVisualDescendants().OfType<Border>()
            .Where(border => border.Name == "PART_TransparencyFallback"))
            fallback.IsVisible = false;
    }
    using Bitmap frame = transparent
        ? RenderTransparent(target)
        : target.CaptureRenderedFrame()
            ?? throw new InvalidOperationException("Screenshot rendering failed.");
    using var output = File.Create(Path.Combine(args[0], filename));
    frame.Save(output, PngBitmapEncoderOptions.Default);
    Console.WriteLine($"Captured {filename} ({frame.PixelSize})");
}

static RenderTargetBitmap RenderTransparent(Window target)
{
    var size = new PixelSize((int)Math.Ceiling(target.Bounds.Width), (int)Math.Ceiling(target.Bounds.Height));
    var bitmap = new RenderTargetBitmap(size);
    bitmap.Render(target);
    return bitmap;
}
