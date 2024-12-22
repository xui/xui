using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Text;
using Xui.Web.Composers;

namespace Xui.Web.HttpX.Composers;

using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Text;

public static class DiffComposerExtensions
{
    private static bool warmedUp = false;
    public static async Task DebugSnapshot(this Func<Html> html, PipeWriter writer)
    {
        var composer = new DiffComposer();
        // // Warmup...
        // var warmup = Stopwatch.StartNew();
        // long c = 0;
        // while (warmup.ElapsedMilliseconds < 1000)
        // {
        //     c++;
        // }
        // Console.WriteLine(c);
        // if (!warmedUp)
        // {
            // for (int i = 0; i < 250_000; i++)
                // composer.Compose($"{html()}");
        //     warmedUp = true;
        // }
        // long gc1 = GC.GetTotalAllocatedBytes();
        long gc1 = GC.GetAllocatedBytesForCurrentThread();
        var sw1 = Stopwatch.GetTimestamp();
        // for (int i = 0; i < 250_000; i++)
            composer.Compose($"{html()}");
        var elapsed = Stopwatch.GetElapsedTime(sw1);
        long gc2 = GC.GetAllocatedBytesForCurrentThread();
        // long gc2 = GC.GetTotalAllocatedBytes();

        Console.WriteLine($"elapsed: {elapsed.TotalNanoseconds} ns, allocations: {(gc2 - gc1):n0} bytes");

        var output = GetOutput(composer, elapsed, gc2 - gc1);
        // var output = GetOutput(Memory<Chunk>.Empty, elapsed, gc2 - gc1);
        writer.Inject($"{output.ToString()}");
        await writer.FlushAsync();
    }

    private static StringBuilder GetOutput(DiffComposer composer, TimeSpan elapsed, long bytesAllocated)
    {
        // double ns = 1_000_000_000.0 * (double)ticks / Stopwatch.Frequency;
        // double us = 1_000_000.0 * (double)ticks / Stopwatch.Frequency;

        Chunk[] slotTable = composer.SlotTable;
        var output = new StringBuilder();

        output.Append($"""
            var cssDefault = "font-weight:normal;font-family:monospace,monospace;";
            var cssVariable = "color:#aadbfb;font-weight:normal;font-family:monospace,monospace;";
            var cssFunction = "color:#f3c349;font-weight:normal;font-family:monospace,monospace;";
            var cssHtml = "font-weight:normal;font-family:monospace,monospace;";
            var cssNumber = "color:#9581f7;font-weight:normal;font-family:monospace,monospace;";
            var cssString = "color:#79c8ea;font-weight:normal;font-family:monospace,monospace;";
            var cssType = "color:#6fc3a7;font-weight:normal;font-family:monospace,monospace;";
            var cssOperator = "font-weight:normal;font-family:monospace,monospace;";
            var cssLiteral = "color:#666666;font-weight:normal;font-family:monospace,monospace;";
            var cssNotes = "font-size:10px;color:#808080;font-weight:normal;font-family:monospace,monospace;";

            console.groupCollapsed("Server Diff\n%c(expand for details)", cssNotes);
            """);

        for (int i = 0; i < composer.Length; i++)
        {
            ref Chunk slot = ref slotTable[i];
            switch (slot.Type)
            {
                case FormatType.StringLiteral:
                    output.AppendLine($"""
                        console.groupCollapsed(`{$"[{i}]",-4}  %cslotC  {"",-15} 🟢 %c"{InlineString(slot.String)}"`, cssVariable, cssLiteral);
                            console.log(`{slot.String}`);
                        console.groupEnd();
                        """);
                    break;
                case FormatType.String:
                    output.AppendLine($"""
                        console.groupCollapsed(`{$"[{i}]",-4}  %cslotF%c: %c{slot.Type,-15} 🟢 %c'{slot.String}'`, cssVariable, cssOperator, cssType, cssString);
                        console.groupEnd();
                        """);
                    break;
                case FormatType.Integer:
                    output.AppendLine($"""
                        console.groupCollapsed(`{$"[{i}]",-4}  %cslotF%c: %c{slot.Type,-15} 🟢 %c{slot.Integer}`, cssVariable, cssOperator, cssType, cssNumber);
                        console.groupEnd();
                        """);
                    break;
                case FormatType.EventHandler:
                    output.AppendLine($"""
                        console.groupCollapsed(`{$"[{i}]",-4}  %cslotD%c: %c{slot.Type,-15} 🟢 %c{ $"{{ {slot.String} }}" }`, cssVariable, cssOperator, cssType, cssDefault);
                        console.groupEnd();
                        """);
                    break;
                case FormatType.Attribute:
                    output.AppendLine($"""
                        console.groupCollapsed(`{$"[{i}]",-4}  %cslotD%c: %c{slot.Type,-15} 🟢 %c{ $"{{ {slot.String} }}" }`, cssVariable, cssOperator, cssType, cssDefault);
                        console.groupEnd();
                        """);
                    break;
                case FormatType.HtmlString:
                    output.AppendLine($"""
                        console.groupCollapsed(`{$"[{i}]",-4}  %cslotD%c: %c{slot.Type,-15} 🟢 %c{ $"{{ {slot.String} }}" }`, cssVariable, cssOperator, cssType, cssDefault);
                        console.groupEnd();
                        """);
                    break;
                default:
                    output.AppendLine($"""
                        console.groupCollapsed(`{$"[{i}]",-4}  %cslotF%c: %c{slot.Type,-15} 🔴 %c{slot.Integer}`, cssVariable, cssOperator, cssType, cssNumber);
                        console.groupEnd();
                        """);
                    break;
            }
        }

        output.Append($"""
            console.log("\n%cBenchmark this shell:\n%c› %cserver.%cbenchmark%c();", cssType, cssVariable, cssDefault, cssFunction, cssDefault);
            console.groupEnd();
            """);
        return output;
    }

