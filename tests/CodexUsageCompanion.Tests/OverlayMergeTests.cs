using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.VisualTree;
using CodexUsageCompanion.Configuration;
using CodexUsageCompanion.Localization;
using CodexUsageCompanion.Lifecycle;
using CodexUsageCompanion.RateLimits;
using CodexUsageCompanion.Ui;
using Xunit;

[assembly: AvaloniaTestApplication(typeof(CodexUsageCompanion.Tests.OverlayTestApplication))]

namespace CodexUsageCompanion.Tests;

public static class OverlayTestApplication
{
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<Application>().UseHeadless(new AvaloniaHeadlessPlatformOptions());
}

public sealed class OverlayMergeTests
{
    private static readonly HeadlessUnitTestSession Session =
        HeadlessUnitTestSession.StartNew(typeof(OverlayTestApplication));

    [Fact]
    public Task HeaderWhitespaceStartsCustomPositionTracking() => Session.Dispatch(() =>
    {
        var window = new UsageOverlayWindow();
        try
        {
            window.Show();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            PixelPoint? savedPosition = null;
            window.CustomPositionChanged += position => savedPosition = position;
            var title = window.GetVisualDescendants().OfType<TextBlock>()
                .Single(text => text.Text == UiText.For(UiLanguage.English).CombinedUsageHeaderTitle);
            var header = title.GetVisualAncestors().OfType<Grid>().First();
            // The empty strip above the vertically centered title must also drag.
            var point = header.TranslatePoint(new Point(8, 1), window)!.Value;
            var originalPosition = window.Position;
            window.MouseDown(point, MouseButton.Left);
            window.Width += 30;
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            Assert.Equal(originalPosition, window.Position);
            // Headless input cannot drive the compositor; simulate its move event.
            var movedPosition = new PixelPoint(originalPosition.X - 110, originalPosition.Y - 70);
            window.Position = movedPosition;
            window.MouseUp(point, MouseButton.Left);
            window.Width += 30;
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            window.UpdateUsage(UsageProvider.Codex, null);
            window.HideToTray();

            Assert.Equal(movedPosition, savedPosition);
            window.RestoreAndActivate();
            Assert.Equal(movedPosition, window.Position);
        }
        finally
        {
            window.Close();
        }
    }, CancellationToken.None);

    [Fact]
    public Task HeaderSettingsButtonDoesNotStartWindowDrag() => Session.Dispatch(() =>
    {
        var theme = new Avalonia.Themes.Fluent.FluentTheme();
        Application.Current!.Styles.Add(theme);
        var window = new UsageOverlayWindow();
        try
        {
            window.Show();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            var settingsRequested = false;
            PixelPoint? savedPosition = null;
            window.SettingsRequested += (_, _) => settingsRequested = true;
            window.CustomPositionChanged += position => savedPosition = position;
            var button = window.GetVisualDescendants().OfType<Button>()
                .Single(control => control.Content as string == "⚙");
            var point = button.TranslatePoint(new Point(button.Bounds.Width / 2, button.Bounds.Height / 2), window)!.Value;
            window.MouseDown(point, MouseButton.Left);
            window.MouseUp(point, MouseButton.Left);
            window.Position = new PixelPoint(window.Position.X - 110, window.Position.Y - 70);
            window.HideToTray();

            Assert.True(settingsRequested);
            Assert.Null(savedPosition);
        }
        finally
        {
            window.Close();
            Application.Current.Styles.Remove(theme);
        }
    }, CancellationToken.None);

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public Task TopPaddingAboveEachHeaderButtonStartsWindowDrag(int buttonIndex) => Session.Dispatch(() =>
    {
        var window = new UsageOverlayWindow();
        try
        {
            window.Show();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            PixelPoint? savedPosition = null;
            window.CustomPositionChanged += position => savedPosition = position;
            var title = window.GetVisualDescendants().OfType<TextBlock>()
                .Single(text => text.Text == UiText.For(UiLanguage.English).CombinedUsageHeaderTitle);
            var header = title.GetVisualAncestors().OfType<Grid>().First();
            var buttons = header.Children.OfType<Button>().ToArray();
            Assert.Equal(6, buttons.Length);
            var button = buttons[buttonIndex];
            var point = button.TranslatePoint(new Point(button.Bounds.Width / 2, -6), window)!.Value;
            window.MouseDown(point, MouseButton.Left);
            // Simulate the compositor reporting a manual move from the top strip.
            var movedPosition = new PixelPoint(window.Position.X - 110, window.Position.Y - 70);
            window.Position = movedPosition;
            window.MouseUp(point, MouseButton.Left);
            window.HideToTray();

            Assert.Equal(movedPosition, savedPosition);
        }
        finally
        {
            window.Close();
        }
    }, CancellationToken.None);

