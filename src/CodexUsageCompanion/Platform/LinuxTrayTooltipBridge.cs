using System.Reflection;
using Avalonia.Controls;
using CodexUsageCompanion.Diagnostics;
using Tmds.DBus.Protocol;
using DBusArray = Tmds.DBus.Protocol.Array<
    Tmds.DBus.Protocol.Struct<
        int,
        int,
        Tmds.DBus.Protocol.Array<byte>>>;

namespace CodexUsageCompanion.Platform;

public sealed class LinuxTrayTooltipBridge
{
    private const string FreeDesktopImplementation =
        "Avalonia.FreeDesktop.DBusTrayIconImpl";
    private const string StatusNotifierInterface =
        "org.kde.StatusNotifierItem";
    private string _tooltipText = string.Empty;
    private Action? _invalidate;

    public void UpdateText(string text)
    {
        Volatile.Write(ref _tooltipText, text);
    }

    public void Invalidate()
    {
        try
        {
            Volatile.Read(ref _invalidate)?.Invoke();
        }
        catch (Exception exception) when (
            exception is InvalidOperationException or
            TargetException or
            TargetInvocationException)
        {
            CompanionLog.Shared.Write("tray-invalidate", exception);
        }
    }

    public bool TryInstall(TrayIcon trayIcon)
    {
        if (!OperatingSystem.IsLinux())
        {
            return false;
        }

        try
        {
            var implementation = GetFieldValue(trayIcon, "_impl");
            if (implementation?.GetType().FullName != FreeDesktopImplementation)
            {
                return false;
            }

            var connection = GetFieldValue(implementation, "_connection") as DBusConnection;
            var originalHandler =
                GetFieldValue(implementation, "_statusNotifierItemDbusObj")
                as IPathMethodHandler;
            if (connection is null || originalHandler is null)
            {
                return false;
            }

            connection.RemoveMethodHandler(originalHandler.Path);
            connection.AddMethodHandler(new TooltipMethodHandler(
                originalHandler,
                () => Volatile.Read(ref _tooltipText)));
            var invalidateMethod = originalHandler.GetType().GetMethod(
                "InvalidateAll",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Volatile.Write(
                ref _invalidate,
                invalidateMethod is null
                    ? null
                    : () => invalidateMethod.Invoke(originalHandler, null));
            Invalidate();
            return true;
        }
        catch (Exception exception) when (
            exception is ArgumentException or
            InvalidOperationException or
            NotSupportedException or
            TargetException or
            TargetInvocationException)
        {
            return false;
        }
    }

    public static (string Title, string Description) SplitText(string text)
    {
        var separator = text.IndexOfAny(['\r', '\n']);
        if (separator < 0)
        {
            return (text, string.Empty);
        }

        return (
            text[..separator],
            text[(separator + 1)..].TrimStart('\r', '\n'));
    }

    private static object? GetFieldValue(object instance, string fieldName)
    {
        return instance.GetType()
            .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)?
            .GetValue(instance);
    }

    private sealed class TooltipMethodHandler(
        IPathMethodHandler originalHandler,
        Func<string> tooltipProvider) : IPathMethodHandler
    {
        public string Path => originalHandler.Path;

        public bool HandlesChildPaths => originalHandler.HandlesChildPaths;

        public ValueTask HandleMethodAsync(MethodContext context)
        {
            if (IsGetAllRequest(context.Request))
            {
                ReplyGetAll(context);
                return ValueTask.CompletedTask;
            }

            if (!IsTooltipRequest(context.Request))
            {
                return originalHandler.HandleMethodAsync(context);
            }

            try
            {
                var (title, description) = SplitText(tooltipProvider());
                VariantValue emptyPixmaps = new DBusArray();
                var tooltip = VariantValue.Struct(
                    VariantValue.String(string.Empty),
                    emptyPixmaps,
                    VariantValue.String(title),
                    VariantValue.String(description));

                using var writer = context.CreateReplyWriter("v");
                writer.WriteVariant(tooltip);
                context.Reply(writer.CreateMessage());
            }
            catch (Exception exception)
            {
                CompanionLog.Shared.Write("tray-tooltip", exception);
                context.HandleException(exception, false);
            }

            return ValueTask.CompletedTask;
        }

        private void ReplyGetAll(MethodContext context)
        {
            try
            {
                var tooltipText = tooltipProvider();
                var (title, description) = SplitText(tooltipText);
                var menu = GetFieldValue(originalHandler, "_menu") is ObjectPath path
                    ? path
                    : new ObjectPath("/");
                var iconPixmaps = ToDBusPixmaps(
                    GetFieldValue(originalHandler, "_iconPixmap"));
                VariantValue emptyPixmaps = new DBusArray();
                var tooltip = VariantValue.Struct(
                    VariantValue.String(string.Empty),
                    emptyPixmaps,
                    VariantValue.String(title),
                    VariantValue.String(description));
                var properties = new KeyValuePair<string, VariantValue>[]
                {
                    new("Category", "ApplicationStatus"),
                    new("Id", tooltipText),
                    new("Title", tooltipText),
                    new("Status", "Active"),
                    new("WindowId", 0),
                    new("IconThemePath", string.Empty),
                    new("Menu", menu),
                    new("ItemIsMenu", false),
                    new("IconName", string.Empty),
                    new("IconPixmap", iconPixmaps),
                    new("OverlayIconName", string.Empty),
                    new("OverlayIconPixmap", emptyPixmaps),
                    new("AttentionIconName", string.Empty),
                    new("AttentionIconPixmap", emptyPixmaps),
                    new("AttentionMovieName", string.Empty),
                    new("ToolTip", tooltip)
                };

                using var writer = context.CreateReplyWriter("a{sv}");
                writer.WriteDictionary(properties);
                context.Reply(writer.CreateMessage());
            }
            catch (Exception exception)
            {
                CompanionLog.Shared.Write("tray-tooltip", exception);
                context.HandleException(exception, false);
            }
        }

        private static DBusArray ToDBusPixmaps(object? value)
        {
            var result = new DBusArray();
            if (value is not (int Width, int Height, byte[] Pixels)[] pixmaps)
            {
                return result;
            }

            foreach (var pixmap in pixmaps)
            {
                result.Add(new(
                    pixmap.Width,
                    pixmap.Height,
                    new Tmds.DBus.Protocol.Array<byte>(pixmap.Pixels)));
            }

            return result;
        }

        private static bool IsTooltipRequest(Message request)
        {
            try
            {
                var reader = request.GetBodyReader();
                var interfaceName = reader.ReadString();
                var propertyName = reader.ReadString();
                return interfaceName == StatusNotifierInterface &&
                       propertyName == "ToolTip";
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static bool IsGetAllRequest(Message request)
        {
            if (request.InterfaceAsString != "org.freedesktop.DBus.Properties" ||
                request.MemberAsString != "GetAll")
            {
                return false;
            }

            try
            {
                return request.GetBodyReader().ReadString() ==
                       StatusNotifierInterface;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
