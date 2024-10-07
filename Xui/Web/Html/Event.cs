using System.Text.Json;

namespace Xui.Web;

#pragma warning disable IDE1006 // Naming Styles

public record class Event(
    bool? altKey = null,
    int? button = null,
    int? buttons = null,
    TouchPoint[]? changedTouches = null,
    double? clientX = null,
    double? clientY = null,
    string? code = null,
    bool? ctrlKey = null,
    HtmlElement? currentTarget = null,
    string? data = null,
    DataTransfer? dataTransfer = null,
    long? deltaMode = null,
    double? deltaX = null,
    double? deltaY = null,
    double? deltaZ = null,
    long? detail = null,
    string? inputType = null,
    bool? isComposing = null,
    bool? isTrusted = null,
    string? key = null,
    double? layerX = null,
    double? layerY = null,
    string? locale = null,
    int? location = null,
    bool? metaKey = null,
    double? movementX = null,
    double? movementY = null,
    double? offsetX = null,
    double? offsetY = null,
    double? pageX = null,
    double? pageY = null,
    HtmlElement? relatedTarget = null,
    bool? repeat = null,
    double? screenX = null,
    double? screenY = null,
    bool? shiftKey = null,
    HtmlElement? target = null,
    TouchPoint[]? targetTouches = null,
    TouchPoint[]? touches = null,
    string? type = null,
    double? x = null,
    double? y = null
) {
    public static readonly Event Empty = new();

    public static (int, Event?) ParseEvent(ReadOnlySpan<byte> buffer)
    {
        int i = 0, slot = 0;
        while (true)
        {
            // Convert from ASCII to int, digit by digit.
            int d = buffer[i] - 48;
            if (d >= 0 && d <= 9) {
                slot = slot * 10 + d;
                ++i;
                continue;
            }

            if (i >= buffer.Length - 1)
                return (slot, null);
            
            var message = buffer[i..];
            var @event = JsonSerializer.Deserialize<Event>(message);
            return (slot, @event);
        }
    }
}

public record class HtmlElement(
    string? id = null,
    string? name = null,
    string? type = null,
    string? value = null
);

public record class DataTransfer(
    string? dropEffect = null,
    string? effectAllowed = null,
    string[]? files = null,
    DataTransferItem[]? items = null,
    string[]? types = null
);

public record class DataTransferItem(
    string? kind = null,
    string? type = null
);

public record class TouchPoint(
    long? identifier = null,
    double? screenX = null,
    double? screenY = null,
    double? clientX = null,
    double? clientY = null,
    double? pageX = null,
    double? pageY = null
);

#pragma warning restore IDE1006 // Naming Styles
