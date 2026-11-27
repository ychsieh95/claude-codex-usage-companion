// SPDX-License-Identifier: MIT

import QtQuick
import QtQuick.Layouts
import org.kde.kirigami as Kirigami
import org.kde.plasma.components as PlasmaComponents3
import org.kde.plasma.plasmoid

Item {
    id: full
    required property var controller

    Layout.minimumWidth: Kirigami.Units.gridUnit * 18
    Layout.preferredWidth: Kirigami.Units.gridUnit * 22
    Layout.minimumHeight: Kirigami.Units.gridUnit * 12
    Layout.preferredHeight: content.implicitHeight + Kirigami.Units.largeSpacing * 2

    ColumnLayout {
        id: content
        anchors.fill: parent
        anchors.margins: Kirigami.Units.largeSpacing
        spacing: Kirigami.Units.largeSpacing

        RowLayout {
            Layout.fillWidth: true
            spacing: Kirigami.Units.smallSpacing

            Kirigami.Icon {
                source: "claude-codex-usage-companion"
                implicitWidth: Kirigami.Units.iconSizes.medium
                implicitHeight: Kirigami.Units.iconSizes.medium
            }

            ColumnLayout {
                Layout.fillWidth: true
                spacing: 0

                PlasmaComponents3.Label {
                    Layout.fillWidth: true
                    text: i18n("Claude Codex Usage")
                    font.bold: true
                    font.pointSize: Kirigami.Theme.defaultFont.pointSize * 1.15
                }

                PlasmaComponents3.Label {
                    Layout.fillWidth: true
                    text: full.lastUpdatedText()
                    opacity: 0.7
                    font.pointSize: Kirigami.Theme.smallFont.pointSize
                }
            }

            PlasmaComponents3.ToolButton {
                icon.name: "view-refresh"
                text: i18n("Refresh")
                display: PlasmaComponents3.AbstractButton.IconOnly
                enabled: !controller.refreshing
                onClicked: controller.refresh()

                PlasmaComponents3.ToolTip {
                    text: parent.text
                }
            }

            PlasmaComponents3.ToolButton {
                icon.name: "open-menu-symbolic"
                text: i18n("Open companion")
                display: PlasmaComponents3.AbstractButton.IconOnly
                onClicked: controller.openCompanion()

                PlasmaComponents3.ToolTip {
                    text: parent.text
                }
            }
        }

        PlasmaComponents3.BusyIndicator {
            Layout.alignment: Qt.AlignHCenter
            running: controller.refreshing && !controller.hasProviders
            visible: running
        }

        ClassicInlineMessage {
            Layout.fillWidth: true
            visible: controller.generalError.length > 0
            text: controller.generalError
            iconName: "dialog-error-symbolic"
            accentColor: Kirigami.Theme.negativeTextColor
        }

        ClassicInlineMessage {
            Layout.fillWidth: true
            visible: !controller.refreshing &&
                     controller.generalError.length === 0 &&
                     !controller.hasProviders
            text: i18n("No usage providers are enabled. Enable Claude or Codex in the companion settings.")
            iconName: "dialog-information-symbolic"
            accentColor: Kirigami.Theme.textColor
        }

        ClassicInlineMessage {
            Layout.fillWidth: true
            visible: !controller.refreshing &&
                     controller.generalError.length === 0 &&
                     controller.hasProviders &&
                     !controller.anyLimitSelected
            text: i18n("No usage limits are selected. Choose at least one in the widget settings.")
            iconName: "dialog-information-symbolic"
            accentColor: Kirigami.Theme.textColor
        }

        ClassicProviderSection {
            Layout.fillWidth: true
            visible: controller.claudePresent &&
                     (Plasmoid.configuration.showClaudeSession ||
                      Plasmoid.configuration.showClaudeWeekly)
            providerName: i18n("Claude")
            providerIcon: Qt.resolvedUrl("../images/claude.svg")
            fiveHourTitle: i18n("Current session")
            weeklyTitle: i18n("Current week (All)")
            state: controller.claudeState
            errorMessage: controller.claudeError
            isClaude: true
            showFiveHour: Plasmoid.configuration.showClaudeSession
            showWeekly: Plasmoid.configuration.showClaudeWeekly
            colorForPercent: controller.classicColorForPercent
            showPacePointer: Plasmoid.configuration.showPacePointer
        }

        ClassicProviderSection {
            Layout.fillWidth: true
            visible: controller.codexPresent &&
                     (Plasmoid.configuration.showCodexFiveHour ||
                      Plasmoid.configuration.showCodexWeekly)
            providerName: i18n("Codex")
            providerIcon: Qt.resolvedUrl("../images/codex.svg")
            fiveHourTitle: i18n("5-hour limit")
            weeklyTitle: i18n("Weekly limit")
            state: controller.codexState
            errorMessage: controller.codexError
            isClaude: false
            showFiveHour: Plasmoid.configuration.showCodexFiveHour
            showWeekly: Plasmoid.configuration.showCodexWeekly
            colorForPercent: controller.classicColorForPercent
            showPacePointer: Plasmoid.configuration.showPacePointer
        }

        Item {
            Layout.fillHeight: true
            visible: content.height > content.implicitHeight
        }
    }

    function lastUpdatedText() {
        if (controller.refreshing) {
            return i18n("Refreshing…");
        }
        if (isNaN(controller.lastUpdated.getTime())) {
            return i18n("Not updated yet")
        }
        return i18n("Updated %1", controller.lastUpdated.toLocaleTimeString(Qt.locale(), Locale.ShortFormat));
    }
}
