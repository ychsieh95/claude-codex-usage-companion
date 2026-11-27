using System.Text.Json;
using System.Xml.Linq;
using Xunit;

namespace CodexUsageCompanion.Tests;

public sealed class PlasmaWidgetPackageTests
{
    private const string WidgetId = "com.github.ychsieh95.claude-codex-usage-companion";

    [Fact]
    public void MetadataDeclaresAPlasma6Applet()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "plasmoid", "metadata.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        var root = document.RootElement;

        Assert.Equal("Plasma/Applet", root.GetProperty("KPackageStructure").GetString());
        Assert.Equal("6.0", root.GetProperty("X-Plasma-API-Minimum-Version").GetString());
        Assert.Equal(WidgetId, root.GetProperty("KPlugin").GetProperty("Id").GetString());
    }

    [Fact]
    public void PackageContainsItsPlasma6EntryPointAndComponents()
    {
        var uiDirectory = Path.Combine(AppContext.BaseDirectory, "plasmoid", "contents", "ui");
        var main = File.ReadAllText(Path.Combine(uiDirectory, "main.qml"));

        Assert.Contains("PlasmoidItem {", main, StringComparison.Ordinal);
        Assert.Contains("claude-codex-usage-companion status --json", main, StringComparison.Ordinal);
        Assert.Contains("engine: \"executable\"", main, StringComparison.Ordinal);
        Assert.Contains("Plasmoid.configuration.showClaudeSession", main, StringComparison.Ordinal);
        Assert.Contains("Plasmoid.configuration.showCodexWeekly", main, StringComparison.Ordinal);

        var fullRepresentation = File.ReadAllText(Path.Combine(uiDirectory, "FullRepresentation.qml"));
        Assert.Contains("showFiveHour: Plasmoid.configuration.showClaudeSession", fullRepresentation, StringComparison.Ordinal);
        Assert.Contains("showFiveHour: Plasmoid.configuration.showCodexFiveHour", fullRepresentation, StringComparison.Ordinal);
        Assert.Contains("showWeekly: Plasmoid.configuration.showCodexWeekly", fullRepresentation, StringComparison.Ordinal);
        Assert.Contains("../images/claude.svg", fullRepresentation, StringComparison.Ordinal);
        Assert.Contains("../images/codex.svg", fullRepresentation, StringComparison.Ordinal);

        var providerSection = File.ReadAllText(Path.Combine(uiDirectory, "ProviderSection.qml"));
        Assert.Contains("visible: section.showFiveHour", providerSection, StringComparison.Ordinal);
        Assert.Contains("visible: section.showWeekly", providerSection, StringComparison.Ordinal);
        Assert.Contains("Image {", providerSection, StringComparison.Ordinal);

        foreach (var component in new[]
                 {
                     "CompactRepresentation.qml",
                     "ConfigGeneral.qml",
                     "FullRepresentation.qml",
                     "InlineMessage.qml",
                     "ProviderSection.qml",
                     "UsageWindow.qml"
                 })
        {
            Assert.True(File.Exists(Path.Combine(uiDirectory, component)), $"Missing {component}");
        }

        Assert.True(File.Exists(Path.Combine(
            AppContext.BaseDirectory, "plasmoid", "contents", "config", "config.qml")));

        var imageDirectory = Path.Combine(
            AppContext.BaseDirectory, "plasmoid", "contents", "images");
        var claudeIcon = File.ReadAllText(Path.Combine(imageDirectory, "claude.svg"));
        var codexIcon = File.ReadAllText(Path.Combine(imageDirectory, "codex.svg"));
        Assert.Contains("#D77655", claudeIcon, StringComparison.Ordinal);
        Assert.Contains("#FCF2EE", claudeIcon, StringComparison.Ordinal);
        Assert.Contains("#10A37F", codexIcon, StringComparison.Ordinal);
    }

    [Fact]
    public void ConfigurationDefaultsShowEveryUsageWindow()
    {
        var path = Path.Combine(
            AppContext.BaseDirectory, "plasmoid", "contents", "config", "main.xml");
        var document = XDocument.Load(path);
        var xmlNamespace = document.Root!.Name.Namespace;
        var entries = document.Descendants(xmlNamespace + "entry")
            .ToDictionary(
                entry => entry.Attribute("name")!.Value,
                entry => entry.Element(xmlNamespace + "default")!.Value);

        Assert.Equal("0", entries["compactViewStyle"]);
        Assert.Equal("1", entries["refreshIntervalMinutes"]);
        Assert.Equal("true", entries["showClaudeSession"]);
        Assert.Equal("true", entries["showClaudeWeekly"]);
        Assert.Equal("true", entries["showCodexFiveHour"]);
        Assert.Equal("true", entries["showCodexWeekly"]);
        Assert.Equal("true", entries["showPacePointer"]);
        Assert.Equal("0", entries["badgeMode"]);
        Assert.DoesNotContain("showLowestRemaining", entries.Keys);
    }

    [Fact]
    public void PopupPaceMarkerIsARingedDot()
    {
        var usageWindow = File.ReadAllText(Path.Combine(
            AppContext.BaseDirectory, "plasmoid", "contents", "ui", "UsageWindow.qml"));
        var marker = usageWindow[usageWindow.IndexOf("id: paceMarker", StringComparison.Ordinal)..];
        marker = marker[..marker.IndexOf("HoverHandler", StringComparison.Ordinal)];

        Assert.Contains("radius: width / 2", marker, StringComparison.Ordinal);
        Assert.Contains("border.color: Kirigami.Theme.backgroundColor", marker, StringComparison.Ordinal);
        Assert.DoesNotContain("rotation: 45", marker, StringComparison.Ordinal);
    }

    [Fact]
    public void PopupBadgeModeIsConfigurableAndDrivesTheBadge()
    {
        var ui = Path.Combine(AppContext.BaseDirectory, "plasmoid", "contents", "ui");
        var config = File.ReadAllText(Path.Combine(ui, "ConfigGeneral.qml"));
        var main = File.ReadAllText(Path.Combine(ui, "main.qml"));
        var full = File.ReadAllText(Path.Combine(ui, "FullRepresentation.qml"));

        Assert.Contains("property alias cfg_badgeMode: badgeMode.currentIndex", config, StringComparison.Ordinal);
        Assert.Contains("property int cfg_badgeModeDefault: 0", config, StringComparison.Ordinal);
        // The five choices follow the badgeMode values 0-4 in main.xml.
        Assert.Contains("[i18n(\"Most-used limit\"), i18n(\"Average of shown limits\"),", config, StringComparison.Ordinal);
        Assert.Contains("readonly property var popupBadge: calculateBadge()", main, StringComparison.Ordinal);
        Assert.Contains("Plasmoid.configuration.badgeMode", main, StringComparison.Ordinal);
        Assert.Contains("visible: controller.popupBadge !== null", full, StringComparison.Ordinal);
        Assert.DoesNotContain("showLowestRemaining", config + main + full, StringComparison.Ordinal);
    }
}