    [Fact]
    public Task FirstOpenedFrameAlreadyHasConfiguredSizeAndLayout() => Session.Dispatch(() =>
    {
        var theme = new Avalonia.Themes.Fluent.FluentTheme();
        Application.Current!.Styles.Add(theme);
        try
        {
            foreach (var language in Enum.GetValues<UiLanguage>())
            {
                var window = new UsageOverlayWindow(new CompanionSettings
                {
                    ShowCodexFiveHour = true,
                    FontScale = 1.5,
                    LabelPercentageSpacing = 180,
                    PercentageTimeSpacing = 32
                }, UiText.For(language));
                window.PrepareForFirstShow();
                var size = new Size(window.Width, window.Height);
                var opened = false;
                window.Opened += (_, _) =>
                {
                    opened = true;
                    Assert.Equal(size, new Size(window.Width, window.Height));
                    Assert.Contains(window.GetVisualDescendants().OfType<TextBlock>(),
                        text => text.Text == "Claude" && text.Bounds.Height > 0);
                };
                window.Show();
                Avalonia.Threading.Dispatcher.UIThread.RunJobs();
                Assert.True(opened);
                Assert.Equal(size, new Size(window.Width, window.Height));
                window.Close();
            }
        }
        finally
        {
            Application.Current.Styles.Remove(theme);
        }
    }, CancellationToken.None);

    [Fact]
    public Task EnablingCodexFiveHourFitsCreditsAndStatusWithoutScrolling() => Session.Dispatch(() =>
    {
        var settings = new CompanionSettings { EnableClaudeUsage = false };
        var window = new UsageOverlayWindow(settings);
        window.UpdateUsage(UsageProvider.Codex, new RateLimitState(
            new(3, 300, 1791775260), new(85, 10080, 1792371660), null)
        {
            CreditBalance = "12.50"
        });
        window.SetStatus(DateTimeOffset.UtcNow, null, UsageProvider.Codex);
        window.Show();
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        var initialHeight = window.Height;
        window.ApplySettings(settings with { ShowCodexFiveHour = true }, UiText.For(UiLanguage.English));
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        Assert.True(window.Height >= initialHeight + OverlayLayoutOptions.FiveHourCardHeight);
        var status = window.GetVisualDescendants().OfType<TextBlock>()
            .Single(text => text.Text?.StartsWith("Last updated at") == true);
        var bottom = status.TranslatePoint(new Point(0, status.Bounds.Height), window)!.Value.Y;
        Assert.InRange(window.Height - bottom, 12.5, 14.5);
        window.ApplySettings(settings, UiText.For(UiLanguage.English));
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        Assert.Equal(initialHeight, window.Height);
        window.Close();
    }, CancellationToken.None);

    [Fact]
    public Task CreditDetailsAreVisibleBelowWeeklyProgressBar() => Session.Dispatch(() =>
    {
        var window = new UsageOverlayWindow();
        window.UpdateUsage(UsageProvider.Codex, new RateLimitState(null,
            new RateLimitWindowState(87, 10080, null), null)
        {
            CreditBalance = "12.50",
            AutomaticReloadEnabled = true
        });
        window.Show();
        var details = Assert.Single(window.GetVisualDescendants().OfType<TextBlock>(),
            text => text.Text == "Credits: 12.50, Automatic reload: Enabled");
        Assert.True(details.IsEffectivelyVisible);
        var card = details.GetVisualAncestors().OfType<Border>()
            .First(border => border.Height == OverlayLayoutOptions.WeeklyCardHeight);
        var progress = card.GetVisualDescendants().OfType<Border>()
            .First(border => border.Height == 36 && border.ClipToBounds);
        var detailsPosition = details.TranslatePoint(default, card)!.Value;
        var progressPosition = progress.TranslatePoint(default, card)!.Value;
        Assert.True(detailsPosition.Y >= progressPosition.Y + progress.Bounds.Height);
        Assert.True(detailsPosition.Y + details.Bounds.Height <= card.Bounds.Height);
        window.Close();
    }, CancellationToken.None);

