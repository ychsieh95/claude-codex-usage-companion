// SPDX-License-Identifier: MIT

import QtQuick
import QtQuick.Controls as QQC2
import QtQuick.Layouts
import org.kde.kcmutils as KCM
import org.kde.kirigami as Kirigami

KCM.SimpleKCM {
    id: page

    property alias cfg_compactViewStyle: compactViewStyle.currentIndex
    property alias cfg_refreshIntervalMinutes: refreshInterval.value
    property alias cfg_showClaudeSession: showClaudeSession.checked
    property alias cfg_showClaudeWeekly: showClaudeWeekly.checked
    property alias cfg_showCodexFiveHour: showCodexFiveHour.checked
    property alias cfg_showCodexWeekly: showCodexWeekly.checked
    property alias cfg_showPacePointer: showPacePointer.checked
    property int cfg_compactViewStyleDefault: 0
    property int cfg_refreshIntervalMinutesDefault: 1
    property bool cfg_showClaudeSessionDefault: true
    property bool cfg_showClaudeWeeklyDefault: true
    property bool cfg_showCodexFiveHourDefault: true
    property bool cfg_showCodexWeeklyDefault: true
    property bool cfg_showPacePointerDefault: true

    Kirigami.FormLayout {
        QQC2.ComboBox {
            id: compactViewStyle
            model: [i18n("Circles"), i18n("Bars")]
            Kirigami.FormData.label: i18n("Panel view:")
        }

        QQC2.SpinBox {
            id: refreshInterval
            from: 1
            to: 60
            editable: true
            Kirigami.FormData.label: i18n("Refresh interval:")
            textFromValue: function(value, locale) {
                return i18np("%1 minute", "%1 minutes", value);
            }
            valueFromText: function(text, locale) {
                const parsed = parseInt(text, 10);
                return isNaN(parsed) ? 1 : parsed;
            }
        }

        Kirigami.Heading {
            text: i18n("Displayed limits")
            level: 3
            Kirigami.FormData.isSection: true
        }

        QQC2.CheckBox {
            id: showClaudeSession
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
            text: i18n("Providers disabled in the companion are omitted. Unavailable limits display --.")
                + "\n"
                + i18n("Selections apply to both the panel and the expanded popup.")
            wrapMode: Text.WordWrap
            opacity: 0.7
        }

        Kirigami.Heading {
            text: i18n("Usage bars")
            level: 3
            Kirigami.FormData.isSection: true
        }

        QQC2.CheckBox {
            id: showPacePointer
            text: i18n("Show even-pace pointer on bars")
        }
    }
}
