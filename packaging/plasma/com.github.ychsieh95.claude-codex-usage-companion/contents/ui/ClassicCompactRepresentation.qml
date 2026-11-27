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

    readonly property bool barView: Plasmoid.configuration.compactViewStyle === 1
    readonly property bool vertical: {
        if (width > 0 && height > 0 &&
                Math.abs(height - width) > Kirigami.Units.gridUnit) {
            return height > width;
        }
        return Plasmoid.formFactor === PlasmaCore.Types.Vertical;
    }
    readonly property int entryCount: Math.max(1, controller.compactEntries.length)
    readonly property real circleExtent: Kirigami.Units.iconSizes.medium + Kirigami.Units.smallSpacing
    readonly property real barExtent: Kirigami.Units.gridUnit * 4
    readonly property real entryExtent: barView ? barExtent : circleExtent
    readonly property real crossExtent: barView
        ? Kirigami.Units.iconSizes.medium + Kirigami.Units.smallSpacing
        : Kirigami.Units.iconSizes.medium

    Layout.minimumWidth: vertical ? Kirigami.Units.iconSizes.smallMedium : entryExtent * entryCount
    Layout.minimumHeight: vertical ? entryExtent * entryCount : Kirigami.Units.iconSizes.smallMedium
    Layout.preferredWidth: vertical ? crossExtent : entryExtent * entryCount
    Layout.preferredHeight: vertical ? entryExtent * entryCount : crossExtent
    activeFocusOnTab: true
    hoverEnabled: true
    onClicked: controller.activate()
    Keys.onSpacePressed: controller.activate()
    Keys.onReturnPressed: controller.activate()

    Loader {
        anchors.fill: parent
        sourceComponent: compact.barView ? barComponent : circleComponent
    }

    Component {
        id: circleComponent

        GridLayout {
            rows: compact.vertical ? compact.entryCount : 1
            columns: compact.vertical ? 1 : compact.entryCount
            rowSpacing: 0
            columnSpacing: 0

            Repeater {
                model: compact.displayEntries()

                delegate: Item {
                    id: circleItem
                    required property var modelData

                    Layout.fillWidth: true
                    Layout.fillHeight: true
                    Layout.preferredWidth: compact.circleExtent
                    Layout.preferredHeight: compact.circleExtent

                    readonly property real circleSize: Math.max(
                        Kirigami.Units.iconSizes.small,
                        Math.min(width, height) - Kirigami.Units.smallSpacing)
                    readonly property color ringColor: compact.controller.classicColorForPercent(modelData.percent)

                    Canvas {
                        id: progressRing
                        anchors.centerIn: parent
                        width: circleItem.circleSize
                        height: width
                        opacity: compact.controller.refreshing ? 0.55 : 1

                        onPaint: compact.paintRing(
                            getContext("2d"),
                            width,
                            circleItem.modelData.percent,
                            circleItem.ringColor)

                        Connections {
                            target: compact.controller
                            function onCompactEntriesChanged() { progressRing.requestPaint(); }
                        }
                    }

                    Column {
                        anchors.centerIn: parent
                        spacing: -2

                        PlasmaComponents3.Label {
                            anchors.horizontalCenter: parent.horizontalCenter
                            text: compact.percentText(circleItem.modelData.percent)
                            font.bold: true
                            font.pixelSize: Math.max(8, circleItem.circleSize * 0.28)
                        }

                        PlasmaComponents3.Label {
                            anchors.horizontalCenter: parent.horizontalCenter
                            text: compact.classicLabel(circleItem.modelData.shortLabel)
                            opacity: 0.7
                            font.pixelSize: Math.max(6, circleItem.circleSize * 0.18)
                        }
                    }
                }
            }
        }
    }

    Component {
        id: barComponent

        GridLayout {
            rows: compact.vertical ? compact.entryCount : 1
            columns: compact.vertical ? 1 : compact.entryCount
            rowSpacing: 0
            columnSpacing: 0

            Repeater {
                model: compact.displayEntries()

                delegate: Item {
                    id: barItem
                    required property var modelData

                    Layout.fillWidth: true
                    Layout.fillHeight: true
                    Layout.preferredWidth: compact.vertical ? compact.crossExtent : compact.barExtent
                    Layout.preferredHeight: compact.vertical ? compact.barExtent : compact.crossExtent

                    readonly property real fraction: modelData.percent < 0
                        ? 0
                        : Math.max(0, Math.min(1, modelData.percent / 100))
                    readonly property color progressColor: compact.controller.classicColorForPercent(modelData.percent)

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
                                text: compact.classicLabel(barItem.modelData.shortLabel)
                                opacity: 0.75
                                font.pixelSize: Math.max(7, Kirigami.Theme.smallFont.pixelSize * 0.85)
                            }

                            PlasmaComponents3.Label {
                                text: compact.percentText(barItem.modelData.percent)
                                font.bold: true
                                font.pixelSize: Math.max(8, Kirigami.Theme.smallFont.pixelSize)
                            }
                        }

                        Rectangle {
                            Layout.fillWidth: true
                            implicitHeight: Math.max(4, Kirigami.Units.smallSpacing)
                            radius: height / 2
                            color: compact.trackColor()

                            Rectangle {
                                width: parent.width * barItem.fraction
                                height: parent.height
                                radius: parent.radius
                                color: barItem.modelData.percent < 0 ? "transparent" : barItem.progressColor

                                Behavior on width {
                                    NumberAnimation { duration: Kirigami.Units.shortDuration }
                                }
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
                            implicitWidth: Math.max(4, Kirigami.Units.smallSpacing)
                            radius: width / 2
                            color: compact.trackColor()

                            Rectangle {
                                anchors.bottom: parent.bottom
                                width: parent.width
                                height: parent.height * barItem.fraction
                                radius: parent.radius
                                color: barItem.modelData.percent < 0 ? "transparent" : barItem.progressColor

                                Behavior on height {
                                    NumberAnimation { duration: Kirigami.Units.shortDuration }
                                }
                            }
                        }

                        ColumnLayout {
                            Layout.fillWidth: true
                            Layout.alignment: Qt.AlignVCenter
                            spacing: -2

                            PlasmaComponents3.Label {
                                Layout.alignment: Qt.AlignHCenter
                                text: compact.percentText(barItem.modelData.percent)
                                font.bold: true
                                font.pixelSize: Math.max(8, Kirigami.Theme.smallFont.pixelSize)
                            }

                            PlasmaComponents3.Label {
                                Layout.alignment: Qt.AlignHCenter
                                text: compact.classicLabel(barItem.modelData.shortLabel)
                                opacity: 0.75
                                font.pixelSize: Math.max(7, Kirigami.Theme.smallFont.pixelSize * 0.85)
                            }
                        }
                    }
                }
            }
        }
    }

    function displayEntries() {
        return controller.compactEntries.length > 0
            ? controller.compactEntries
            : [{ "shortLabel": "", "title": i18n("Usage unavailable"), "percent": -1 }];
    }

    // The latest design's short labels mapped back to the original ones.
    function classicLabel(label) {
        const labels = { "C · S": "Cl·S", "C · W": "Cl·W", "X · 5": "Cx·5", "X · W": "Cx·W" };
        return labels[label] !== undefined ? labels[label] : (label || "");
    }

    function percentText(percent) {
        return percent < 0 ? "--" : percent.toString();
    }

    function trackColor() {
        return Qt.rgba(
            Kirigami.Theme.textColor.r,
            Kirigami.Theme.textColor.g,
            Kirigami.Theme.textColor.b,
            0.16);
    }

    function paintRing(context, size, percent, progressColor) {
        context.clearRect(0, 0, size, size);
        const lineWidth = Math.max(2, Math.round(size / 12));
        const radius = size / 2 - lineWidth / 2;
        const center = size / 2;

        context.lineWidth = lineWidth;
        context.lineCap = "round";
        context.strokeStyle = trackColor();
        context.beginPath();
        context.arc(center, center, radius, 0, Math.PI * 2, false);
        context.stroke();

        if (percent >= 0) {
            context.strokeStyle = progressColor;
            context.beginPath();
            context.arc(
                center,
                center,
                radius,
                -Math.PI / 2,
                -Math.PI / 2 + Math.PI * 2 * percent / 100,
                false);
            context.stroke();
        }
    }
}
