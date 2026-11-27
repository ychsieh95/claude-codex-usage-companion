// SPDX-License-Identifier: MIT

import QtQuick
import org.kde.kirigami as Kirigami
import org.kde.plasma.plasma5support as Plasma5Support
import org.kde.plasma.plasmoid

PlasmoidItem {
    id: root

    readonly property string statusCommand: "claude-codex-usage-companion status --json"
    readonly property string guiCommand: "claude-codex-usage-companion gui"
    property var claudeState: null
    property string claudeError: ""
    property bool claudePresent: false
    property var codexState: null
    property string codexError: ""
    property bool codexPresent: false
    property bool refreshing: false
    property string generalError: ""
    property date lastUpdated
    property double nowSeconds: Date.now() / 1000

    readonly property int lowestRemaining: calculateLowestRemaining()
    readonly property string lowestRemainingTitle: findLowestRemainingTitle()
    readonly property bool showUsed: Plasmoid.configuration.percentMode === 1
    readonly property string panelText: lowestRemaining < 0 ? "--" : displayPercent(lowestRemaining).toString()
    readonly property color signalColor: colorForPercent(lowestRemaining)
    readonly property bool hasProviders: claudePresent || codexPresent
    readonly property bool anyLimitSelected: Plasmoid.configuration.showClaudeSession ||
                                             Plasmoid.configuration.showClaudeWeekly ||
                                             Plasmoid.configuration.showCodexFiveHour ||
                                             Plasmoid.configuration.showCodexWeekly
    readonly property var compactEntries: buildCompactEntries()
    // The popup header badge: { percent: remaining percentage, text: tooltip }, or null.
    readonly property var popupBadge: calculateBadge()
    // What the panel draws: every selected limit, or only the tightest one.
    readonly property var panelEntries: Plasmoid.configuration.compactShowTightestOnly
        ? tightestEntries() : compactEntries

    Plasmoid.icon: "claude-codex-usage-companion"
    toolTipMainText: i18n("Claude Codex Usage")
    // Plain-text summary, kept for screen readers; the visible tooltip is TooltipContent.
    toolTipSubText: tooltipSummary()
    toolTipItem: TooltipContent { controller: root }
    switchWidth: Kirigami.Units.gridUnit * 14
    switchHeight: Kirigami.Units.gridUnit * 12

    readonly property bool classicDesign: Plasmoid.configuration.designStyle === 1

    compactRepresentation: classicDesign ? classicCompact : latestCompact
    fullRepresentation: classicDesign ? classicFull : latestFull

    Component {
        id: latestCompact
        CompactRepresentation { controller: root }
    }
    Component {
        id: latestFull
        FullRepresentation { controller: root }
    }
    Component {
        id: classicCompact
        ClassicCompactRepresentation { controller: root }
    }
    Component {
        id: classicFull
        ClassicFullRepresentation { controller: root }
    }

    Plasma5Support.DataSource {
        id: statusSource
        engine: "executable"

        onNewData: function(sourceName, data) {
            if (sourceName !== root.statusCommand) {
                return;
            }

            disconnectSource(sourceName);
            refreshTimeout.stop();
            root.refreshing = false;

            const output = data["stdout"] || "";
            if (output.trim().length === 0) {
                const detail = (data["stderr"] || "").trim();
                root.generalError = detail.length > 0
                    ? detail
                    : i18n("The companion command returned no data.");
                return;
            }

            try {
                root.applyPayload(JSON.parse(output));
                root.generalError = "";
                root.lastUpdated = new Date();
                root.nowSeconds = Date.now() / 1000;
            } catch (error) {
                root.generalError = i18n("Could not read usage data: %1", error.message);
            }
        }
    }

    Plasma5Support.DataSource {
        id: commandRunner
        engine: "executable"

        onNewData: function(sourceName, data) {
            disconnectSource(sourceName);
        }
    }

    Timer {
        id: refreshTimer
        interval: Math.max(1, Math.min(60, Plasmoid.configuration.refreshIntervalMinutes)) * 60000
        repeat: true
        running: true
        onTriggered: root.refresh()
    }

    Timer {
        // Keeps reset countdowns in the panel tooltip current.
        interval: 30000
        repeat: true
        running: Plasmoid.configuration.tooltipShowResetTime
        onTriggered: root.nowSeconds = Date.now() / 1000
    }

    Timer {
        id: refreshTimeout
        interval: 30000
        repeat: false
        onTriggered: {
            statusSource.disconnectSource(root.statusCommand);
            root.refreshing = false;
            root.generalError = i18n("Usage refresh timed out.");
        }
    }

    function refresh() {
        if (refreshing) {
            return;
        }

        refreshing = true;
        generalError = "";
        statusSource.disconnectSource(statusCommand);
        statusSource.connectSource(statusCommand);
        refreshTimeout.restart();
    }

    function openCompanion() {
        commandRunner.disconnectSource(guiCommand);
        commandRunner.connectSource(guiCommand);
    }

    function applyPayload(payload) {
        claudePresent = Object.prototype.hasOwnProperty.call(payload, "claude");
        codexPresent = Object.prototype.hasOwnProperty.call(payload, "codex");

        const claude = claudePresent && payload.claude ? payload.claude : {};
        claudeState = claude.state || null;
        claudeError = claude.error || "";

        const codex = codexPresent && payload.codex ? payload.codex : {};
        codexState = codex.state || null;
        codexError = codex.error || "";
    }

    function calculateLowestRemaining() {
        const percentages = [];
        if (claudePresent && Plasmoid.configuration.showClaudeSession) {
            appendWindowPercentage(percentages, claudeState, "fiveHour");
        }
        if (claudePresent && Plasmoid.configuration.showClaudeWeekly) {
            appendWindowPercentage(percentages, claudeState, "weekly");
        }
        if (codexPresent && Plasmoid.configuration.showCodexFiveHour) {
            appendWindowPercentage(percentages, codexState, "fiveHour");
        }
        if (codexPresent && Plasmoid.configuration.showCodexWeekly) {
            appendWindowPercentage(percentages, codexState, "weekly");
        }
        if (percentages.length === 0) {
            return -1;
        }
        return Math.min.apply(Math, percentages);
    }

    function findLowestRemainingTitle() {
        if (lowestRemaining < 0) {
            return "";
        }
        for (let index = 0; index < compactEntries.length; index++) {
            if (compactEntries[index].percent === lowestRemaining) {
                return compactEntries[index].title;
            }
        }
        return "";
    }

    function appendWindowPercentage(target, state, key) {
        if (state && state[key] && typeof state[key].remainingPercent === "number") {
            target.push(state[key].remainingPercent);
        }
    }

    // Fixed palette used by the previous (classic) design.
    function classicColorForPercent(percent) {
        if (percent <= 0) {
            return "#7f8c8d";
        }
        if (percent < 40) {
            return "#da4453";
        }
        if (percent < 60) {
            return "#f67400";
        }
        if (percent < 80) {
            return "#fdbc4b";
        }
        return "#27ae60";
    }

    function colorForPercent(percent) {
        if (percent < 0) {
            return Kirigami.Theme.disabledTextColor;
        }
        const warning = Plasmoid.configuration.warningThreshold;
        // A critical threshold above the warning one would hide the warning band.
        const critical = Math.min(Plasmoid.configuration.criticalThreshold, warning);
        if (percent < critical) {
            return Kirigami.Theme.negativeTextColor;
        }
        if (percent < warning) {
            return Kirigami.Theme.neutralTextColor;
        }
        return Kirigami.Theme.positiveTextColor;
    }

    function calculateBadge() {
        const mode = Plasmoid.configuration.badgeMode;
        if (mode === 4) {
            return null;
        }
        const candidates = compactEntries.filter(function(entry) {
            return entry.percent >= 0 &&
                (mode !== 2 || entry.provider === "claude") &&
                (mode !== 3 || entry.provider === "codex");
        });
        if (candidates.length === 0) {
            return null;
        }
        // An average of one limit is that limit, so the badge can name it.
        if (mode === 1 && candidates.length > 1) {
            let total = 0;
            for (let index = 0; index < candidates.length; index++) {
                total += candidates[index].percent;
            }
            const average = Math.round(total / candidates.length);
            return {
                "percent": average,
                "text": showUsed
                    ? i18n("Average used of %1 limits: %2%", candidates.length, displayPercent(average))
                    : i18n("Average remaining of %1 limits: %2%", candidates.length, average)
            };
        }
        let lowest = candidates[0];
        for (let index = 1; index < candidates.length; index++) {
            if (candidates[index].percent < lowest.percent) {
                lowest = candidates[index];
            }
        }
        return {
            "percent": lowest.percent,
            "text": showUsed
                ? i18n("Highest used: %1 (%2%)", lowest.title, displayPercent(lowest.percent))
                : i18n("Lowest remaining: %1 (%2%)", lowest.title, lowest.percent)
        };
    }

    function tightestEntries() {
        let lowest = null;
        for (let index = 0; index < compactEntries.length; index++) {
            const entry = compactEntries[index];
            if (entry.percent >= 0 && (lowest === null || entry.percent < lowest.percent)) {
                lowest = entry;
            }
        }
        if (lowest !== null) {
            return [lowest];
        }
        return compactEntries.length > 0 ? [compactEntries[0]] : [];
    }

    function activate() {
        if (Plasmoid.configuration.clickAction === 1) {
            openCompanion();
        } else {
            expanded = !expanded;
        }
    }

    function buildCompactEntries() {
        const entries = [];
        if (claudePresent) {
            if (Plasmoid.configuration.showClaudeSession) {
                entries.push(compactEntry(
                    "C · S",
                    i18n("Claude current session"),
                    claudeState ? claudeState.fiveHour : null,
                    "claude"));
            }
            if (Plasmoid.configuration.showClaudeWeekly) {
                entries.push(compactEntry(
                    "C · W",
                    i18n("Claude week (All)"),
                    claudeState ? claudeState.weekly : null,
                    "claude"));
            }
        }
        if (codexPresent) {
            if (Plasmoid.configuration.showCodexFiveHour) {
                entries.push(compactEntry(
                    "X · 5",
                    i18n("Codex 5-hour limit"),
                    codexState ? codexState.fiveHour : null,
                    "codex"));
            }
            if (Plasmoid.configuration.showCodexWeekly) {
                entries.push(compactEntry(
                    "X · W",
                    i18n("Codex weekly limit"),
                    codexState ? codexState.weekly : null,
                    "codex"));
            }
        }
        return entries;
    }

    function compactEntry(shortLabel, title, windowState, provider) {
        const available = windowState && typeof windowState.remainingPercent === "number";
        return {
            "shortLabel": shortLabel,
            "title": title,
            "percent": available ? windowState.remainingPercent : -1,
            "provider": provider,
            "resetsAt": windowState && windowState.resetsAt !== undefined
                ? windowState.resetsAt
                : null
        };
    }

    // A message that replaces the usage table, or "" when there is usage to show.
    function tooltipStatusText() {
        if (refreshing && !hasProviders) {
            return i18n("Refreshing usage…");
        }
        if (generalError.length > 0) {
            return generalError;
        }
        if (compactEntries.length === 0) {
            return hasProviders ? i18n("Usage data is unavailable.")
                                : i18n("No usage providers are enabled.");
        }
        return "";
    }

    function tooltipSummary() {
        if (refreshing && !hasProviders) {
            return i18n("Refreshing usage…");
        }
        if (generalError.length > 0) {
            return generalError;
        }

        const showReset = Plasmoid.configuration.tooltipShowResetTime;
        const lines = [];
        if (compactEntries.length > 0) {
            if (Plasmoid.configuration.tooltipShowTightest &&
                    compactEntries.length > 1 && lowestRemainingTitle.length > 0) {
                lines.push(tightestLimitText());
                lines.push("");
            }
            for (let index = 0; index < compactEntries.length; index++) {
                const entry = compactEntries[index];
                if (entry.percent < 0) {
                    lines.push(i18n("%1: unavailable", entry.title));
                } else if (showReset) {
                    lines.push(showUsed
                        ? i18n("%1: %2% used · %3", entry.title, displayPercent(entry.percent),
                               resetText(entry.resetsAt))
                        : i18n("%1: %2% left · %3", entry.title, entry.percent,
                               resetText(entry.resetsAt)));
                } else {
                    lines.push(showUsed
                        ? i18n("%1: %2% used", entry.title, displayPercent(entry.percent))
                        : i18n("%1: %2% remaining", entry.title, entry.percent));
                }
            }
        } else if (hasProviders) {
            lines.push(i18n("Usage data is unavailable."));
        } else {
            lines.push(i18n("No usage providers are enabled."));
        }

        if (Plasmoid.configuration.tooltipShowLastUpdated) {
            lines.push("");
            lines.push(lastUpdatedText());
        }
        return lines.join("\n");
    }

    // Percentages arrive as "remaining"; this converts them for display.
    // Colours always follow the remaining value.
    function displayPercent(remaining) {
        if (remaining < 0) {
            return -1;
        }
        return showUsed ? 100 - remaining : remaining;
    }

    function tightestLimitText() {
        if (lowestRemainingTitle.length === 0) {
            return showUsed ? i18n("Highest used limit") : i18n("Lowest remaining limit");
        }
        return showUsed
            ? i18n("Highest used: %1 (%2%)", lowestRemainingTitle, displayPercent(lowestRemaining))
            : i18n("Lowest remaining: %1 (%2%)", lowestRemainingTitle, lowestRemaining);
    }

    function lastUpdatedText() {
        if (refreshing) {
            return i18n("Refreshing usage…");
        }
        if (isNaN(lastUpdated.getTime())) {
            return i18n("Waiting for the first update");
        }
        return i18n("Last updated at %1", clockText(lastUpdated));
    }

    function resetText(resetsAt) {
        return resetDescription(resetsAt, nowSeconds, false);
    }

    // Describes a reset time in the configured format. sentenceCase gives
    // "Resets in …" for popup rows; otherwise "resets in …" for tooltip lines.
    function resetDescription(resetsAt, now, sentenceCase) {
        if (resetsAt === null || resetsAt === undefined) {
            return sentenceCase ? i18n("Reset time unavailable") : i18n("reset time unavailable");
        }
        const resetSeconds = Number(resetsAt);
        const reset = new Date(resetSeconds * 1000);
        const countdown = durationText(Math.max(0, resetSeconds - now));
        const clock = clockText(reset);
        switch (Plasmoid.configuration.resetTimeFormat) {
        case 1:
            return sentenceCase ? i18n("Resets in %1", countdown) : i18n("resets in %1", countdown);
        case 2: {
            // Without a countdown, a reset more than a day away needs its weekday.
            if (resetSeconds - now < 86400) {
                return sentenceCase ? i18n("Resets at %1", clock) : i18n("resets at %1", clock);
            }
            const day = reset.toLocaleDateString(Qt.locale(), "ddd");
            return sentenceCase ? i18n("Resets %1 %2", day, clock) : i18n("resets %1 %2", day, clock);
        }
        default:
            // The countdown already gives the day, and the tooltip has an 8-line limit,
            // so this format keeps the clock time short.
            return sentenceCase ? i18n("Resets in %1 · %2", countdown, clock)
                                : i18n("resets in %1 (%2)", countdown, clock);
        }
    }

    function clockText(date) {
        switch (Plasmoid.configuration.clockFormat) {
        case 1:
            return Qt.formatTime(date, "h:mm AP");
        case 2:
            return Qt.formatTime(date, "HH:mm");
        default:
            return date.toLocaleTimeString(Qt.locale(), Locale.ShortFormat);
        }
    }

    function durationText(seconds) {
        const minutes = Math.max(0, Math.ceil(seconds / 60));
        const days = Math.floor(minutes / 1440);
        const hours = Math.floor((minutes % 1440) / 60);
        const mins = minutes % 60;
        if (days > 0) {
            return hours > 0 ? i18n("%1d %2h", days, hours) : i18n("%1d", days);
        }
        if (hours > 0) {
            return mins > 0 ? i18n("%1h %2m", hours, mins) : i18n("%1h", hours);
        }
        return i18np("%1 minute", "%1 minutes", minutes);
    }

    Component.onCompleted: refresh()
}
