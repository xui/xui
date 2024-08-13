using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Xui.Web.Html;

[InterpolatedStringHandler]
[StructLayout(LayoutKind.Auto)]
public partial struct HtmlString
{
    [ThreadStatic] static Composition? root;

    readonly int start;
    int end;

    int literalLengthRemaining;
    int formattedValuesRemaining;

    public HtmlString(int literalLength, int formattedCount)
    {
        root ??= new();
        composition = root;

        composition.depth++;
        start = composition.cursor;
        end = start;

        literalLengthRemaining = literalLength;
        formattedValuesRemaining = formattedCount;

        ref var chunk = ref composition.chunks[end];
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
        composition.cursor = end;

        if (literalLengthRemaining == 0 && formattedValuesRemaining == 0)
        {
            Clear();
        }
    }

    private void Clear()
    {
        if (--composition.depth == 0)
        {
            composition.cursor = 0;
            root = null;
        }
    }

    public void AppendLiteral(string s)
    {
        ref var chunk = ref composition.chunks[end];
        chunk.Id = end;
        chunk.String = s;
        chunk.Integer = start;
        chunk.Type = FormatType.StringLiteral;

        literalLengthRemaining -= s.Length;
        MoveNext();
    }

    public void AppendFormatted(string s)
    {
        ref var chunk = ref composition.chunks[end];
        chunk.Id = end;
        chunk.String = s;
        chunk.Type = FormatType.String;
        chunk.Format = null;

        formattedValuesRemaining--;
        MoveNext();
    }

    public void AppendFormatted(int i, string? format = null)
    {
        ref var chunk = ref composition.chunks[end];
        chunk.Id = end;
        chunk.Integer = i;
        chunk.Type = FormatType.Integer;
        chunk.Format = format;

        formattedValuesRemaining--;
        MoveNext();
    }

    public void AppendFormatted(double d, string? format = null)
    {
        ref var chunk = ref composition.chunks[end];
        chunk.Id = end;
        chunk.Double = d;
        chunk.Type = FormatType.Double;
        chunk.Format = format;

        formattedValuesRemaining--;
        MoveNext();
    }

    public void AppendFormatted(bool b)
    {
        ref var chunk = ref composition.chunks[end];
        chunk.Id = end;
        chunk.Boolean = b;
        chunk.Type = FormatType.Boolean;
        chunk.Format = null;

        formattedValuesRemaining--;
        MoveNext();
    }

    public void AppendFormatted(DateTime d, string? format = null)
    {
        ref var chunk = ref composition.chunks[end];
        chunk.Id = end;
        chunk.DateTime = d;
        chunk.Type = FormatType.DateTime;
        chunk.Format = format;

        formattedValuesRemaining--;
        MoveNext();
    }

    public void AppendFormatted<TView>(TView v) where TView : IView
    {
        AppendFormatted(v.Render());
    }

    public void AppendFormatted(HtmlString h)
    {
        end = h.end;

        ref var chunk = ref composition.chunks[end];
        chunk.Id = end;
        chunk.Integer = h.start;
        chunk.Type = FormatType.HtmlString;

        ref var start = ref composition.chunks[h.start];
        start.Integer = end;

        formattedValuesRemaining--;
        MoveNext();
    }

    public void AppendFormatted(Func<HtmlString> f)
    {
        formattedValuesRemaining--;
        MoveNext();
    }

    public void AppendFormatted(Func<string, HtmlString> f)
    {
        formattedValuesRemaining--;
        MoveNext();
    }

    // public void AppendFormatted<T>(T t)
    // {
    // }

    public void AppendFormatted(Action a)
    {
        ref var chunk = ref composition.chunks[end];
        chunk.Id = end;
        chunk.Action = a;
        chunk.Type = FormatType.Action;

        formattedValuesRemaining--;
        MoveNext();
    }

    public void AppendFormatted(Action<Event> a)
    {
        ref var chunk = ref composition.chunks[end];
        chunk.Id = end;
        chunk.ActionEvent = a;
        chunk.Type = FormatType.ActionEvent;

        formattedValuesRemaining--;
        MoveNext();
    }

    public void AppendFormatted(Func<Task> f)
    {
        ref var chunk = ref composition.chunks[end];
        chunk.Id = end;
        chunk.ActionAsync = f;
        chunk.Type = FormatType.ActionAsync;

        formattedValuesRemaining--;
        MoveNext();
    }

    public void AppendFormatted(Func<Event, Task> f)
    {
        ref var chunk = ref composition.chunks[end];
        chunk.Id = end;
        chunk.ActionEventAsync = f;
        chunk.Type = FormatType.ActionEventAsync;

        formattedValuesRemaining--;
        MoveNext();
    }

    public readonly IDisposable ReuseBuffer()
    {
        return new Reuseable(composition);
    }

    private class Reuseable : IDisposable
    {
        public Reuseable(Composition composition)
        {
            root = composition;
        }

        public void Dispose()
        {
            root = null;
        }
    }
}