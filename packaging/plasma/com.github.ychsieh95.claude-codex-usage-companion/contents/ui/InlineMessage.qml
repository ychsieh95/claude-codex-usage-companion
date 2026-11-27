// SPDX-License-Identifier: MIT

import QtQuick
import QtQuick.Layouts
import org.kde.kirigami as Kirigami
import org.kde.plasma.components as PlasmaComponents3

Rectangle {
    id: message
    required property string text
    required property string iconName
    required property color accentColor

    implicitHeight: messageRow.implicitHeight + Kirigami.Units.largeSpacing
    radius: Kirigami.Units.cornerRadius
    color: Qt.rgba(accentColor.r, accentColor.g, accentColor.b, 0.09)
    border.width: 1
    border.color: Qt.rgba(accentColor.r, accentColor.g, accentColor.b, 0.22)

    RowLayout {
        id: messageRow
        anchors.fill: parent
        anchors.margins: Kirigami.Units.smallSpacing
        spacing: Kirigami.Units.smallSpacing
        Kirigami.Icon {
            source: message.iconName
            implicitWidth: Kirigami.Units.iconSizes.small
            implicitHeight: Kirigami.Units.iconSizes.small
            color: message.accentColor
        }
        PlasmaComponents3.Label {
            Layout.fillWidth: true
            text: message.text
            wrapMode: Text.Wrap
            font: Kirigami.Theme.smallFont
        }
    }
}
