// SPDX-License-Identifier: MIT

import QtQuick
import QtQuick.Layouts
import org.kde.kirigami as Kirigami
import org.kde.plasma.components as PlasmaComponents3

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
    property bool showPacePointer: true

    implicitHeight: sectionContent.implicitHeight + Kirigami.Units.largeSpacing * 2
    radius: Kirigami.Units.cornerRadius
    color: Kirigami.Theme.alternateBackgroundColor
    border.width: 1
    border.color: Qt.rgba(
        Kirigami.Theme.textColor.r,
        Kirigami.Theme.textColor.g,
        Kirigami.Theme.textColor.b,
        0.12)

    ColumnLayout {
        id: sectionContent
        anchors.fill: parent
        anchors.margins: Kirigami.Units.largeSpacing
        spacing: Kirigami.Units.smallSpacing

        RowLayout {
            Layout.fillWidth: true

            Image {
                source: section.providerIcon
                implicitWidth: Kirigami.Units.iconSizes.smallMedium
                implicitHeight: Kirigami.Units.iconSizes.smallMedium
                fillMode: Image.PreserveAspectFit
                smooth: true
                mipmap: true
            }

            PlasmaComponents3.Label {
                Layout.fillWidth: true
                text: section.providerName
                font.bold: true
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
            showPacePointer: section.showPacePointer
        }

        UsageWindow {
            Layout.fillWidth: true
            visible: section.showWeekly
            title: section.weeklyTitle
            windowState: section.state ? section.state.weekly : null
            colorForPercent: section.colorForPercent
            showPacePointer: section.showPacePointer
        }

        PlasmaComponents3.Label {
            Layout.fillWidth: true
            visible: text.length > 0
            text: section.supplementalText()
            wrapMode: Text.Wrap
            opacity: 0.75
            font.pointSize: Kirigami.Theme.smallFont.pointSize
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
                return i18n("Usage credits: %1 / %2%3 · Auto-reload: %4",
                            Number(extra.usedAmount).toFixed(2),
                            Number(extra.limitAmount).toFixed(2),
                            currency,
                            extra.enabled ? i18n("Enabled") : i18n("Disabled"));
            }
        }

        if (!isClaude) {
            const details = [];
            if (state.creditBalance) {
                details.push(i18n("Credits: %1 · Automatic reload: %2",
                                  state.creditBalance,
                                  state.automaticReloadEnabled ? i18n("Enabled") : i18n("Disabled")));
            }
            if (state.availableResetCredits !== null && state.availableResetCredits !== undefined) {
                details.push(i18n("Reset credits: %1", state.availableResetCredits));
            }
            return details.join(" · ");
        }

        return "";
    }
}
