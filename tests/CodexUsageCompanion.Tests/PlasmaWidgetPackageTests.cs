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
    }
}
