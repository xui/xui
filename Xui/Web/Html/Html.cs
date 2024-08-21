using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Xui.Web;

[InterpolatedStringHandler]
[StructLayout(LayoutKind.Auto)]
public partial struct Html
{
    readonly Composer composer;

    readonly int start;
    int end;

    int literalLengthRemaining;
    int formattedValuesRemaining;

    public Html(int literalLength, int formattedCount)
    {
        composer = Composer.Current ??= new();

        composer.depth++;
        start = composer.cursor;
        end = start;

        literalLengthRemaining = literalLength;
        formattedValuesRemaining = formattedCount;

        ref var chunk = ref composer.chunks[end];
        chunk.Id = end;
        chunk.Integer = start;
        chunk.Type = FormatType.HtmlString;
        end++;

        if (literalLength == 0 && formattedCount == 0)
        {
            Clear();
        }
    }

    private void MoveNext()
    {
        end++;
        composer.cursor = end;

        if (literalLengthRemaining == 0 && formattedValuesRemaining == 0)
        {
            Clear();
        }
    }

    private void Clear()
    {
        if (--composer.depth == 0)
        {
            composer.cursor = 0;
            composer.end = end;
            Composer.Current = null;
        }
    }

    public void AppendLiteral(string s)
    {
        ref var chunk = ref composer.chunks[end];
        chunk.Id = end;
        chunk.String = s;
        chunk.Integer = start;
        chunk.Type = FormatType.StringLiteral;

        literalLengthRemaining -= s.Length;
        MoveNext();
    }

    public void AppendFormatted(string s)
    {
        ref var chunk = ref composer.chunks[end];
        chunk.Id = end;
        chunk.String = s;
        chunk.Type = FormatType.String;
        chunk.Format = null;

        formattedValuesRemaining--;
        MoveNext();
    }

    public void AppendFormatted(int i, string? format = null)
    {
        ref var chunk = ref composer.chunks[end];
        chunk.Id = end;
        chunk.Integer = i;
        chunk.Type = FormatType.Integer;
        chunk.Format = format;

        formattedValuesRemaining--;
        MoveNext();
    }

    public void AppendFormatted(long l, string? format = null)
    {
        ref var chunk = ref composer.chunks[end];
        chunk.Id = end;
        chunk.Long = l;
        chunk.Type = FormatType.Long;
        chunk.Format = format;

        formattedValuesRemaining--;
        MoveNext();
    }

    public void AppendFormatted(float f, string? format = null)
    {
        ref var chunk = ref composer.chunks[end];
        chunk.Id = end;
        chunk.Float = f;
        chunk.Type = FormatType.Float;
        chunk.Format = format;

        formattedValuesRemaining--;
        MoveNext();
    }

    public void AppendFormatted(double d, string? format = null)
    {
        ref var chunk = ref composer.chunks[end];
        chunk.Id = end;
        chunk.Double = d;
        chunk.Type = FormatType.Double;
        chunk.Format = format;

        formattedValuesRemaining--;
        MoveNext();
    }

    public void AppendFormatted(decimal d, string? format = null)
    {
        ref var chunk = ref composer.chunks[end];
        chunk.Id = end;
        chunk.Decimal = d;
        chunk.Type = FormatType.Decimal;
        chunk.Format = format;

        formattedValuesRemaining--;
        MoveNext();
    }

    public void AppendFormatted(bool b)
    {
        ref var chunk = ref composer.chunks[end];
        chunk.Id = end;
        chunk.Boolean = b;
        chunk.Type = FormatType.Boolean;
        chunk.Format = null;

        formattedValuesRemaining--;
        MoveNext();
    }

    public void AppendFormatted(DateTime d, string? format = null)
    {
        ref var chunk = ref composer.chunks[end];
        chunk.Id = end;
        chunk.DateTime = d;
        chunk.Type = FormatType.DateTime;
        chunk.Format = format;

        formattedValuesRemaining--;
        MoveNext();
    }

    public void AppendFormatted(TimeSpan t, string? format = null)
    {
        ref var chunk = ref composer.chunks[end];
        chunk.Id = end;
        chunk.TimeSpan = t;
        chunk.Type = FormatType.TimeSpan;
        chunk.Format = format;

        formattedValuesRemaining--;
        MoveNext();
    }

    public void AppendFormatted<TView>(TView v) where TView : IView
    {
        AppendFormatted(v.Render());
    }

    public void AppendFormatted(Html h)
    {
        end = h.end;

        ref var chunk = ref composer.chunks[end];
        chunk.Id = end;
        chunk.Integer = h.start;
        chunk.Type = FormatType.HtmlString;

        ref var start = ref composer.chunks[h.start];
        start.Integer = end;

        formattedValuesRemaining--;
        MoveNext();
    }

    public void AppendFormatted(Func<Html> f)
    {
        formattedValuesRemaining--;
        MoveNext();
    }

    public void AppendFormatted(Func<string, Html> f)
    {
        formattedValuesRemaining--;
        MoveNext();
    }

    // public void AppendFormatted<T>(T t)
    // {
    // }

    public void AppendFormatted(Action a)
    {
        ref var chunk = ref composer.chunks[end];
        chunk.Id = end;
        chunk.Action = a;
        chunk.Type = FormatType.Action;

        formattedValuesRemaining--;
        MoveNext();
    }

    public void AppendFormatted(Action<Event> a)
    {
        ref var chunk = ref composer.chunks[end];
        chunk.Id = end;
        chunk.ActionEvent = a;
        chunk.Type = FormatType.ActionEvent;

        formattedValuesRemaining--;
        MoveNext();
    }

    public void AppendFormatted(Func<Task> f)
    {
        ref var chunk = ref composer.chunks[end];
        chunk.Id = end;
        chunk.ActionAsync = f;
        chunk.Type = FormatType.ActionAsync;

        formattedValuesRemaining--;
        MoveNext();
    }

    public void AppendFormatted(Func<Event, Task> f)
    {
        ref var chunk = ref composer.chunks[end];
        chunk.Id = end;
        chunk.ActionEventAsync = f;
        chunk.Type = FormatType.ActionEventAsync;

        formattedValuesRemaining--;
        MoveNext();
    }
}