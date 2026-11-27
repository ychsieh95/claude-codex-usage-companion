// SPDX-License-Identifier: MIT

import QtQuick
import QtQuick.Layouts
import org.kde.kirigami as Kirigami
import org.kde.plasma.components as PlasmaComponents3

ColumnLayout {
    id: usageWindow

    required property string title
    required property var windowState
    required property var colorForPercent
    property bool showPacePointer: true

    // Refreshed every minute so the even-pace pointer follows the clock.
    property double nowSeconds: Date.now() / 1000

    spacing: 2

    Timer {
        interval: 60000
        running: usageWindow.visible && usageWindow.showPacePointer
        repeat: true
        triggeredOnStart: true
        onTriggered: usageWindow.nowSeconds = Date.now() / 1000
    }

    RowLayout {
        Layout.fillWidth: true

        PlasmaComponents3.Label {
            Layout.fillWidth: true
            text: usageWindow.title
            opacity: 0.85
            font.pointSize: Kirigami.Theme.smallFont.pointSize
        }

        PlasmaComponents3.Label {
            text: usageWindow.windowState
                ? i18n("%1% remaining", usageWindow.windowState.remainingPercent)
                : i18n("Unavailable")
            color: usageWindow.windowState
                ? usageWindow.colorForPercent(usageWindow.windowState.remainingPercent)
                : Kirigami.Theme.disabledTextColor
            font.bold: true
            font.pointSize: Kirigami.Theme.smallFont.pointSize
        }
    }

    Rectangle {
        id: track
        Layout.fillWidth: true
        implicitHeight: Kirigami.Units.smallSpacing + 3
        radius: height / 2
        color: Qt.rgba(
            Kirigami.Theme.textColor.r,
            Kirigami.Theme.textColor.g,
            Kirigami.Theme.textColor.b,
            0.12)

        Rectangle {
            width: parent.width * usageWindow.fractionRemaining()
            height: parent.height
            radius: parent.radius
            color: usageWindow.windowState
                ? usageWindow.colorForPercent(usageWindow.windowState.remainingPercent)
                : "transparent"

            Behavior on width {
                NumberAnimation { duration: Kirigami.Units.shortDuration }
            }
        }

        // Even-pace pointer: where the bar would end if usage were spread
        // evenly across the window. A bar that ends before it is running fast.
        Rectangle {
            id: paceMarker
            readonly property real elapsed: usageWindow.elapsedRatio()
            visible: usageWindow.showPacePointer && elapsed >= 0 && usageWindow.windowState !== null
            width: 2
            height: parent.height + 4
            radius: 1
            anchors.verticalCenter: parent.verticalCenter
            x: Math.max(0, Math.min(parent.width - width, parent.width * (1 - elapsed) - width / 2))
            color: Kirigami.Theme.textColor

            HoverHandler { id: paceHover }

            PlasmaComponents3.ToolTip {
                visible: paceHover.hovered
                text: i18n("Even-pace pointer: %1% of this window has passed. A bar that ends before the pointer means you are using it faster than an even pace.",
                           Math.round(paceMarker.elapsed * 100))
            }
        }
    }

    PlasmaComponents3.Label {
        Layout.fillWidth: true
        text: usageWindow.resetText()
        visible: text.length > 0
        opacity: 0.65
        elide: Text.ElideRight
        font.pointSize: Kirigami.Theme.smallFont.pointSize
    }

    function fractionRemaining() {
        if (!windowState || typeof windowState.remainingPercent !== "number") {
            return 0;
        }
        return Math.max(0, Math.min(1, windowState.remainingPercent / 100));
    }

    // Share of the reset window already passed (0..1), or -1 when unknown.
    function elapsedRatio() {
        if (!windowState || typeof windowState.windowDurationMins !== "number" ||
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
            return "";
        }
        const reset = new Date(Number(windowState.resetsAt) * 1000);
        return i18n("Resets %1", reset.toLocaleString(Qt.locale(), Locale.ShortFormat));
    }
}
