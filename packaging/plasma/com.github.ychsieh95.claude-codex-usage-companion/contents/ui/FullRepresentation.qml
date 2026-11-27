// SPDX-License-Identifier: MIT

import QtQuick
import QtQuick.Layouts
import org.kde.kirigami as Kirigami
import org.kde.plasma.components as PlasmaComponents3
import org.kde.plasma.plasmoid

Item {
    id: full
    required property var controller

    Layout.minimumWidth: Kirigami.Units.gridUnit * 19
    Layout.preferredWidth: Kirigami.Units.gridUnit * 23
    Layout.minimumHeight: Kirigami.Units.gridUnit * 16
    Layout.preferredHeight: Math.min(Kirigami.Units.gridUnit * 34,
                                     page.implicitHeight + Kirigami.Units.largeSpacing * 2)

    PlasmaComponents3.ScrollView {
        id: scroller
        anchors.fill: parent
        clip: true
        PlasmaComponents3.ScrollBar.horizontal.policy: PlasmaComponents3.ScrollBar.AlwaysOff

        ColumnLayout {
            id: page
            // availableWidth excludes the vertical scroll bar, so content never sits under it.
            width: scroller.availableWidth
            spacing: Kirigami.Units.largeSpacing

            RowLayout {
                Layout.fillWidth: true
                Layout.leftMargin: Kirigami.Units.largeSpacing
                Layout.rightMargin: Kirigami.Units.largeSpacing
                Layout.topMargin: Kirigami.Units.largeSpacing
                spacing: Kirigami.Units.smallSpacing

                ColumnLayout {
                    Layout.fillWidth: true
                    spacing: 0
                    Kirigami.Heading {
                        Layout.fillWidth: true
                        text: i18n("Usage overview")
                        level: 2
                        elide: Text.ElideRight
                    }
                    PlasmaComponents3.Label {
                        Layout.fillWidth: true
                        text: full.lastUpdatedText()
                        color: Kirigami.Theme.disabledTextColor
                        font: Kirigami.Theme.smallFont
                    }
                }

                Rectangle {
                    id: badgeChip
                    visible: controller.popupBadge !== null
                    implicitWidth: summaryRow.implicitWidth + Kirigami.Units.largeSpacing
                    implicitHeight: Kirigami.Units.iconSizes.medium
                    radius: height / 2
                    readonly property color badgeColor: controller.popupBadge !== null
                        ? controller.colorForPercent(controller.popupBadge.percent)
                        : Kirigami.Theme.disabledTextColor
                    color: full.alpha(badgeColor, 0.12)
                    RowLayout {
                        id: summaryRow
                        anchors.centerIn: parent
                        spacing: 2
                        PlasmaComponents3.Label {
                            text: controller.popupBadge !== null
                                ? controller.displayPercent(controller.popupBadge.percent).toString() : ""
                            color: badgeChip.badgeColor
                            font.weight: Font.DemiBold
                        }
                        PlasmaComponents3.Label {
                            text: "%"
                            color: badgeChip.badgeColor
                            font: Kirigami.Theme.smallFont
                        }
                    }
                    HoverHandler {
                        id: summaryHover
                    }
                    PlasmaComponents3.ToolTip {
                        visible: summaryHover.hovered
                        text: controller.popupBadge !== null ? controller.popupBadge.text : ""
                    }
                }

                PlasmaComponents3.ToolButton {
                    icon.name: "view-refresh"
                    text: i18n("Refresh")
                    display: PlasmaComponents3.AbstractButton.IconOnly
                    enabled: !controller.refreshing
                    onClicked: controller.refresh()
                    PlasmaComponents3.ToolTip { text: parent.text }
                }
                PlasmaComponents3.ToolButton {
                    icon.name: "window-new-symbolic"
                    text: i18n("Open companion")
                    display: PlasmaComponents3.AbstractButton.IconOnly
                    onClicked: controller.openCompanion()
                    PlasmaComponents3.ToolTip { text: parent.text }
                }
            }

            Rectangle {
                Layout.fillWidth: true
                implicitHeight: 1
                color: full.alpha(Kirigami.Theme.textColor, 0.10)
            }

            PlasmaComponents3.BusyIndicator {
                Layout.alignment: Qt.AlignHCenter
                running: controller.refreshing && !controller.hasProviders
                visible: running
            }

            InlineMessage {
                Layout.fillWidth: true
                Layout.leftMargin: Kirigami.Units.largeSpacing
                Layout.rightMargin: Kirigami.Units.largeSpacing
                visible: controller.generalError.length > 0
                text: controller.generalError
                iconName: "dialog-error-symbolic"
                accentColor: Kirigami.Theme.negativeTextColor
            }
            InlineMessage {
                Layout.fillWidth: true
                Layout.leftMargin: Kirigami.Units.largeSpacing
                Layout.rightMargin: Kirigami.Units.largeSpacing
                visible: !controller.refreshing && controller.generalError.length === 0 &&
                         !controller.hasProviders
                text: i18n("No usage providers are enabled. Enable Claude or Codex in the companion settings.")
                iconName: "dialog-information-symbolic"
                accentColor: Kirigami.Theme.textColor
            }
            InlineMessage {
                Layout.fillWidth: true
                Layout.leftMargin: Kirigami.Units.largeSpacing
                Layout.rightMargin: Kirigami.Units.largeSpacing
                visible: !controller.refreshing && controller.generalError.length === 0 &&
                         controller.hasProviders && !controller.anyLimitSelected
                text: i18n("No usage limits are selected. Choose at least one in the widget settings.")
                iconName: "dialog-information-symbolic"
                accentColor: Kirigami.Theme.textColor
            }

            ProviderSection {
                Layout.fillWidth: true
                Layout.leftMargin: Kirigami.Units.largeSpacing
                Layout.rightMargin: Kirigami.Units.largeSpacing
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
                colorForPercent: controller.colorForPercent
                controller: full.controller
                showPacePointer: Plasmoid.configuration.showPacePointer
            }

            ProviderSection {
                Layout.fillWidth: true
                Layout.leftMargin: Kirigami.Units.largeSpacing
                Layout.rightMargin: Kirigami.Units.largeSpacing
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
                colorForPercent: controller.colorForPercent
                controller: full.controller
                showPacePointer: Plasmoid.configuration.showPacePointer
            }

            Item { Layout.preferredHeight: Kirigami.Units.smallSpacing }
        }
    }

    function alpha(color, opacity) { return Qt.rgba(color.r, color.g, color.b, opacity); }
    function lastUpdatedText() {
        return controller.lastUpdatedText();
    }
}
