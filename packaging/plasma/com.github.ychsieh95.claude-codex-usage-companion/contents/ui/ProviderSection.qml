// SPDX-License-Identifier: MIT

import QtQuick
import QtQuick.Layouts
import org.kde.kirigami as Kirigami
import org.kde.plasma.components as PlasmaComponents3
import org.kde.plasma.plasmoid

Rectangle {
    id: section
    required property string providerName
    required property url providerIcon
    required property string fiveHourTitle
    required property string weeklyTitle
    required property var state
    required property string errorMessage
    required property bool isClaude
    required property bool showFiveHour
    required property bool showWeekly
    required property var colorForPercent
    required property var controller
    property bool showPacePointer: true

    readonly property color brandColor: isClaude ? "#D77655" : "#10A37F"

    implicitHeight: sectionContent.implicitHeight + Kirigami.Units.largeSpacing * 2
    radius: Kirigami.Units.cornerRadius
    color: Kirigami.Theme.backgroundColor
    border.width: 1
    border.color: Qt.rgba(Kirigami.Theme.textColor.r, Kirigami.Theme.textColor.g,
                          Kirigami.Theme.textColor.b, 0.12)

    Rectangle {
        anchors.left: parent.left
        anchors.top: parent.top
        anchors.bottom: parent.bottom
        width: 3
        radius: width / 2
        color: section.brandColor
    }

    ColumnLayout {
        id: sectionContent
        anchors.fill: parent
        anchors.margins: Kirigami.Units.largeSpacing
        spacing: Kirigami.Units.largeSpacing

        RowLayout {
            Layout.fillWidth: true
            spacing: Kirigami.Units.smallSpacing
            Rectangle {
                implicitWidth: Kirigami.Units.iconSizes.medium
                implicitHeight: Kirigami.Units.iconSizes.medium
                radius: Kirigami.Units.cornerRadius
                color: Qt.rgba(section.brandColor.r, section.brandColor.g, section.brandColor.b, 0.12)
                Image {
                    anchors.centerIn: parent
                    source: section.providerIcon
                    width: Kirigami.Units.iconSizes.smallMedium
                    height: Kirigami.Units.iconSizes.smallMedium
                    fillMode: Image.PreserveAspectFit
                    smooth: true
                    mipmap: true
                }
            }
            PlasmaComponents3.Label {
                Layout.fillWidth: true
                text: section.providerName
                font.weight: Font.DemiBold
                font.pixelSize: Kirigami.Theme.defaultFont.pixelSize * 1.1
            }
            PlasmaComponents3.Label {
                text: Plasmoid.configuration.percentMode === 1 ? i18n("USED") : i18n("REMAINING")
                color: Kirigami.Theme.disabledTextColor
                font.pointSize: Kirigami.Theme.smallFont.pointSize
                font.capitalization: Font.AllUppercase
                font.letterSpacing: 0.6
            }
        }

        InlineMessage {
            Layout.fillWidth: true
            visible: section.errorMessage.length > 0
            text: section.errorMessage
            iconName: "dialog-warning-symbolic"
            accentColor: Kirigami.Theme.neutralTextColor
        }

        UsageWindow {
            Layout.fillWidth: true
            visible: section.showFiveHour
            title: section.fiveHourTitle
            windowState: section.state ? section.state.fiveHour : null
            colorForPercent: section.colorForPercent
            controller: section.controller
            showPacePointer: section.showPacePointer
        }

        Rectangle {
            Layout.fillWidth: true
            implicitHeight: 1
            visible: section.showFiveHour && section.showWeekly
            color: Qt.rgba(Kirigami.Theme.textColor.r, Kirigami.Theme.textColor.g,
                           Kirigami.Theme.textColor.b, 0.10)
        }

        UsageWindow {
            Layout.fillWidth: true
            visible: section.showWeekly
            title: section.weeklyTitle
            windowState: section.state ? section.state.weekly : null
            colorForPercent: section.colorForPercent
            controller: section.controller
            showPacePointer: section.showPacePointer
        }

        PlasmaComponents3.Label {
            Layout.fillWidth: true
            visible: Plasmoid.configuration.showCredits && text.length > 0
            text: section.supplementalText()
            wrapMode: Text.Wrap
            color: Kirigami.Theme.disabledTextColor
            font: Kirigami.Theme.smallFont
        }
    }

    function supplementalText() {
        if (!state) {
            return "";
        }
        if (isClaude && state.extraUsage) {
            const extra = state.extraUsage;
            if (extra.usedAmount !== null && extra.usedAmount !== undefined &&
                    extra.limitAmount !== null && extra.limitAmount !== undefined) {
                const currency = extra.currency ? " " + extra.currency : "";
                return i18n("Usage credits %1 of %2%3 · Auto-reload %4",
                            Number(extra.usedAmount).toFixed(2),
                            Number(extra.limitAmount).toFixed(2), currency,
                            extra.enabled ? i18n("on") : i18n("off"));
            }
        }
        if (!isClaude) {
            const details = [];
            if (state.creditBalance) {
                details.push(i18n("Credits %1 · Auto-reload %2", state.creditBalance,
                                  state.automaticReloadEnabled ? i18n("on") : i18n("off")));
            }
            if (state.availableResetCredits !== null && state.availableResetCredits !== undefined) {
                details.push(i18n("%1 reset credits", state.availableResetCredits));
            }
            return details.join(" · ");
        }
        return "";
    }
}