    private static string? InlineString(string? value)
    {
        int maxLength = 100;
        var inlined = value
            ?.Replace("\n", "")
            ?.Replace("\r", "") 
            ?.Replace("  ", "")
            ?? "";
        return (inlined.Length > maxLength)
            ? inlined[..(maxLength-3)] + "..."
            : inlined;
    }
}

public class DiffComposer : BaseComposer
{
    private static int highWaterMark = 2048;
    // private Chunk[] slotTable;
    private static Chunk[] slotTable = ArrayPool<Chunk>.Shared.Rent(highWaterMark);
    public Chunk[] SlotTable { get => slotTable; }
    // private int segmentPosition = 0;
    public int Length { get; private set; } = 0;
    public int WriteHead { get; private set; } = 0;

    public DiffComposer()
    {
        // slotTable = ArrayPool<Chunk>.Shared.Rent(highWaterMark);
    }

    protected override void Clear()
    {
        WriteHead = 0;
        Length = Cursor; // TODO: Fix this (+2) once the empty slot problem is solved.
        // currentIndex = 0;
        // parentStartIndex = 0;

        base.Clear();
    }

    public override void PrepareHtml(ref Html html, int literalLength, int formattedCount)
    {
        html.Index = WriteHead;
        WriteHead += (2 * formattedCount + 1);

        // ref var chunk = ref slotTable[index];
        // chunk.SlotId = Cursor;
        // // chunk.RefId = parentStartIndex;
        // chunk.Type = FormatType.HtmlString;
        // chunk.String = $" --------start-----------: literalLength: {literalLength}, formattedCount: {formattedCount}";

        // // Update the "starting end cap" to point its end.
        // ref var start = ref slotTable[parentStartIndex];
        // start.Integer = Cursor;

        base.PrepareHtml(ref html, literalLength, formattedCount);
    }

    public override bool WriteImmutableMarkup(ref Html html, string literal)
    {
        ref var chunk = ref slotTable[html.Index + html.Length++];
        chunk.SlotId = Cursor;
        // chunk.RefId = parentStartIndex;
        chunk.String = literal;
        chunk.Type = FormatType.StringLiteral;

        return base.WriteImmutableMarkup(ref html, literal);
    }

    public override bool WriteMutableValue(ref Html html, string value)
    {
        ref var chunk = ref slotTable[html.Index + html.Length++];
        chunk.SlotId = Cursor;
        // chunk.RefId = parentStartIndex;
        chunk.String = value;
        chunk.Type = FormatType.String;
        chunk.Format = null;

        return base.WriteMutableValue(ref html, value);
    }

    public override bool WriteMutableValue(ref Html html, bool value)
    {
        ref var chunk = ref slotTable[html.Index + html.Length++];
        chunk.SlotId = Cursor;
        // chunk.RefId = parentStartIndex;
        chunk.Boolean = value;
        chunk.Type = FormatType.Boolean;
        chunk.Format = null;

        return base.WriteMutableValue(ref html, value);
    }

    public override bool WriteMutableValue<T>(ref Html html, T value, string? format = default)
        // where T : struct, IUtf8SpanFormattable // (from base)
    {
        ref var chunk = ref slotTable[html.Index + html.Length++];
        chunk.SlotId = Cursor;
        // chunk.RefId = parentStartIndex;
        chunk.Format = format;

        switch (value)
        {
            case int i:
                chunk.Integer = i;
                chunk.Type = FormatType.Integer;
                break;
            case long l:
                chunk.Long = l;
                chunk.Type = FormatType.Long;
                break;
            case float f:
                chunk.Float = f;
                chunk.Type = FormatType.Float;
                break;
            case double d:
                chunk.Double = d;
                chunk.Type = FormatType.Double;
                break;
            case decimal m:
                chunk.Decimal = m;
                chunk.Type = FormatType.Decimal;
                break;
            case DateTime dt:
                chunk.DateTime = dt;
                chunk.Type = FormatType.DateTime;
                break;
            case TimeSpan ts:
                chunk.TimeSpan = ts;
                chunk.Type = FormatType.TimeSpan;
                break;
            default:
                // In the future, possibly support other/custom IUtf8SpanFormattable types?
                // This may require much boxing/unboxing.
                // Currently Html.cs is a gatekeeper for these types.
                // So this is currently technically a dead code path.
                throw new NotSupportedException($"Type {typeof(T)} not supported");            
        }

        return base.WriteMutableValue(ref html, value, format);
    }

