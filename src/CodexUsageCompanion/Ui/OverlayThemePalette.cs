namespace CodexUsageCompanion.Ui;

internal sealed record OverlayThemePalette(
    string RootBackground,
    string RootBorder,
    string Shadow,
    string HeaderForeground,
    string CardBackground,
    string CardBorder,
    string CardTitle,
    string SecondaryText,
    string StatusText,
    string ErrorText,
    string EmptyCell,
    string Green,
    string Yellow,
    string Orange,
    string Red,
    string Gray)
{
    public static OverlayThemePalette Dark { get; } = new(
        "#F2272927",
        "#655B605B",
        "#70000000",
        "#CDD1CD",
        "#FF353835",
        "#FF4A4E4A",
        "#F4F5F3",
        "#B7BAB6",
        "#8E938E",
        "#FFFF8A80",
        "#FF4A4D49",
        "#FF4AD894",
        "#FFF2CF5B",
        "#FFFF9847",
        "#FFFF6666",
        "#FF777C78");

    public static OverlayThemePalette Light { get; } = new(
        "#F7FFFFFF",
        "#66777D77",
        "#35000000",
        "#FF343834",
        "#FFF3F5F2",
        "#FFD4D9D3",
        "#FF202420",
        "#FF606660",
        "#FF6A706A",
        "#FFC62828",
        "#FFD8DDD7",
        "#FF16835A",
        "#FF9C7A00",
        "#FFC65D00",
        "#FFC62828",
        "#FF707770");
}
