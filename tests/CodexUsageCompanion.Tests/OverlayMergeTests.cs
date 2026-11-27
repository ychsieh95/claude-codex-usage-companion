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
    // Tests of the Previous (v0.1.5) bar layout pin that design; the default is Latest.
    private static readonly CompanionSettings PreviousDesign = new() { OverlayDesign = OverlayDesignOptions.Previous };

    private static readonly HeadlessUnitTestSession Session =
        HeadlessUnitTestSession.StartNew(typeof(OverlayTestApplication));

    [Fact]
    public Task HeaderWhitespaceStartsCustomPositionTracking() => Session.Dispatch(() =>
    {
        var window = new UsageOverlayWindow(PreviousDesign);
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
        var window = new UsageOverlayWindow(PreviousDesign);
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
        var settings = PreviousDesign with { EnableClaudeUsage = false };
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
            .Single(text => text.IsEffectivelyVisible && text.Text?.StartsWith("Last updated at") == true);
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
        var window = new UsageOverlayWindow(PreviousDesign);
        window.UpdateUsage(UsageProvider.Codex, new RateLimitState(null,
            new RateLimitWindowState(87, 10080, null), null)
        {
            CreditBalance = "12.50",
            AutomaticReloadEnabled = true
        });
        window.Show();
        var details = Assert.Single(window.GetVisualDescendants().OfType<TextBlock>(),
            text => text.IsEffectivelyVisible && text.Text == "Credits: 12.50, Automatic reload: Enabled");
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
        var settings = PreviousDesign with
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
        var window = new UsageOverlayWindow(PreviousDesign);
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
    public Task SettingsBadgeAverageDescriptionFollowsTheSelectedMethod() => Session.Dispatch(() =>
    {
        var text = UiText.For(UiLanguage.English);
        var window = new SettingsWindow(new CompanionSettings
        {
            SettingsPage = SettingsPageOptions.Appearance,
            BadgeAverage = BadgeAverageOptions.WindowLength
        }, text);
        try
        {
            window.Show();
            string Description(string method) => text.BadgeAverageChoices.Single(choice => choice.Value == method).Description;
            TextBlock? Shown(string method) => window.GetVisualDescendants().OfType<TextBlock>()
                .SingleOrDefault(block => block.Text == Description(method));
            Assert.NotNull(Shown(BadgeAverageOptions.WindowLength));

            window.SelectBadgeAverage(BadgeAverageOptions.Pace);
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            Assert.NotNull(Shown(BadgeAverageOptions.Pace));
            Assert.Null(Shown(BadgeAverageOptions.WindowLength));
        }
        finally
        {
            window.Close();
        }
    }, CancellationToken.None);

    [Fact]
    public Task SettingsSidebarShowsOnePageAndRestoresItWithoutUnsavedEdits() => Session.Dispatch(() =>
    {
        var window = new SettingsWindow(new CompanionSettings { SettingsPage = SettingsPageOptions.Logging },
            UiText.For(UiLanguage.English));
        window.Show();
        Assert.Equal(SettingsPageOptions.Logging, window.SelectedPage);
        var headings = window.GetVisualDescendants().OfType<TextBlock>().Where(text => text.FontSize == 20).ToArray();
        Assert.Equal(7, headings.Length);
        Assert.Equal("Logging", Assert.Single(headings, text => text.IsEffectivelyVisible).Text);

        var sidebar = window.GetVisualDescendants().OfType<ListBox>().Single();
        sidebar.SelectedIndex = 1;
        Assert.Equal(SettingsPageOptions.Appearance, window.SelectedPage);
        Assert.Equal("Appearance", Assert.Single(headings, text => text.IsEffectivelyVisible).Text);
        Assert.Contains(window.GetVisualDescendants().OfType<CheckBox>(),
            box => box.IsEffectivelyVisible && Equals(box.Content, "Show each limit's name and reset time on one row"));

        // Switching pages is view state, so closing does not ask about unsaved changes.
        var closed = false;
        window.Closed += (_, _) => closed = true;
        window.Close();
        Assert.True(closed);
    }, CancellationToken.None);

    [Fact]
    public Task LatestDesignCanShowNameAndResetTimeOnOneRow() => Session.Dispatch(() =>
    {
        var settings = new CompanionSettings { EnableCodexUsage = false, ShowClaudeWeekly = false };
        var window = new UsageOverlayWindow(settings);
        try
        {
            window.UpdateUsage(UsageProvider.Claude, new RateLimitState(
                new RateLimitWindowState(68, 300, DateTimeOffset.UtcNow.AddMinutes(134).ToUnixTimeSeconds()), null, null));
            window.Show();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            TextBlock Find(string prefix) => window.GetVisualDescendants().OfType<TextBlock>()
                .Single(text => text.IsEffectivelyVisible && text.Text?.StartsWith(prefix) == true);
            double Top(TextBlock text) => text.TranslatePoint(default, window)!.Value.Y;
            Assert.True(Top(Find("Resets in")) > Top(Find("Current session")));
            var twoRowHeight = window.Height;

            window.ApplySettings(settings with { ResetTimeOnNameRow = true }, UiText.For(UiLanguage.English));
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            var title = Find("Current session");
            var reset = Find("Resets in");
            Assert.InRange(Top(reset) + reset.Bounds.Height / 2 - (Top(title) + title.Bounds.Height / 2), -3, 3);
            Assert.True(reset.TranslatePoint(default, window)!.Value.X >
                title.TranslatePoint(new Point(title.Bounds.Width, 0), window)!.Value.X);
            Assert.True(window.Height < twoRowHeight);
        }
        finally
        {
            window.Close();
        }
    }, CancellationToken.None);
    [Fact]
    public Task DefaultCardsFitProgressBarsAtEveryFontScale() => Session.Dispatch(() =>
    {
        foreach (var scale in new[] { 0.8, 1.0, 1.5 })
        {
            var window = new UsageOverlayWindow(new CompanionSettings { OverlayDesign = OverlayDesignOptions.Previous, ShowCodexFiveHour = true, FontScale = scale });
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
            OverlayDesign = OverlayDesignOptions.Previous,
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
        var window = new UsageOverlayWindow(new CompanionSettings { OverlayDesign = OverlayDesignOptions.Previous, ShowCodexFiveHour = true });
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
            OverlayDesign = OverlayDesignOptions.Previous,
            ShowCodexFiveHour = true, LabelPercentageSpacing = 0, PercentageTimeSpacing = 0
        });
        var spaced = new UsageOverlayWindow(new CompanionSettings
        {
            OverlayDesign = OverlayDesignOptions.Previous,
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
            OverlayDesign = OverlayDesignOptions.Previous,
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
        var window = new UsageOverlayWindow(new CompanionSettings { OverlayDesign = OverlayDesignOptions.Previous, ShowCodexFiveHour = true });
        window.Show();
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
        var status = window.GetVisualDescendants().OfType<TextBlock>()
            .Single(text => text.IsEffectivelyVisible &&
                text.Text == UiText.For(UiLanguage.English).WaitingForData && text.FontSize == 11);
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
        var window = new UsageOverlayWindow(new CompanionSettings { OverlayDesign = OverlayDesignOptions.Previous, ShowCodexFiveHour = true });
        try
        {
            Assert.True(window.Height > 200);
            var initialHeight = window.Height;
            window.Show();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            Assert.Equal(initialHeight, window.Height);
            var status = window.GetVisualDescendants().OfType<TextBlock>()
                .Single(text => text.IsEffectivelyVisible &&
                text.Text == UiText.For(UiLanguage.English).WaitingForData && text.FontSize == 11);
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
    [Fact]
    public Task LatestDesignShowsProviderCardsCountdownsAndLowestBadge() => Session.Dispatch(() =>
    {
        var window = new UsageOverlayWindow();
        try
        {
            var now = DateTimeOffset.UtcNow;
            var sessionReset = now.AddMinutes(134).AddSeconds(-30).ToUnixTimeSeconds();
            window.UpdateUsage(UsageProvider.Claude, new RateLimitState(
                new RateLimitWindowState(68, 300, sessionReset),
                new RateLimitWindowState(41, 10080, now.AddDays(3).ToUnixTimeSeconds()), null));
            window.UpdateUsage(UsageProvider.Codex, new RateLimitState(
                null, new RateLimitWindowState(82, 10080, null), null) { CreditBalance = "12.50" });
            window.SetStatus(DateTimeOffset.UtcNow, null, UsageProvider.Codex);
            window.Show();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            var visible = window.GetVisualDescendants().OfType<TextBlock>()
                .Where(text => text.IsEffectivelyVisible).Select(text => text.Text).ToArray();

            Assert.Contains("Usage overview", visible);
            Assert.DoesNotContain("USAGE", visible);
            Assert.Contains("Current session", visible);
            Assert.Contains("Current week (All)", visible);
            Assert.Contains("Weekly limit", visible);
            Assert.DoesNotContain("5-hour limit", visible);
            var clock = DateTimeOffset.FromUnixTimeSeconds(sessionReset).ToLocalTime()
                .ToString("HH:mm", System.Globalization.CultureInfo.InvariantCulture);
            Assert.Contains($"Resets in 2h 14m · {clock}", visible);
            Assert.Contains("Reset time unavailable", visible);
            Assert.Contains("Credits: 12.50, Automatic reload: Disabled", visible);
            Assert.Contains(visible, text => text?.StartsWith("Last updated at") == true);
            // The badge names the lowest shown limit, which is Claude's weekly one.
            var badge = window.GetVisualDescendants().OfType<Border>()
                .Single(border => ToolTip.GetTip(border) is string tip && tip.StartsWith("Lowest remaining"));
            Assert.True(badge.IsEffectivelyVisible);
            Assert.StartsWith("Lowest remaining: Claude Current week (All), 41%", (string)ToolTip.GetTip(badge)!);
            Assert.Contains(badge.GetVisualDescendants().OfType<TextBlock>(), text => text.Text == "41");
            // The status line moves under the heading, so the bottom line stays hidden.
            Assert.Single(window.GetVisualDescendants().OfType<TextBlock>(),
                text => text.IsEffectivelyVisible && text.Text?.StartsWith("Last updated at") == true);
        }
        finally
        {
            window.Close();
        }
    }, CancellationToken.None);

    [Fact]
    public Task LatestDesignPlacesPaceMarkerAndExplainsPace() => Session.Dispatch(() =>
    {
        var window = new UsageOverlayWindow(new CompanionSettings { EnableCodexUsage = false, ShowClaudeWeekly = false });
        try
        {
            window.Show();
            // 60% of the five-hour window has passed, so an even pace leaves 40%.
            var reset = DateTimeOffset.UtcNow.AddMinutes(120).ToUnixTimeSeconds();
            window.UpdateUsage(UsageProvider.Claude, new RateLimitState(
                new RateLimitWindowState(23, 300, reset), null, null));
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            var marker = window.GetVisualDescendants().OfType<Grid>()
                .Single(grid => grid.IsEffectivelyVisible && ToolTip.GetTip(grid) is string tip && tip.StartsWith("Even pace"));
            Assert.Equal("Even pace: about 40% should remain now. Your usage is running faster than pace.",
                ToolTip.GetTip(marker));
            var track = ((Grid)marker.Parent!).Children.OfType<Border>().First();
            var center = marker.Margin.Left + marker.Width / 2;
            Assert.InRange(center / track.Bounds.Width, 0.39, 0.41);

            window.ApplySettings(new CompanionSettings
            {
                EnableCodexUsage = false, ShowClaudeWeekly = false, ShowPacePointer = false
            }, UiText.For(UiLanguage.English));
            Assert.False(marker.IsVisible);
        }
        finally
        {
            window.Close();
        }
    }, CancellationToken.None);

    [Fact]
    public Task SwitchingDesignSwapsLayoutAndRefitsWindow() => Session.Dispatch(() =>
    {
        var latest = new CompanionSettings { ShowCodexFiveHour = true };
        var window = new UsageOverlayWindow(latest);
        try
        {
            window.UpdateUsage(UsageProvider.Codex, new RateLimitState(
                new RateLimitWindowState(63, 300, 1791763260),
                new RateLimitWindowState(94, 10080, 1792371660), null));
            window.Show();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            var latestSize = new Size(window.Width, window.Height);
            Assert.Equal(420, window.Width);

            window.ApplySettings(latest with { OverlayDesign = OverlayDesignOptions.Previous }, UiText.For(UiLanguage.English));
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            var texts = window.GetVisualDescendants().OfType<TextBlock>().Where(text => text.IsEffectivelyVisible).ToArray();
            Assert.Contains(texts, text => text.Text == "USAGE");
            Assert.DoesNotContain(texts, text => text.Text == "Usage overview");
            Assert.NotEqual(latestSize, new Size(window.Width, window.Height));

            window.ApplySettings(latest, UiText.For(UiLanguage.English));
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            Assert.Equal(latestSize, new Size(window.Width, window.Height));
        }
        finally
        {
            window.Close();
        }
    }, CancellationToken.None);

    [Fact]
    public Task LatestDesignShowsErrorsBelowCards() => Session.Dispatch(() =>
    {
        var window = new UsageOverlayWindow();
        try
        {
            window.Show();
            window.SetStatus(null, "Claude refresh failed", UsageProvider.Claude);
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            Assert.Contains(window.GetVisualDescendants().OfType<TextBlock>(),
                text => text.Text == "Claude refresh failed" && text.IsEffectivelyVisible);
            window.SetStatus(DateTimeOffset.UtcNow, null, UsageProvider.Claude);
            Assert.DoesNotContain(window.GetVisualDescendants().OfType<TextBlock>(),
                text => text.Text == "Claude refresh failed" && text.IsEffectivelyVisible);
        }
        finally
        {
            window.Close();
        }
    }, CancellationToken.None);
    [Fact]
    public Task LatestDesignGrowsToFitUsageThatArrivesBeforeOrAfterShow() => Session.Dispatch(() =>
    {
        var theme = new Avalonia.Themes.Fluent.FluentTheme();
        Application.Current!.Styles.Add(theme);
        var early = new UsageOverlayWindow();
        var late = new UsageOverlayWindow();
        try
        {
            var state = new RateLimitState(
                new RateLimitWindowState(23, 300, DateTimeOffset.UtcNow.AddMinutes(48).ToUnixTimeSeconds()),
                new RateLimitWindowState(82, 10080, DateTimeOffset.UtcNow.AddDays(5).ToUnixTimeSeconds()), null)
            {
                CreditBalance = "0.00"
            };
            early.UpdateUsage(UsageProvider.Codex, state);
            early.Show();
            late.Show();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            late.UpdateUsage(UsageProvider.Codex, state);
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            foreach (var window in new[] { early, late })
            {
                window.UpdateLayout();
                var scroll = window.GetVisualDescendants().OfType<ScrollViewer>().Single();
                Assert.InRange(scroll.Extent.Height - scroll.Viewport.Height, -0.5, 0.5);
            }

            // A window the user resized keeps its size when usage changes.
            late.Height = 600;
            late.UpdateUsage(UsageProvider.Codex, state with { CreditBalance = null });
            Assert.Equal(600, late.Height);
        }
        finally
        {
            early.Close();
            late.Close();
            Application.Current.Styles.Remove(theme);
        }
    }, CancellationToken.None);
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public Task LatestPercentageSpansFromNameToBarBottom(bool oneRow) => Session.Dispatch(() =>
    {
        var window = new UsageOverlayWindow(new CompanionSettings
        {
            EnableCodexUsage = false, ShowClaudeWeekly = false, ResetTimeOnNameRow = oneRow
        });
        try
        {
            window.UpdateUsage(UsageProvider.Claude, new RateLimitState(
                new RateLimitWindowState(68, 300, DateTimeOffset.UtcNow.AddMinutes(134).ToUnixTimeSeconds()), null, null));
            window.Show();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            var number = window.GetVisualDescendants().OfType<FittedNumber>().Single(n => n.IsEffectivelyVisible);
            var title = window.GetVisualDescendants().OfType<TextBlock>()
                .Single(text => text.IsEffectivelyVisible && text.Text == "Current session");
            var track = window.GetVisualDescendants().OfType<Border>()
                .Single(border => border.IsEffectivelyVisible && border.Height == 6);
            var numberTop = number.TranslatePoint(default, window)!.Value.Y;
            var titleTop = title.TranslatePoint(default, window)!.Value.Y;
            var trackBottom = track.TranslatePoint(new Point(0, track.Bounds.Height), window)!.Value.Y;

            Assert.Equal("68", number.Text);
            // The digits start at the name's capitals and end at the bar's bottom edge.
            Assert.InRange(numberTop - titleTop, 2, 5);
            Assert.InRange(numberTop + number.TargetHeight - trackBottom, -0.5, 0.5);
            // The bar ends before the percentage column.
            Assert.True(track.TranslatePoint(new Point(track.Bounds.Width, 0), window)!.Value.X <
                number.TranslatePoint(default, window)!.Value.X);
        }
        finally
        {
            window.Close();
        }
    }, CancellationToken.None);
    [Fact]
    public Task LatestCreditsHaveADividerAndPercentagesUseMonospacedDigits() => Session.Dispatch(() =>
    {
        var settings = new CompanionSettings { EnableClaudeUsage = false };
        var window = new UsageOverlayWindow(settings);
        try
        {
            var state = new RateLimitState(null, new RateLimitWindowState(82, 10080, null), null) { CreditBalance = "0.00" };
            window.UpdateUsage(UsageProvider.Codex, state);
            window.Show();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            var credits = window.GetVisualDescendants().OfType<TextBlock>()
                .Single(text => text.IsEffectivelyVisible && text.Text?.StartsWith("Credits:") == true);
            var card = (StackPanel)credits.Parent!;
            var divider = (Border)card.Children[card.Children.IndexOf(credits) - 1];
            Assert.Equal(1, divider.Height);
            Assert.True(divider.IsVisible);

            window.UpdateUsage(UsageProvider.Codex, state with { CreditBalance = null });
            Assert.False(credits.IsVisible);
            Assert.False(divider.IsVisible);

            var number = window.GetVisualDescendants().OfType<FittedNumber>().First(n => n.IsEffectivelyVisible);
            Assert.Contains("monospace", number.FontFamily!.ToString());
        }
        finally
        {
            window.Close();
        }
    }, CancellationToken.None);

    [Fact]
    public Task HeaderBadgeFollowsTheChosenMode() => Session.Dispatch(() =>
    {
        var settings = new CompanionSettings { ShowCodexFiveHour = true };
        var window = new UsageOverlayWindow(settings);
        try
        {
            window.UpdateUsage(UsageProvider.Claude, new RateLimitState(
                new RateLimitWindowState(68, 300, null), new RateLimitWindowState(41, 10080, null), null));
            window.UpdateUsage(UsageProvider.Codex, new RateLimitState(
                new RateLimitWindowState(23, 300, null), new RateLimitWindowState(82, 10080, null), null));
            window.Show();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            var badge = window.GetVisualDescendants().OfType<Border>()
                .Single(border => ToolTip.GetTip(border) is string tip && tip.StartsWith("Lowest remaining"));
            string Value() => badge.GetVisualDescendants().OfType<TextBlock>().First().Text!;
            Assert.Equal("23", Value());

            window.ApplySettings(settings with { HeaderBadge = HeaderBadgeOptions.Average }, UiText.For(UiLanguage.English));
            Assert.Equal("54", Value());
            Assert.StartsWith("Average remaining of 4 limits: 54%", (string)ToolTip.GetTip(badge)!);

            window.ApplySettings(settings with { HeaderBadge = HeaderBadgeOptions.Claude }, UiText.For(UiLanguage.English));
            Assert.Equal("41", Value());
            Assert.StartsWith("Lowest remaining: Claude Current week (All), 41%", (string)ToolTip.GetTip(badge)!);

            window.ApplySettings(settings with { HeaderBadge = HeaderBadgeOptions.ClaudeSession }, UiText.For(UiLanguage.English));
            Assert.Equal("68", Value());
            Assert.StartsWith("Remaining: Claude Current session, 68%", (string)ToolTip.GetTip(badge)!);

            window.ApplySettings(settings with { HeaderBadge = HeaderBadgeOptions.CodexAverage }, UiText.For(UiLanguage.English));
            Assert.Equal("53", Value());
            Assert.StartsWith("Average remaining of 2 Codex limits: 53%", (string)ToolTip.GetTip(badge)!);

            // The pace gap shows as signed points, without the percent sign.
            window.ApplySettings(settings with { HeaderBadge = HeaderBadgeOptions.Average, BadgeAverage = BadgeAverageOptions.Pace },
                UiText.For(UiLanguage.English));
            Assert.Equal("54", Value());
            Assert.StartsWith("Average remaining of 4 limits: 54%", (string)ToolTip.GetTip(badge)!);

            window.ApplySettings(settings with { HeaderBadge = HeaderBadgeOptions.Hidden }, UiText.For(UiLanguage.English));
            Assert.False(badge.IsVisible);
        }
        finally
        {
            window.Close();
        }
    }, CancellationToken.None);

    [Fact]
    public Task PaceGapBadgeShowsSignedPointsWithoutAPercentSign() => Session.Dispatch(() =>
    {
        var settings = new CompanionSettings
        {
            HeaderBadge = HeaderBadgeOptions.ClaudeAverage,
            BadgeAverage = BadgeAverageOptions.Pace
        };
        var window = new UsageOverlayWindow(settings);
        try
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            // Session: 68% left, 1h of 5h to go (+48). Week: 41% left, 3 of 7 days to go (−2).
            window.UpdateUsage(UsageProvider.Claude, new RateLimitState(
                new RateLimitWindowState(68, 300, now + 3600),
                new RateLimitWindowState(41, 10080, now + 3 * 24 * 3600), null));
            window.Show();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            var badge = window.GetVisualDescendants().OfType<Border>()
                .Single(border => ToolTip.GetTip(border) is string tip && tip.StartsWith("Average gap from an even pace"));
            var texts = badge.GetVisualDescendants().OfType<TextBlock>().ToList();
            Assert.Equal("+23", texts[0].Text);
            Assert.False(texts[1].IsVisible);
            Assert.StartsWith("Average gap from an even pace of 2 Claude limits: +23 points", (string)ToolTip.GetTip(badge)!);

            window.ApplySettings(settings with { BadgeAverage = BadgeAverageOptions.TimeToReset }, UiText.For(UiLanguage.English));
            Assert.Equal("41", texts[0].Text);
            Assert.True(texts[1].IsVisible);
            Assert.StartsWith("Average remaining of 2 Claude limits, weighted by time to reset: 41%", (string)ToolTip.GetTip(badge)!);
        }
        finally
        {
            window.Close();
        }
    }, CancellationToken.None);

    [Fact]
    public Task BadgeMenuSwitchesTheAverageMethod() => Session.Dispatch(() =>
    {
        var settings = new CompanionSettings { HeaderBadge = HeaderBadgeOptions.ClaudeAverage };
        var window = new UsageOverlayWindow(settings);
        var requested = new List<string>();
        window.BadgeAverageRequested += requested.Add;
        try
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            window.UpdateUsage(UsageProvider.Claude, new RateLimitState(
                new RateLimitWindowState(68, 300, now + 3600),
                new RateLimitWindowState(41, 10080, now + 3 * 24 * 3600), null));
            window.Show();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            var badge = window.GetVisualDescendants().OfType<Border>()
                .Single(border => ToolTip.GetTip(border) is string tip && tip.StartsWith("Average remaining"));
            string Value() => badge.GetVisualDescendants().OfType<TextBlock>().First().Text!;
            Assert.Equal("55", Value());

            List<MenuItem> Methods() => window.CreateBadgeMenu().Items.OfType<MenuItem>()
                .Single(item => (string)item.Header! == "Badge average").Items.OfType<MenuItem>().ToList();
            Assert.Equal(["Plain average", "Weighted by time to reset", "Weighted by window length", "Gap from even pace"],
                Methods().Select(item => (string)item.Header!));
            Assert.True(Methods()[0].IsChecked);
            // Each method explains itself in its tooltip.
            Assert.Equal(UiText.For(UiLanguage.English).BadgeAverageChoices.Select(choice => choice.Description),
                Methods().Select(item => (string)ToolTip.GetTip(item)!));
            Assert.StartsWith("Remaining percent minus the share of time left", (string)ToolTip.GetTip(Methods()[3])!);

            Methods()[3].RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(MenuItem.ClickEvent));
            Assert.Equal("+23", Value());
            Assert.Equal([BadgeAverageOptions.Pace], requested);
            Assert.True(Methods()[3].IsChecked);
            Assert.False(Methods()[0].IsChecked);

            // Picking the current method again changes nothing.
            Methods()[3].RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(MenuItem.ClickEvent));
            Assert.Single(requested);
        }
        finally
        {
            window.Close();
        }
    }, CancellationToken.None);

    [Fact]
    public Task ClickingTheBadgeMenuSwitchesAndReportsTheMode() => Session.Dispatch(() =>
    {
        var settings = new CompanionSettings();
        var window = new UsageOverlayWindow(settings);
        var requested = new List<(string Mode, bool Confirm)>();
        window.HeaderBadgeRequested += (mode, confirm) => requested.Add((mode, confirm));
        try
        {
            window.UpdateUsage(UsageProvider.Claude, new RateLimitState(
                new RateLimitWindowState(68, 300, null), new RateLimitWindowState(41, 10080, null), null));
            window.Show();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            var items = window.CreateBadgeMenu().Items.OfType<MenuItem>().ToList();
            // Every badge choice, then the average method submenu.
            Assert.Equal(UiText.For(UiLanguage.English).HeaderBadgeChoices.Count + 1, items.Count);
            Assert.Equal("Badge average", items[^1].Header);
            Assert.True(items.Single(item => (string)item.Header! == "Lowest remaining limit").IsChecked);

            items.Single(item => (string)item.Header! == "Claude: current session")
                .RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(MenuItem.ClickEvent));
            Assert.Equal([(HeaderBadgeOptions.ClaudeSession, true)], requested);
            var badge = window.GetVisualDescendants().OfType<Border>()
                .Single(border => ToolTip.GetTip(border) is string tip && tip.StartsWith("Remaining: Claude"));
            Assert.Equal("68", badge.GetVisualDescendants().OfType<TextBlock>().First().Text);
            Assert.True(window.CreateBadgeMenu().Items.OfType<MenuItem>()
                .Single(item => (string)item.Header! == "Claude: current session").IsChecked);

            // Hiding asks first: declining keeps the badge, confirming hides it.
            var confirmations = 0;
            var answer = new UsageOverlayWindow.HideBadgeAnswer(false, false);
            window.ConfirmHideBadge = () => { confirmations++; return Task.FromResult(answer); };
            void Pick(string label)
            {
                window.CreateBadgeMenu().Items.OfType<MenuItem>().Single(item => (string)item.Header! == label)
                    .RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(MenuItem.ClickEvent));
                Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            }
            Pick("Hidden");
            Assert.Equal(1, confirmations);
            Assert.True(badge.IsVisible);
            Assert.Equal([(HeaderBadgeOptions.ClaudeSession, true)], requested);

            answer = new UsageOverlayWindow.HideBadgeAnswer(true, false);
            Pick("Hidden");
            Assert.Equal(2, confirmations);
            Assert.False(badge.IsVisible);
            Assert.Equal((HeaderBadgeOptions.Hidden, true), requested[^1]);

            // "Do not show again" turns the prompt off and reports it.
            Pick("Claude: weekly");
            answer = new UsageOverlayWindow.HideBadgeAnswer(true, true);
            Pick("Hidden");
            Assert.Equal(3, confirmations);
            Assert.Equal((HeaderBadgeOptions.Hidden, false), requested[^1]);

            Pick("Claude: weekly");
            Pick("Hidden");
            Assert.Equal(3, confirmations);
            Assert.Equal((HeaderBadgeOptions.Hidden, false), requested[^1]);

            // Settings with the prompt turned back on (for example after a reset) ask again.
            window.ApplySettings(settings with { HeaderBadge = HeaderBadgeOptions.Lowest }, UiText.For(UiLanguage.English));
            Pick("Hidden");
            Assert.Equal(4, confirmations);
        }
        finally
        {
            window.Close();
        }
    }, CancellationToken.None);

    [Theory]
    [InlineData(true, true, 1.0)]
    [InlineData(true, false, 0.75)]
    [InlineData(false, true, 1.5)]
    [InlineData(false, false, 1.0)]
    public Task RingsRespectLimitsScaleAndProviderVisibility(bool current, bool weekly, double scale) => Session.Dispatch(() =>
    {
        var settings = new CompanionSettings
        {
            OverlayDesign = OverlayDesignOptions.Rings, EnableCodexUsage = false,
            ShowClaudeSession = current, ShowClaudeWeekly = weekly, FontScale = scale
        };
        var window = new UsageOverlayWindow(settings);
        try
        {
            var text = UiText.For(UiLanguage.English);
            var state = new RateLimitState(
                new RateLimitWindowState(68, 300, DateTimeOffset.UtcNow.AddHours(2).ToUnixTimeSeconds()),
                new RateLimitWindowState(23, 10080, DateTimeOffset.UtcNow.AddDays(3).ToUnixTimeSeconds()), null);
            window.UpdateUsage(UsageProvider.Claude, state);
            window.Show();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            TextBlock[] VisibleText() => window.GetVisualDescendants().OfType<TextBlock>()
                .Where(control => control.IsEffectivelyVisible).ToArray();
            Assert.Equal(current, VisibleText().Any(control => control.Text == "68%"));
            Assert.Equal(weekly, VisibleText().Any(control => control.Text == "23%"));
            Assert.DoesNotContain(VisibleText(), control => control.Text == "Codex");
            var dials = window.GetVisualDescendants().OfType<Control>()
                .Where(control => control.GetType().Name == "RingDrawing" && control.IsEffectivelyVisible).ToArray();
            Assert.Equal(current || weekly ? 1 : 0, dials.Length);
            foreach (var dial in dials)
                Assert.InRange(dial.Bounds.Width, Math.Floor(140 * OverlayLayoutOptions.NormalizeFontScale(scale)), Math.Ceiling(140 * OverlayLayoutOptions.NormalizeFontScale(scale)));
            foreach (var percent in VisibleText().Where(control => control.Text is "68%" or "23%"))
            {
                Assert.Equal(20 * OverlayLayoutOptions.NormalizeFontScale(scale), percent.FontSize);
                Assert.Contains("Resets in", ToolTip.GetTip(percent) as string);
            }

            // Switch theme and design with data already present, then return to Rings.
            window.RequestedThemeVariant = Avalonia.Styling.ThemeVariant.Light;
            window.ApplySettings(settings with { OverlayDesign = OverlayDesignOptions.Previous }, text);
            Assert.DoesNotContain(window.GetVisualDescendants().OfType<Control>(),
                control => control.GetType().Name == "RingDrawing" && control.IsEffectivelyVisible);
            window.ApplySettings(settings, text);
            window.UpdateUsage(UsageProvider.Claude, null);
            Assert.Equal((current ? 1 : 0) + (weekly ? 1 : 0), VisibleText().Count(control => control.Text == "—"));
            window.SetStatus(null, "Claude refresh failed", UsageProvider.Claude);
            Assert.Contains(VisibleText(), control => control.Text == "Claude refresh failed");
        }
        finally
        {
            window.Close();
        }
    }, CancellationToken.None);

    [Theory]
    [InlineData(UiLanguage.English, "Rings")]
    [InlineData(UiLanguage.TraditionalChinese, "圓環")]
    [InlineData(UiLanguage.SimplifiedChinese, "圆环")]
    public void RingsDesignLabelIsLocalized(UiLanguage language, string expected)
    {
        Assert.Equal(expected, UiText.For(language).RingsOverlayDesign);
    }

    [Fact]
    public Task AboutLinksToTheProjectAndTheOriginalProject() => Session.Dispatch(() =>
    {
        var window = new SettingsWindow(new CompanionSettings { SettingsPage = SettingsPageOptions.Updates },
            UiText.For(UiLanguage.English));
        try
        {
            window.Show();
            var links = window.GetVisualDescendants().OfType<HyperlinkButton>()
                .Where(link => link.IsEffectivelyVisible).Select(link => link.NavigateUri!.ToString()).ToArray();
            Assert.Equal(
                [AppUpdateChecker.RepositoryUrl, "https://github.com/gkfriend/codex-usage-companion"],
                links);
        }
        finally
        {
            window.Close();
        }
    }, CancellationToken.None);
}