    public override bool WriteMutableAttribute(ref Html html, ReadOnlySpan<char> attrName, Func<Event, bool> attrValue, string? expression = null)
    {
        // end = h.end;

        ref var chunk = ref slotTable[html.Index + html.Length++];
        chunk.SlotId = Cursor;
        // chunk.RefId = ...TBD.. // actually I DO know it since this is executing AFTER the HTML instantiates
        // chunk.Integer = h.start;
        chunk.Type = FormatType.Attribute;
        chunk.String = expression;

        // ref var start = ref chunks[h.start];
        // start.Integer = Cursor;

        return base.WriteMutableAttribute(ref html, attrName, attrValue, expression);
    }

    public override bool WriteMutableAttribute<T>(ref Html html, ReadOnlySpan<char> attrName, Func<Event, T> attrValue, string? format = null, string? expression = null)
        // where T : struct, IUtf8SpanFormattable // (from base)
    {
        // end = h.end;

        ref var chunk = ref slotTable[html.Index + html.Length++];
        chunk.SlotId = Cursor;
        // chunk.RefId = ...TBD.. // actually I DO know it since this is executing AFTER the HTML instantiates
        // chunk.Integer = h.start;
        chunk.Type = FormatType.Attribute;
        chunk.String = expression;

        // ref var start = ref chunks[h.start];
        // start.Integer = Cursor;

        return base.WriteMutableAttribute(ref html, attrName, attrValue, format, expression);
    }

    public override bool WriteMutableAttribute(ref Html html, ReadOnlySpan<char> attrName, Func<string, Html> attrValue, string? expression = null)
    {
        // end = h.end;

        ref var chunk = ref slotTable[html.Index + html.Length++];
        chunk.SlotId = Cursor;
        // chunk.RefId = ...TBD.. // actually I DO know it since this is executing AFTER the HTML instantiates
        // chunk.Integer = h.start;
        chunk.Type = FormatType.Attribute;
        chunk.String = expression;

        // ref var start = ref chunks[h.start];
        // start.Integer = Cursor;

        return base.WriteMutableAttribute(ref html, attrName, attrValue, expression);
    }

    public override bool WriteEventHandler(ref Html html, Action eventHandler, string? expression = null) => WriteEventHandler(ref html, expression);
    public override bool WriteEventHandler(ref Html html, Action<Event> eventHandler, string? expression = null) => WriteEventHandler(ref html, expression);
    public override bool WriteEventHandler(ref Html html, Func<Task> eventHandler, string? expression = null) => WriteEventHandler(ref html, expression);
    public override bool WriteEventHandler(ref Html html, Func<Event, Task> eventHandler, string? expression = null) => WriteEventHandler(ref html, expression);
    public override bool WriteEventHandler(ref Html html, ReadOnlySpan<char> argName, Action eventHandler, string? expression = null) => WriteEventHandler(ref html, expression);
    public override bool WriteEventHandler(ref Html html, ReadOnlySpan<char> argName, Action<Event> eventHandler, string? expression = null) => WriteEventHandler(ref html, expression);
    public override bool WriteEventHandler(ref Html html, ReadOnlySpan<char> argName, Func<Task> eventHandler, string? expression = null) => WriteEventHandler(ref html, expression);
    public override bool WriteEventHandler(ref Html html, ReadOnlySpan<char> argName, Func<Event, Task> eventHandler, string? expression = null) => WriteEventHandler(ref html, expression);
    private bool WriteEventHandler(ref Html html, string? expression = null)
    {
        ref var chunk = ref slotTable[html.Index + html.Length++];
        chunk.SlotId = Cursor;
        chunk.Type = FormatType.EventHandler;
        chunk.String = expression;

        return CompleteFormattedValue();
    }

    public override bool WriteMutableElement<TView>(ref Html html, TView view) => WriteMutableElement(ref html, view.Render());
    public override bool WriteMutableElement(ref Html html, Slot slot) => WriteMutableElement(ref html, slot());
    public override bool WriteMutableElement(ref Html html, Html partial, string? expression = null)
    {
        ref var chunk = ref slotTable[html.Index + html.Length++];
        chunk.SlotId = Cursor;
        // chunk.RefId = parentStartIndex;
        chunk.Type = FormatType.HtmlString;
        chunk.String = expression + $" Index:{partial.Index} Length:{partial.Length}";

        // // Update the "starting end cap" to point its end.
        // ref var start = ref slotTable[parentStartIndex];
        // start.Integer = Cursor;

        return base.WriteMutableElement(ref html, partial);
    }
}