    [Fact]
    public Task UsageRefreshPreservesResizedHeightAndIndependentProviderError() => Session.Dispatch(() =>
    {
        var window = new UsageOverlayWindow();
        window.Show();
        window.Height = 600;
        window.Width = 700;
        window.SetStatus(null, "Claude refresh failed", UsageProvider.Claude);
        window.UpdateUsage(UsageProvider.Codex, null);
        window.SetStatus(DateTimeOffset.UtcNow, null, UsageProvider.Codex);

        Assert.True(window.CanResize);
        Assert.Equal(600, window.Height);
        Assert.Equal(700, window.Width);
        Assert.Contains(window.GetVisualDescendants().OfType<TextBlock>(),
            text => text.Text == "Claude refresh failed");
        window.Close();
    }, CancellationToken.None);

    [Fact]
    public Task FontAndCardSizingCoexistWithProviderHeadingsAndAppearance() => Session.Dispatch(() =>
    {
        var settings = new CompanionSettings
        {
            FontScale = 1.5,
            ClaudeSessionCardHeight = 200,
            BackgroundColor = "#123456",
            LabelPercentageSpacing = 24,
            PercentageTimeSpacing = 12
        };
        var window = new UsageOverlayWindow(settings);
        window.Show();
        var texts = window.GetVisualDescendants().OfType<TextBlock>().ToArray();

        Assert.Contains(texts, text => text.Text == "Claude" && text.FontSize == 31.5);
        Assert.Contains(texts, text => text.Text == "Codex" && text.FontSize == 31.5);
        Assert.Contains(window.GetVisualDescendants().OfType<Border>(), border => border.Height == 200);
        Assert.True(window.Height >= 200);

        window.ApplySettings(settings with { EnableClaudeUsage = false }, UiText.For(UiLanguage.English));
        Assert.DoesNotContain(window.GetVisualDescendants().OfType<TextBlock>(),
            text => text.Text == "Claude" && text.IsEffectivelyVisible);
        window.Close();
    }, CancellationToken.None);

    [Fact]
    public Task CardResizeHandleUpdatesHeightAndNotifiesPersistence() => Session.Dispatch(() =>
    {
        var window = new UsageOverlayWindow();
        window.Show();
        UsageCardKind? changedKind = null;
        double changedHeight = 0;
        window.CardHeightChanged += (kind, height) => { changedKind = kind; changedHeight = height; };
        var handle = window.GetVisualDescendants().OfType<Border>()
            .First(border => border.Focusable && border.Height == 12);
        handle.RaiseEvent(new KeyEventArgs { RoutedEvent = InputElement.KeyDownEvent, Key = Key.Down });

        Assert.Equal(UsageCardKind.ClaudeSession, changedKind);
        Assert.Equal(OverlayLayoutOptions.FiveHourCardHeight + 10, changedHeight);
        Assert.Contains(window.GetVisualDescendants().OfType<Border>(), border => border.Height == changedHeight);
        window.Close();
    }, CancellationToken.None);

    [Fact]
    public Task SettingsIncludeFontControlsAndExistingAppearanceControls() => Session.Dispatch(() =>
    {
        var window = new SettingsWindow(new CompanionSettings(), UiText.For(UiLanguage.English));
        window.Show();
        Assert.Single(window.GetVisualDescendants().OfType<Slider>());
        Assert.Contains(window.GetVisualDescendants().OfType<TextBox>(), box => box.Text == string.Empty);
        window.Close();
    }, CancellationToken.None);
    [Fact]
    public Task SettingsBulkActionsRestoreStateAndDoNotCreateUnsavedEdits() => Session.Dispatch(() =>
    {
        var settings = new CompanionSettings
        {
            SettingsSections = new SettingsSectionState { Appearance = false, Logging = false }
        };
        var window = new SettingsWindow(settings, UiText.For(UiLanguage.English));
        window.Show();
        Assert.Equal(settings.SettingsSections, window.SectionState);
        var buttons = window.GetVisualDescendants().OfType<Button>().ToArray();
        buttons.Single(button => Avalonia.Automation.AutomationProperties.GetName(button) == "Collapse all")
            .RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
        foreach (var key in new[] { "appearance", "dateTime", "logging", "notifications", "usage", "window" })
            Assert.False(window.SectionState.IsExpanded(key));
        buttons.Single(button => Avalonia.Automation.AutomationProperties.GetName(button) == "Expand all")
            .RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
        Assert.Equal(new SettingsSectionState(), window.SectionState);
        var closed = false;
        window.Closed += (_, _) => closed = true;
        window.Close();
        Assert.True(closed);
    }, CancellationToken.None);
    [Fact]
    public Task DefaultCardsFitProgressBarsAtEveryFontScale() => Session.Dispatch(() =>
    {
        foreach (var scale in new[] { 0.8, 1.0, 1.5 })
        {
            var window = new UsageOverlayWindow(new CompanionSettings { ShowCodexFiveHour = true, FontScale = scale });
            window.Show();
            var progresses = window.GetVisualDescendants().OfType<Border>()
                .Where(border => border.ClipToBounds && border.CornerRadius.TopLeft == 6).ToArray();
            Assert.Equal(4, progresses.Length);
            foreach (var progress in progresses)
            {
                var card = progress.GetVisualAncestors().OfType<Border>()
                    .First(border => !double.IsNaN(border.Height));
                var details = card.GetVisualDescendants().OfType<TextBlock>()
                    .FirstOrDefault(text => text.IsVisible && text.Text?.Contains("reload", StringComparison.OrdinalIgnoreCase) == true);
                Assert.True(progress.Height <= card.Height);
                if (details is not null)
                {
                    var position = details.TranslatePoint(default, card)!.Value;
                    Assert.True(position.Y >= progress.Height);
                    Assert.True(position.Y + details.Bounds.Height <= card.Bounds.Height);
                }
                else
                    Assert.Equal(progress.Height, card.Height);
            }
            window.Close();
        }
    }, CancellationToken.None);

