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
        // var output = GetOutput(Memory<Keyhole>.Empty, elapsed, gc2 - gc1);
        writer.Inject($"{output.ToString()}");
        await writer.FlushAsync();
    }

    private static StringBuilder GetOutput(DiffComposer composer, TimeSpan elapsed, long bytesAllocated)
    {
        // double ns = 1_000_000_000.0 * (double)ticks / Stopwatch.Frequency;
        // double us = 1_000_000.0 * (double)ticks / Stopwatch.Frequency;

        Keyhole[] keyholes = composer.Keyholes;
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
            ref Keyhole keyhole = ref keyholes[i];
            switch (keyhole.Type)
            {
                case FormatType.StringLiteral:
                    output.AppendLine($"""
                        console.groupCollapsed(`{$"[{i}]",-4}  %ckeyC  {"",-15} 🟢 %c"{InlineString(keyhole.String)}"`, cssVariable, cssLiteral);
                            console.log(`{keyhole.String}`);
                        console.groupEnd();
                        """);
                    break;
                case FormatType.String:
                    output.AppendLine($"""
                        console.groupCollapsed(`{$"[{i}]",-4}  %ckeyF%c: %c{keyhole.Type,-15} 🟢 %c'{keyhole.String}'`, cssVariable, cssOperator, cssType, cssString);
                        console.groupEnd();
                        """);
                    break;
                case FormatType.Integer:
                    output.AppendLine($"""
                        console.groupCollapsed(`{$"[{i}]",-4}  %ckeyF%c: %c{keyhole.Type,-15} 🟢 %c{keyhole.Integer}`, cssVariable, cssOperator, cssType, cssNumber);
                        console.groupEnd();
                        """);
                    break;
                case FormatType.EventHandler:
                    output.AppendLine($"""
                        console.groupCollapsed(`{$"[{i}]",-4}  %ckeyD%c: %c{keyhole.Type,-15} 🟢 %c{ $"{{ {keyhole.String} }}" }`, cssVariable, cssOperator, cssType, cssDefault);
                        console.groupEnd();
                        """);
                    break;
                case FormatType.Attribute:
                    output.AppendLine($"""
                        console.groupCollapsed(`{$"[{i}]",-4}  %ckeyD%c: %c{keyhole.Type,-15} 🟢 %c{ $"{{ {keyhole.String} }}" }`, cssVariable, cssOperator, cssType, cssDefault);
                        console.groupEnd();
                        """);
                    break;
                case FormatType.HtmlString:
                    output.AppendLine($"""
                        console.groupCollapsed(`{$"[{i}]",-4}  %ckeyD%c: %c{keyhole.Type,-15} 🟢 %c{ $"{{ {keyhole.String} }}" }`, cssVariable, cssOperator, cssType, cssDefault);
                        console.groupEnd();
                        """);
                    break;
                default:
                    output.AppendLine($"""
                        console.groupCollapsed(`{$"[{i}]",-4}  %ckeyF%c: %c{keyhole.Type,-15} 🔴 %c{keyhole.Integer}`, cssVariable, cssOperator, cssType, cssNumber);
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
    // private Keyhole[] keyholes;
    private static Keyhole[] keyholes = ArrayPool<Keyhole>.Shared.Rent(highWaterMark);
    public Keyhole[] Keyholes { get => keyholes; }
    // private int segmentPosition = 0;
    public int Length { get; private set; } = 0;
    public int WriteHead { get; private set; } = 0;

    public DiffComposer()
    {
        // keyholes = ArrayPool<Keyhole>.Shared.Rent(highWaterMark);
    }

    private void Enumerate()
    {
        ref var keyhole = ref keyholes[1];
    }

    protected override void Clear()
    {
        Enumerate();

        WriteHead = 0;
        Length = Cursor; // TODO: Fix this (+2) once the empty keyhole problem is solved.
        // currentIndex = 0;
        // parentStartIndex = 0;

        base.Clear();
    }

    public override void PrepareHtml(ref Html html, int literalLength, int formattedCount)
    {
        html.Index = WriteHead;
        WriteHead += (2 * formattedCount + 1);

        // ref var keyhole = ref keyholes[index];
        // keyhole.Key = Cursor;
        // // keyhole.RefId = parentStartIndex;
        // keyhole.Type = FormatType.HtmlString;
        // keyhole.String = $" --------start-----------: literalLength: {literalLength}, formattedCount: {formattedCount}";

        // // Update the "starting end cap" to point its end.
        // ref var start = ref keyholes[parentStartIndex];
        // start.Integer = Cursor;

        base.PrepareHtml(ref html, literalLength, formattedCount);
    }

    public override bool WriteImmutableMarkup(ref Html html, string literal)
    {
        ref var keyhole = ref keyholes[html.Index + html.Length++];
        keyhole.Key = Cursor;
        // keyhole.RefId = parentStartIndex;
        keyhole.String = literal;
        keyhole.Type = FormatType.StringLiteral;

        return base.WriteImmutableMarkup(ref html, literal);
    }

    public override bool WriteMutableValue(ref Html html, string value)
    {
        if (IsEven(html.Length))
            WriteImmutableMarkup(ref html, string.Empty);

        ref var keyhole = ref keyholes[html.Index + html.Length++];
        keyhole.Key = Cursor;
        // keyhole.RefId = parentStartIndex;
        keyhole.String = value;
        keyhole.Type = FormatType.String;
        keyhole.Format = null;

        return base.WriteMutableValue(ref html, value);
    }

    public override bool WriteMutableValue(ref Html html, bool value)
    {
        if (IsEven(html.Length))
            WriteImmutableMarkup(ref html, string.Empty);

        ref var keyhole = ref keyholes[html.Index + html.Length++];
        keyhole.Key = Cursor;
        // keyhole.RefId = parentStartIndex;
        keyhole.Boolean = value;
        keyhole.Type = FormatType.Boolean;
        keyhole.Format = null;

        return base.WriteMutableValue(ref html, value);
    }

    public override bool WriteMutableValue<T>(ref Html html, T value, string? format = default)
        // where T : struct, IUtf8SpanFormattable // (from base)
    {
        if (IsEven(html.Length))
            WriteImmutableMarkup(ref html, string.Empty);

        ref var keyhole = ref keyholes[html.Index + html.Length++];
        keyhole.Key = Cursor;
        // keyhole.RefId = parentStartIndex;
        keyhole.Format = format;

        switch (value)
        {
            case int i:
                keyhole.Integer = i;
                keyhole.Type = FormatType.Integer;
                break;
            case long l:
                keyhole.Long = l;
                keyhole.Type = FormatType.Long;
                break;
            case float f:
                keyhole.Float = f;
                keyhole.Type = FormatType.Float;
                break;
            case double d:
                keyhole.Double = d;
                keyhole.Type = FormatType.Double;
                break;
            case decimal m:
                keyhole.Decimal = m;
                keyhole.Type = FormatType.Decimal;
                break;
            case DateTime dt:
                keyhole.DateTime = dt;
                keyhole.Type = FormatType.DateTime;
                break;
            case TimeSpan ts:
                keyhole.TimeSpan = ts;
                keyhole.Type = FormatType.TimeSpan;
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
        if (IsEven(html.Length))
            WriteImmutableMarkup(ref html, string.Empty);

        // end = h.end;

        ref var keyhole = ref keyholes[html.Index + html.Length++];
        keyhole.Key = Cursor;
        // keyhole.RefId = ...TBD.. // actually I DO know it since this is executing AFTER the HTML instantiates
        // keyhole.Integer = h.start;
        keyhole.Type = FormatType.Attribute;
        keyhole.String = expression;

        // ref var start = ref keyholes[h.start];
        // start.Integer = Cursor;

        return base.WriteMutableAttribute(ref html, attrName, attrValue, expression);
    }

    public override bool WriteMutableAttribute<T>(ref Html html, ReadOnlySpan<char> attrName, Func<Event, T> attrValue, string? format = null, string? expression = null)
        // where T : struct, IUtf8SpanFormattable // (from base)
    {
        if (IsEven(html.Length))
            WriteImmutableMarkup(ref html, string.Empty);

        // end = h.end;

        ref var keyhole = ref keyholes[html.Index + html.Length++];
        keyhole.Key = Cursor;
        // keyhole.RefId = ...TBD.. // actually I DO know it since this is executing AFTER the HTML instantiates
        // keyhole.Integer = h.start;
        keyhole.Type = FormatType.Attribute;
        keyhole.String = expression;

        // ref var start = ref keyholes[h.start];
        // start.Integer = Cursor;

        return base.WriteMutableAttribute(ref html, attrName, attrValue, format, expression);
    }

    public override bool WriteMutableAttribute(ref Html html, ReadOnlySpan<char> attrName, Func<string, Html> attrValue, string? expression = null)
    {
        if (IsEven(html.Length))
            WriteImmutableMarkup(ref html, string.Empty);

        // end = h.end;

        ref var keyhole = ref keyholes[html.Index + html.Length++];
        keyhole.Key = Cursor;
        // keyhole.RefId = ...TBD.. // actually I DO know it since this is executing AFTER the HTML instantiates
        // keyhole.Integer = h.start;
        keyhole.Type = FormatType.Attribute;
        keyhole.String = expression;

        // ref var start = ref keyholes[h.start];
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
        if (IsEven(html.Length))
            WriteImmutableMarkup(ref html, string.Empty);

        ref var keyhole = ref keyholes[html.Index + html.Length++];
        keyhole.Key = Cursor;
        keyhole.Type = FormatType.EventHandler;
        keyhole.String = expression;

        return CompleteFormattedValue();
    }

    public override bool WriteMutableElement<TView>(ref Html html, TView view) => WriteMutableElement(ref html, view.Render());
    public override bool WriteMutableElement(ref Html html, Slot slot) => WriteMutableElement(ref html, slot());
    public override bool WriteMutableElement(ref Html html, Html partial, string? expression = null)
    {
        if (IsEven(html.Length))
            WriteImmutableMarkup(ref html, string.Empty);

        ref var keyhole = ref keyholes[html.Index + html.Length++];
        keyhole.Key = Cursor;
        // keyhole.RefId = parentStartIndex;
        keyhole.Type = FormatType.HtmlString;
        keyhole.String = expression + $" Index:{partial.Index} Length:{partial.Length}";

        // // Update the "starting end cap" to point its end.
        // ref var start = ref keyholes[parentStartIndex];
        // start.Integer = Cursor;

        return base.WriteMutableElement(ref html, partial);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsEven(int number)
    {
        return number % 2 == 0;
    }
}