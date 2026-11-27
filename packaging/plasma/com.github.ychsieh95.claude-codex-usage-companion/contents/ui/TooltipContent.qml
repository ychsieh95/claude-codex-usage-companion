// SPDX-License-Identifier: MIT

// Panel tooltip laid out as a table. Plasma's default tooltip wraps long lines
// and shows at most 8 lines, which cut off the last update time.

import QtQuick
import QtQuick.Layouts
import org.kde.kirigami as Kirigami
import org.kde.plasma.components as PlasmaComponents3
import org.kde.plasma.plasmoid

Item {
    id: tip
    required property var controller

    readonly property bool showReset: Plasmoid.configuration.tooltipShowResetTime
    readonly property var entries: controller.compactEntries
    readonly property string statusText: controller.tooltipStatusText()

    implicitWidth: content.implicitWidth + Kirigami.Units.largeSpacing * 2
    implicitHeight: content.implicitHeight + Kirigami.Units.largeSpacing * 2
    Layout.minimumWidth: implicitWidth
    Layout.minimumHeight: implicitHeight
    Layout.maximumWidth: implicitWidth
    Layout.maximumHeight: implicitHeight

    ColumnLayout {
        id: content
        anchors.left: parent.left
        anchors.top: parent.top
        anchors.margins: Kirigami.Units.largeSpacing
        spacing: Kirigami.Units.smallSpacing

        Kirigami.Heading {
            level: 3
            text: i18n("Claude Codex Usage")
        }

        PlasmaComponents3.Label {
            visible: tip.statusText.length > 0
            text: tip.statusText
        }

        PlasmaComponents3.Label {
            visible: tip.statusText.length === 0 &&
                     Plasmoid.configuration.tooltipShowTightest &&
                     tip.entries.length > 1 && tip.controller.lowestRemainingTitle.length > 0
            text: tip.controller.tightestLimitText()
            font.weight: Font.DemiBold
        }

        GridLayout {
            visible: tip.statusText.length === 0
            columns: tip.showReset ? 3 : 2
            columnSpacing: Kirigami.Units.largeSpacing
            rowSpacing: Kirigami.Units.smallSpacing / 2

            Repeater {
                model: tip.statusText.length === 0 ? tip.entries : []
                delegate: PlasmaComponents3.Label {
                    required property var modelData
                    Layout.row: index
                    Layout.column: 0
                    required property int index
                    text: modelData.title
                }
            }
            Repeater {
                model: tip.statusText.length === 0 ? tip.entries : []
                delegate: PlasmaComponents3.Label {
                    required property var modelData
                    required property int index
                    Layout.row: index
                    Layout.column: 1
                    Layout.alignment: Qt.AlignRight
                    text: tip.valueText(modelData.percent)
                    color: tip.controller.colorForPercent(modelData.percent)
                    font.weight: Font.DemiBold
                }
            }
            Repeater {
                model: tip.statusText.length === 0 && tip.showReset ? tip.entries : []
                delegate: PlasmaComponents3.Label {
                    required property var modelData
                    required property int index
                    Layout.row: index
                    Layout.column: 2
                    visible: modelData.percent >= 0
                    text: tip.controller.resetDescription(modelData.resetsAt,
                                                          tip.controller.nowSeconds, true)
                    color: Kirigami.Theme.disabledTextColor
                }
            }
        }

        PlasmaComponents3.Label {
            visible: Plasmoid.configuration.tooltipShowLastUpdated
            Layout.topMargin: Kirigami.Units.smallSpacing
            text: tip.controller.lastUpdatedText()
            color: Kirigami.Theme.disabledTextColor
            font: Kirigami.Theme.smallFont
        }
    }

    function valueText(percent) {
        if (percent < 0) {
            return i18n("unavailable");
        }
        return controller.showUsed
            ? i18n("%1% used", controller.displayPercent(percent))
            : i18n("%1% left", percent);
    }
}
