// SPDX-License-Identifier: MIT

import QtQuick
import QtQuick.Layouts
import org.kde.kirigami as Kirigami
import org.kde.plasma.components as PlasmaComponents3
import org.kde.plasma.core as PlasmaCore
import org.kde.plasma.plasmoid

MouseArea {
    id: compact
    required property var controller

    readonly property bool stripView: Plasmoid.configuration.compactViewStyle === 1
    readonly property bool vertical: Plasmoid.formFactor === PlasmaCore.Types.Vertical ||
                                     (Plasmoid.formFactor !== PlasmaCore.Types.Horizontal &&
                                      height > width + Kirigami.Units.gridUnit)
    readonly property bool showLabels: Plasmoid.configuration.compactShowLabels
    readonly property var entries: controller.panelEntries.length > 0
        ? controller.panelEntries
        : [{ "shortLabel": "", "title": i18n("Usage unavailable"), "percent": -1, "provider": "" }]
    readonly property int entryCount: Math.max(1, entries.length)
    readonly property real radialExtent: Kirigami.Units.iconSizes.medium + Kirigami.Units.smallSpacing
    readonly property real stripLength: Kirigami.Units.gridUnit * 3.7
    readonly property real cellLength: stripView ? stripLength : radialExtent
    readonly property real cellThickness: stripView ? Kirigami.Units.iconSizes.medium : radialExtent

    Layout.minimumWidth: vertical ? Kirigami.Units.iconSizes.smallMedium : cellLength * entryCount
    Layout.minimumHeight: vertical ? cellLength * entryCount : Kirigami.Units.iconSizes.smallMedium
    Layout.preferredWidth: vertical ? cellThickness : cellLength * entryCount
    Layout.preferredHeight: vertical ? cellLength * entryCount : cellThickness
    activeFocusOnTab: true
    hoverEnabled: true
    Accessible.name: controller.toolTipSubText

    onClicked: controller.activate()
    Keys.onSpacePressed: controller.activate()
    Keys.onReturnPressed: controller.activate()

    Rectangle {
        anchors.fill: parent
        radius: Kirigami.Units.cornerRadius
        color: compact.containsMouse || compact.activeFocus
            ? compact.alpha(Kirigami.Theme.highlightColor, 0.12)
            : "transparent"
        Behavior on color { ColorAnimation { duration: Kirigami.Units.shortDuration } }
    }

    GridLayout {
        anchors.fill: parent
        rows: compact.vertical ? compact.entryCount : 1
        columns: compact.vertical ? 1 : compact.entryCount
        rowSpacing: 0
        columnSpacing: 0

        Repeater {
            model: compact.entries

            delegate: Item {
                id: cell
                required property var modelData
                readonly property color meterColor: compact.controller.colorForPercent(modelData.percent)
                readonly property real fraction: modelData.percent < 0
                    ? 0 : Math.max(0, Math.min(1, compact.controller.displayPercent(modelData.percent) / 100))

                Layout.fillWidth: true
                Layout.fillHeight: true
                Layout.preferredWidth: compact.vertical ? compact.cellThickness : compact.cellLength
                Layout.preferredHeight: compact.vertical ? compact.cellLength : compact.cellThickness

                Loader {
                    anchors.fill: parent
                    sourceComponent: compact.stripView ? stripMeter : radialMeter
                }

                Component {
                    id: radialMeter
                    Item {
                        Canvas {
                            id: ring
                            anchors.centerIn: parent
                            width: Math.max(Kirigami.Units.iconSizes.small,
                                            Math.min(parent.width, parent.height) - Kirigami.Units.smallSpacing)
                            height: width
                            opacity: compact.controller.refreshing ? 0.55 : 1
                            // Threshold settings change the colour without changing the data.
                            readonly property color paintColor: cell.meterColor
                            onPaintColorChanged: requestPaint()
                            onPaint: compact.paintRing(getContext("2d"), width,
                                                       compact.controller.displayPercent(cell.modelData.percent), cell.meterColor)
                            Connections {
                                target: compact.controller
                                function onCompactEntriesChanged() { ring.requestPaint(); }
                                function onShowUsedChanged() { ring.requestPaint(); }
                            }
                        }

                        Column {
                            anchors.centerIn: parent
                            spacing: -Math.round(Kirigami.Units.smallSpacing / 2)
                            PlasmaComponents3.Label {
                                anchors.horizontalCenter: parent.horizontalCenter
                                text: compact.percentText(compact.controller.displayPercent(cell.modelData.percent))
                                font.weight: Font.DemiBold
                                font.pixelSize: Math.max(8, ring.width * 0.27)
                            }
                            PlasmaComponents3.Label {
                                anchors.horizontalCenter: parent.horizontalCenter
                                text: cell.modelData.shortLabel
                                visible: compact.showLabels && text.length > 0 &&
                                         ring.width >= Kirigami.Units.iconSizes.medium
                                color: Kirigami.Theme.disabledTextColor
                                font.pixelSize: Math.max(6, ring.width * 0.17)
                            }
                        }
                    }
                }

                Component {
                    id: stripMeter
                    Item {
                        ColumnLayout {
                            anchors.fill: parent
                            anchors.margins: Kirigami.Units.smallSpacing
                            spacing: 2
                            visible: !compact.vertical
                            RowLayout {
                                Layout.fillWidth: true
                                spacing: Kirigami.Units.smallSpacing
                                PlasmaComponents3.Label {
                                    Layout.fillWidth: true
                                    opacity: compact.showLabels ? 1 : 0
                                    text: cell.modelData.shortLabel
                                    color: Kirigami.Theme.disabledTextColor
                                    font: Kirigami.Theme.smallFont
                                }
                                PlasmaComponents3.Label {
                                    text: compact.percentText(compact.controller.displayPercent(cell.modelData.percent))
                                    font.weight: Font.DemiBold
                                }
                            }
                            Rectangle {
                                Layout.fillWidth: true
                                implicitHeight: Math.max(3, Math.round(Kirigami.Units.smallSpacing * 0.7))
                                radius: height / 2
                                color: compact.alpha(Kirigami.Theme.textColor, 0.14)
                                Rectangle {
                                    width: parent.width * cell.fraction
                                    height: parent.height
                                    radius: parent.radius
                                    color: cell.modelData.percent < 0 ? "transparent" : cell.meterColor
                                    Behavior on width { NumberAnimation { duration: Kirigami.Units.longDuration } }
                                }
                            }
                        }

                        RowLayout {
                            anchors.fill: parent
                            anchors.margins: Kirigami.Units.smallSpacing
                            spacing: Kirigami.Units.smallSpacing
                            visible: compact.vertical
                            Rectangle {
                                Layout.fillHeight: true
                                implicitWidth: Math.max(3, Math.round(Kirigami.Units.smallSpacing * 0.7))
                                radius: width / 2
                                color: compact.alpha(Kirigami.Theme.textColor, 0.14)
                                Rectangle {
                                    anchors.bottom: parent.bottom
                                    width: parent.width
                                    height: parent.height * cell.fraction
                                    radius: parent.radius
                                    color: cell.modelData.percent < 0 ? "transparent" : cell.meterColor
                                    Behavior on height { NumberAnimation { duration: Kirigami.Units.longDuration } }
                                }
                            }
                            ColumnLayout {
                                Layout.fillWidth: true
                                Layout.alignment: Qt.AlignVCenter
                                spacing: 0
                                PlasmaComponents3.Label {
                                    Layout.alignment: Qt.AlignHCenter
                                    text: compact.percentText(compact.controller.displayPercent(cell.modelData.percent))
                                    font.weight: Font.DemiBold
                                }
                                PlasmaComponents3.Label {
                                    Layout.alignment: Qt.AlignHCenter
                                    visible: compact.showLabels
                                    text: cell.modelData.shortLabel
                                    color: Kirigami.Theme.disabledTextColor
                                    font: Kirigami.Theme.smallFont
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    function percentText(percent) { return percent < 0 ? "—" : percent.toString(); }
    function alpha(color, opacity) { return Qt.rgba(color.r, color.g, color.b, opacity); }

    function paintRing(context, size, percent, progressColor) {
        context.clearRect(0, 0, size, size);
        const lineWidth = Math.max(2, Math.round(size / 12));
        const radius = size / 2 - lineWidth / 2;
        const center = size / 2;
        context.lineWidth = lineWidth;
        context.lineCap = "round";
        context.strokeStyle = alpha(Kirigami.Theme.textColor, 0.14);
        context.beginPath();
        context.arc(center, center, radius, 0, Math.PI * 2, false);
        context.stroke();
        if (percent >= 0) {
            context.strokeStyle = progressColor;
            context.beginPath();
            context.arc(center, center, radius, -Math.PI / 2,
                        -Math.PI / 2 + Math.PI * 2 * percent / 100, false);
            context.stroke();
        }
    }
}
