namespace CodexUsageCompanion.Configuration;

public static class WindowPosition
{
    public const string LeftTop = "left-top";
    public const string MiddleTop = "middle-top";
    public const string RightTop = "right-top";
    public const string LeftCenter = "left-center";
    public const string MiddleCenter = "middle-center";
    public const string RightCenter = "right-center";
    public const string LeftBottom = "left-bottom";
    public const string MiddleBottom = "middle-bottom";
    public const string RightBottom = "right-bottom";

    public static IReadOnlyList<string> Values { get; } = Array.AsReadOnly(
    [
        LeftTop,
        MiddleTop,
        RightTop,
        LeftCenter,
        MiddleCenter,
        RightCenter,
        LeftBottom,
        MiddleBottom,
        RightBottom
    ]);

    public static string Normalize(string? value)
    {
        return value switch
        {
            LeftTop or "top-left" => LeftTop,
            MiddleTop => MiddleTop,
            RightTop or "top-right" => RightTop,
            LeftCenter => LeftCenter,
            MiddleCenter => MiddleCenter,
            RightCenter => RightCenter,
            LeftBottom or "bottom-left" => LeftBottom,
            MiddleBottom => MiddleBottom,
            RightBottom or "bottom-right" => RightBottom,
            _ => RightBottom
        };
    }
}