    [Fact]
    public void LegacyDefaultHeightsBecomeCompactWithoutChangingResizedLayouts()
    {
        var legacy = new CompanionSettings
        {
            ClaudeSessionCardHeight = 78, ClaudeWeeklyCardHeight = 96,
            CodexFiveHourCardHeight = 78, CodexWeeklyCardHeight = 96
        };
        var migrated = CompanionSettingsStore.Normalize(legacy);
        Assert.Equal(36, migrated.ClaudeSessionCardHeight);
        Assert.Equal(OverlayLayoutOptions.WeeklyCardHeight, migrated.ClaudeWeeklyCardHeight);
        Assert.Equal(200, CompanionSettingsStore.Normalize(legacy with { ClaudeSessionCardHeight = 200 }).ClaudeSessionCardHeight);
    }
    [Fact]
    public Task ResetTimeColumnStaysAtRightEdgeWhenWindowIsWider() => Session.Dispatch(() =>
    {
        var window = new UsageOverlayWindow(new CompanionSettings
        {
            EnableClaudeUsage = false, ShowCodexFiveHour = true
        });
        window.Show();
        window.Width = 700;
        window.UpdateUsage(UsageProvider.Codex, new RateLimitState(
            new RateLimitWindowState(85, 300, 1791763260),
            new RateLimitWindowState(98, 10080, 1792371660), null));
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
        var resets = window.GetVisualDescendants().OfType<TextBlock>()
            .Where(text => text.Text?.EndsWith("Local") == true).ToArray();
        Assert.Equal(2, resets.Length);
        foreach (var reset in resets)
        {
            var progress = reset.GetVisualAncestors().OfType<Border>()
                .First(border => border.ClipToBounds && border.CornerRadius.TopLeft == 6);
            var edge = reset.TranslatePoint(new Point(reset.Bounds.Width, 0), progress);
            Assert.NotNull(edge);
            Assert.InRange(progress.Bounds.Width - edge.Value.X, 9.5, 10.5);
        }
        window.Close();
    }, CancellationToken.None);
    [Fact]
    public Task ChangingLabelGapRefitsWidenedWindowInBothDirections() => Session.Dispatch(() =>
    {
        var window = new UsageOverlayWindow(new CompanionSettings { ShowCodexFiveHour = true });
        window.Show();
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        window.Width = 900;
        window.ApplyAppearance("", 8, 0);
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        var compactWidth = window.Width;
        Assert.True(compactWidth < 900);
        window.ApplyAppearance("", 8, 100);
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        Assert.InRange(window.Width - compactWidth, 99.5, 100.5);
        window.ApplyAppearance("", 8, 0);
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        Assert.Equal(compactWidth, window.Width);
        window.Close();
    }, CancellationToken.None);
    [Fact]
    public Task InitialWindowSizeUsesSavedGapsBeforeFirstShow() => Session.Dispatch(() =>
    {
        var compact = new UsageOverlayWindow(new CompanionSettings
        {
            ShowCodexFiveHour = true, LabelPercentageSpacing = 0, PercentageTimeSpacing = 0
        });
        var spaced = new UsageOverlayWindow(new CompanionSettings
        {
            ShowCodexFiveHour = true, LabelPercentageSpacing = 128, PercentageTimeSpacing = 16
        });
        Assert.InRange(spaced.Width - compact.Width, 143.5, 144.5);
        var initialWidth = spaced.Width;
        spaced.Show();
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        Assert.Equal(initialWidth, spaced.Width);
        spaced.Close();
        compact.Close();
    }, CancellationToken.None);
    [Fact]
    public Task FirstUsageResultRefitsDefaultGapsWithoutOpeningSettings() => Session.Dispatch(() =>
    {
        var window = new UsageOverlayWindow(new CompanionSettings
        {
            EnableClaudeUsage = false, ShowCodexFiveHour = true
        });
        window.Show();
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        window.UpdateUsage(UsageProvider.Codex, new RateLimitState(
            new RateLimitWindowState(63, 300, 1791763260),
            new RateLimitWindowState(94, 10080, 1792371660), null));
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        var widthWithDefaults = window.Width;
        window.ApplyAppearance("", 16, 129);
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        Assert.InRange(window.Width - widthWithDefaults, 0.5, 1.5);
        window.Close();
    }, CancellationToken.None);
    [Fact]
    public Task ResetDefaultsUpdatesFormWithoutSavingUntilApply() => Session.Dispatch(() =>
    {
        var defaults = new CompanionSettings();
        var window = new SettingsWindow(defaults with
        {
            EnableClaudeUsage = false, BackgroundColor = "#123456", FontScale = 1.4,
            LabelPercentageSpacing = 1, PercentageTimeSpacing = 2,
            ClaudeSessionCardHeight = 200, Margin = 40, Opacity = 0.7,
            ResetDateTimeFormat = "HH:mm:ss", EnableUsageLogging = true
        }, UiText.For(UiLanguage.English));
        CompanionSettings? applied = null;
        window.ApplyRequested += settings => { applied = settings; return true; };
        window.Show();
        var buttons = window.GetVisualDescendants().OfType<Button>().ToArray();
        buttons.Single(button => Equals(button.Content, "Reset to defaults"))
            .RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
        Assert.Null(applied);
        var confirmation = Assert.Single(window.OwnedWindows);
        confirmation.GetVisualDescendants().OfType<Button>().Single(button => Equals(button.Content, "No"))
            .RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        Assert.Contains(window.GetVisualDescendants().OfType<TextBox>(), box => box.Text == "#123456");
        buttons.Single(button => Equals(button.Content, "Reset to defaults"))
            .RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
        confirmation = Assert.Single(window.OwnedWindows);
        confirmation.GetVisualDescendants().OfType<Button>().Single(button => Equals(button.Content, "Yes"))
            .RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        Assert.Null(applied);
        buttons.Single(button => Equals(button.Content, "Apply"))
            .RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
        Assert.Equal(defaults, applied);
        window.Close();
    }, CancellationToken.None);
    [Fact]
    public Task FittedWindowHasOnlyItsNormalInsetBelowStatus() => Session.Dispatch(() =>
    {
        var window = new UsageOverlayWindow(new CompanionSettings { ShowCodexFiveHour = true });
        window.Show();
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
        var status = window.GetVisualDescendants().OfType<TextBlock>()
            .Single(text => text.Text == UiText.For(UiLanguage.English).WaitingForData && text.FontSize == 11);
        var bottom = status.TranslatePoint(new Point(0, status.Bounds.Height), window);
        Assert.NotNull(bottom);
        Assert.InRange(window.Height - bottom.Value.Y, 12.5, 13.5);
        window.Close();
    }, CancellationToken.None);
    [Fact]
    public Task FirstDisplayFitsAllContentWithRealScrollViewerTemplate() => Session.Dispatch(() =>
    {
        var theme = new Avalonia.Themes.Fluent.FluentTheme();
        Application.Current!.Styles.Add(theme);
        var window = new UsageOverlayWindow(new CompanionSettings { ShowCodexFiveHour = true });
        try
        {
            Assert.True(window.Height > 200);
            var initialHeight = window.Height;
            window.Show();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            Assert.Equal(initialHeight, window.Height);
            var status = window.GetVisualDescendants().OfType<TextBlock>()
                .Single(text => text.Text == UiText.For(UiLanguage.English).WaitingForData && text.FontSize == 11);
            var bottom = status.TranslatePoint(new Point(0, status.Bounds.Height), window);
            Assert.NotNull(bottom);
            Assert.InRange(window.Height - bottom.Value.Y, 12.5, 14.5);
        }
        finally
        {
            window.Close();
            Application.Current.Styles.Remove(theme);
        }
    }, CancellationToken.None);
}
