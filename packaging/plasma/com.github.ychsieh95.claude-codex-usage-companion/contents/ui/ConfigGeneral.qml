// SPDX-License-Identifier: MIT

import QtQuick
import QtQuick.Controls as QQC2
import QtQuick.Layouts
import org.kde.kcmutils as KCM
import org.kde.kirigami as Kirigami

KCM.SimpleKCM {
    id: page

    property alias cfg_designStyle: designStyle.currentIndex
    property alias cfg_compactViewStyle: compactViewStyle.currentIndex
    property alias cfg_refreshIntervalMinutes: refreshInterval.value
    property alias cfg_percentMode: percentMode.currentIndex
    property alias cfg_clickAction: clickAction.currentIndex
    property alias cfg_compactShowTightestOnly: compactShowTightestOnly.checked
    property alias cfg_compactShowLabels: compactShowLabels.checked
    property alias cfg_warningThreshold: warningThreshold.value
    property alias cfg_criticalThreshold: criticalThreshold.value
    property alias cfg_resetTimeFormat: resetTimeFormat.currentIndex
    property alias cfg_clockFormat: clockFormat.currentIndex
    property alias cfg_showCredits: showCredits.checked
    property alias cfg_tooltipShowTightest: tooltipShowTightest.checked
    property alias cfg_tooltipShowResetTime: tooltipShowResetTime.checked
    property alias cfg_tooltipShowLastUpdated: tooltipShowLastUpdated.checked
    property alias cfg_badgeMode: badgeMode.currentIndex
    property alias cfg_showClaudeSession: showClaudeSession.checked
    property alias cfg_showClaudeWeekly: showClaudeWeekly.checked
    property alias cfg_showCodexFiveHour: showCodexFiveHour.checked
    property alias cfg_showCodexWeekly: showCodexWeekly.checked
    property alias cfg_showPacePointer: showPacePointer.checked
    property int cfg_designStyleDefault: 0
    property int cfg_compactViewStyleDefault: 0
    property int cfg_refreshIntervalMinutesDefault: 1
    property int cfg_percentModeDefault: 0
    property int cfg_clickActionDefault: 0
    property bool cfg_compactShowTightestOnlyDefault: false
    property bool cfg_compactShowLabelsDefault: true
    property int cfg_warningThresholdDefault: 70
    property int cfg_criticalThresholdDefault: 40
    property int cfg_resetTimeFormatDefault: 0
    property int cfg_clockFormatDefault: 0
    property bool cfg_showCreditsDefault: true
    property bool cfg_tooltipShowTightestDefault: true
    property bool cfg_tooltipShowResetTimeDefault: true
    property bool cfg_tooltipShowLastUpdatedDefault: true
    property int cfg_badgeModeDefault: 0
    property bool cfg_showClaudeSessionDefault: true
    property bool cfg_showClaudeWeeklyDefault: true
    property bool cfg_showCodexFiveHourDefault: true
    property bool cfg_showCodexWeeklyDefault: true
    property bool cfg_showPacePointerDefault: true

    Kirigami.FormLayout {
        anchors.left: parent.left
        anchors.right: parent.right

        QQC2.ComboBox {
            id: designStyle
            model: [i18n("Latest"), i18n("Previous")]
            Kirigami.FormData.label: i18n("Design:")
        }

        QQC2.ComboBox {
            id: compactViewStyle
            model: [i18n("Radial meters"), i18n("Meter strips")]
            Kirigami.FormData.label: i18n("Style:")
        }

        QQC2.SpinBox {
            id: refreshInterval
            from: 1
            to: 60
            editable: true
            Kirigami.FormData.label: i18n("Update every:")
            textFromValue: function(value, locale) {
                return i18np("%1 minute", "%1 minutes", value);
            }
            valueFromText: function(text, locale) {
                const parsed = parseInt(text, 10);
                return isNaN(parsed) ? 1 : parsed;
            }
        }

        QQC2.ComboBox {
            id: percentMode
            enabled: designStyle.currentIndex === 0
            model: [i18n("Remaining"), i18n("Used")]
            Kirigami.FormData.label: i18n("Percentages:")
        }

        QQC2.ComboBox {
            id: clickAction
            model: [i18n("Open the popup"), i18n("Open the companion app")]
            Kirigami.FormData.label: i18n("On click:")
        }

        Item {
            Kirigami.FormData.isSection: true
        }

        QQC2.CheckBox {
            id: compactShowTightestOnly
            enabled: designStyle.currentIndex === 0
            Kirigami.FormData.label: i18n("Panel:")
            text: percentMode.currentIndex === 1
                ? i18n("Show only the highest used limit")
                : i18n("Show only the lowest remaining limit")
        }

        QQC2.CheckBox {
            id: compactShowLabels
            enabled: designStyle.currentIndex === 0
            text: i18n("Show limit labels")
        }

        Item {
            Kirigami.FormData.isSection: true
        }

        QQC2.SpinBox {
            id: warningThreshold
            enabled: designStyle.currentIndex === 0
            from: 1
            to: 100
            Kirigami.FormData.label: i18n("Warn below:")
            textFromValue: function(value, locale) { return i18n("%1% left", value); }
            valueFromText: function(text, locale) {
                const parsed = parseInt(text, 10);
                return isNaN(parsed) ? 70 : parsed;
            }
        }

        QQC2.SpinBox {
            id: criticalThreshold
            enabled: designStyle.currentIndex === 0
            from: 1
            to: warningThreshold.value
            Kirigami.FormData.label: i18n("Critical below:")
            textFromValue: function(value, locale) { return i18n("%1% left", value); }
            valueFromText: function(text, locale) {
                const parsed = parseInt(text, 10);
                return isNaN(parsed) ? 40 : parsed;
            }
        }

        Item {
            Kirigami.FormData.isSection: true
        }

        QQC2.ComboBox {
            id: resetTimeFormat
            model: [i18n("Countdown and time"), i18n("Countdown"), i18n("Time")]
            Kirigami.FormData.label: i18n("Reset time:")
        }

        QQC2.ComboBox {
            id: clockFormat
            model: [i18n("System default"), i18n("12-hour"), i18n("24-hour")]
            Kirigami.FormData.label: i18n("Clock:")
        }

        Item {
            Kirigami.FormData.isSection: true
        }

        QQC2.CheckBox {
            id: tooltipShowTightest
            Kirigami.FormData.label: i18n("Tooltip:")
            text: percentMode.currentIndex === 1
                ? i18n("Show highest used limit")
                : i18n("Show lowest remaining limit")
        }

        QQC2.CheckBox {
            id: tooltipShowResetTime
            text: i18n("Show reset times")
        }

        QQC2.CheckBox {
            id: tooltipShowLastUpdated
            text: i18n("Show last update time")
        }

        Item {
            Kirigami.FormData.isSection: true
        }

        QQC2.CheckBox {
            id: showClaudeSession
            Kirigami.FormData.label: i18n("Limits:")
            text: i18n("Claude current session")
        }

        QQC2.CheckBox {
            id: showClaudeWeekly
            text: i18n("Claude week (All)")
        }

        QQC2.CheckBox {
            id: showCodexFiveHour
            text: i18n("Codex 5-hour limit")
        }

        QQC2.CheckBox {
            id: showCodexWeekly
            text: i18n("Codex weekly limit")
        }

        QQC2.Label {
            Layout.fillWidth: true
            Layout.preferredWidth: Math.max(
                Kirigami.Units.gridUnit * 16,
                page.width - Kirigami.Units.gridUnit * 6)
            Layout.maximumWidth: Layout.preferredWidth
            text: i18n("Providers disabled in the companion are omitted. Unavailable limits display —.")
                + "\n"
                + i18n("Selections apply to both the panel and the expanded popup.")
            wrapMode: Text.WordWrap
            color: Kirigami.Theme.disabledTextColor
            font: Kirigami.Theme.smallFont
        }

        Item {
            Kirigami.FormData.isSection: true
        }

        QQC2.ComboBox {
            id: badgeMode
            enabled: designStyle.currentIndex === 0
            Kirigami.FormData.label: i18n("Popup badge:")
            // A fixed model: replacing it when Percentages changes would reset the selection.
            // "Most-used" names the same limit whether percentages show remaining or used.
            model: [i18n("Most-used limit"), i18n("Average of shown limits"),
                    i18n("Claude's most-used limit"), i18n("Codex's most-used limit"), i18n("Hidden")]
        }

        QQC2.CheckBox {
            id: showCredits
            enabled: designStyle.currentIndex === 0
            Kirigami.FormData.label: i18n("Popup:")
            text: i18n("Show credits")
        }

        QQC2.CheckBox {
            id: showPacePointer
            text: i18n("Show even-pace marker")
        }

        QQC2.Label {
            Layout.fillWidth: true
            Layout.preferredWidth: Math.max(
                Kirigami.Units.gridUnit * 16,
                page.width - Kirigami.Units.gridUnit * 6)
            Layout.maximumWidth: Layout.preferredWidth
            text: percentMode.currentIndex === 1
                ? i18n("The marker shows how much should be used if usage is spread evenly until reset.")
                : i18n("The marker shows how much should remain if usage is spread evenly until reset.")
            wrapMode: Text.WordWrap
            color: Kirigami.Theme.disabledTextColor
            font: Kirigami.Theme.smallFont
        }
    }
}
