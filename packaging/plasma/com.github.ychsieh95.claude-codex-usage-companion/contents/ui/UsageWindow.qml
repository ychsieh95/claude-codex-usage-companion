// SPDX-License-Identifier: MIT

import QtQuick
import QtQuick.Layouts
import org.kde.kirigami as Kirigami
import org.kde.plasma.components as PlasmaComponents3
import org.kde.plasma.plasmoid

ColumnLayout {
    id: usageWindow
    required property string title
    required property var windowState
    required property var colorForPercent
    required property var controller
    property bool showPacePointer: true
    property double nowSeconds: Date.now() / 1000

    readonly property bool available: windowState !== null && windowState !== undefined &&
                                      typeof windowState.remainingPercent === "number"
    readonly property int remaining: available ? windowState.remainingPercent : -1
    readonly property bool showUsed: Plasmoid.configuration.percentMode === 1
    readonly property int shown: available ? (showUsed ? 100 - remaining : remaining) : -1
    readonly property color signalColor: available
        ? colorForPercent(remaining) : Kirigami.Theme.disabledTextColor
    spacing: Kirigami.Units.smallSpacing

    Timer {
        interval: 60000
        running: usageWindow.visible
        repeat: true
        triggeredOnStart: true
        onTriggered: usageWindow.nowSeconds = Date.now() / 1000
    }

    RowLayout {
        Layout.fillWidth: true
        spacing: Kirigami.Units.largeSpacing
        ColumnLayout {
            Layout.fillWidth: true
            spacing: 0
            PlasmaComponents3.Label {
                Layout.fillWidth: true
                text: usageWindow.title
                font.weight: Font.Medium
                elide: Text.ElideRight
            }
            PlasmaComponents3.Label {
                Layout.fillWidth: true
                text: usageWindow.resetText()
                visible: text.length > 0
                color: Kirigami.Theme.disabledTextColor
                font: Kirigami.Theme.smallFont
                elide: Text.ElideRight
            }
        }
        RowLayout {
            spacing: 2
            PlasmaComponents3.Label {
                text: usageWindow.available ? usageWindow.shown.toString() : "—"
                color: usageWindow.signalColor
                font.weight: Font.DemiBold
                font.pixelSize: Kirigami.Theme.defaultFont.pixelSize * 1.45
            }
            PlasmaComponents3.Label {
                text: usageWindow.available ? "%" : ""
                color: usageWindow.signalColor
                font: Kirigami.Theme.smallFont
                Layout.alignment: Qt.AlignBottom
                Layout.bottomMargin: 2
            }
        }
    }

    Item {
        Layout.fillWidth: true
        implicitHeight: Kirigami.Units.gridUnit
        Rectangle {
            id: track
            anchors.left: parent.left
            anchors.right: parent.right
            anchors.verticalCenter: parent.verticalCenter
            height: Math.max(6, Kirigami.Units.smallSpacing)
            radius: height / 2
            color: usageWindow.alpha(Kirigami.Theme.textColor, 0.12)
            Rectangle {
                width: parent.width * usageWindow.fractionShown()
                height: parent.height
                radius: parent.radius
                color: usageWindow.available ? usageWindow.signalColor : "transparent"
                Behavior on width {
                    NumberAnimation { duration: Kirigami.Units.longDuration; easing.type: Easing.OutCubic }
                }
            }
            Rectangle {
                id: paceMarker
                readonly property real elapsed: usageWindow.elapsedRatio()
                readonly property real expectedRemaining: 1 - elapsed
                readonly property real expectedShown: usageWindow.showUsed ? elapsed : expectedRemaining
                visible: usageWindow.showPacePointer && elapsed >= 0 && usageWindow.available
                // A dot centered on the track. A ring in the popup's background
                // color sets it apart from both the fill and the empty track.
                width: 14
                height: width
                radius: width / 2
                anchors.verticalCenter: parent.verticalCenter
                x: Math.max(0, Math.min(parent.width - width,
                                        parent.width * expectedShown - width / 2))
                color: Kirigami.Theme.textColor
                border.width: 2
                border.color: Kirigami.Theme.backgroundColor
                HoverHandler { id: paceHover }
                PlasmaComponents3.ToolTip {
                    visible: paceHover.hovered
                    text: usageWindow.showUsed
                        ? i18n("Even pace: about %1% should be used now. Your usage is %2.",
                               Math.round(paceMarker.elapsed * 100),
                               usageWindow.paceDescription(paceMarker.expectedRemaining))
                        : i18n("Even pace: about %1% should remain now. Your usage is %2.",
                               Math.round(paceMarker.expectedRemaining * 100),
                               usageWindow.paceDescription(paceMarker.expectedRemaining))
                }
            }
        }
    }

    function fractionRemaining() {
        return available ? Math.max(0, Math.min(1, remaining / 100)) : 0;
    }

    function fractionShown() {
        return available ? Math.max(0, Math.min(1, shown / 100)) : 0;
    }

    function elapsedRatio() {
        if (!available || typeof windowState.windowDurationMins !== "number" ||
                windowState.windowDurationMins <= 0 ||
                windowState.resetsAt === null || windowState.resetsAt === undefined) {
            return -1;
        }
        const windowSeconds = windowState.windowDurationMins * 60;
        const untilReset = Number(windowState.resetsAt) - nowSeconds;
        if (untilReset <= 0) {
            return -1;
        }
        return Math.max(0, Math.min(1, 1 - untilReset / windowSeconds));
    }

    function resetText() {
        if (!windowState || windowState.resetsAt === null || windowState.resetsAt === undefined) {
            return available ? i18n("Reset time unavailable") : i18n("Usage unavailable");
        }
        return controller.resetDescription(windowState.resetsAt, nowSeconds, true);
    }

    function paceDescription(expectedRemaining) {
        const delta = fractionRemaining() - expectedRemaining;
        if (Math.abs(delta) < 0.03) {
            return i18n("on pace");
        }
        return delta > 0 ? i18n("ahead of pace") : i18n("running faster than pace");
    }

    function alpha(color, opacity) { return Qt.rgba(color.r, color.g, color.b, opacity); }
}
