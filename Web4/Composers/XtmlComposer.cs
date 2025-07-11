using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO.Pipelines;
using System.Text;
using Web4.Composers;

namespace Web4.Composers;

public class XtmlComposer(IBufferWriter<byte> writer, WindowBuilder window) : HtmlComposer(writer)
{
    private StableKeyTreeWalker keyGenerator = new();

    private enum AttributeStatus { None, Pending, InProgress }
    private AttributeStatus attributeStatus = AttributeStatus.None;
    private string? deferredLiteral = null;

    protected override void Clear()
    {
        attributeStatus = AttributeStatus.None;
        base.Clear();
    }

    public override void OnHtmlPartialBegins(ref Html html)
    {
        if (IsBeforeAppend())
        {
            html.Key = string.Empty;
            keyGenerator.Reset();
            keyGenerator.CreateNewGeneration(string.Empty, html.Length);
        }
        else
        {
            var key = keyGenerator.GetNextKey();
            html.Key = key;
            keyGenerator.CreateNewGeneration(key, html.Length);
        }

        if (attributeStatus == AttributeStatus.Pending)
        {
            HandleDeferredLiteral();
            Writer.Inject($"\"");
            attributeStatus = AttributeStatus.InProgress;
        }

        base.OnHtmlPartialBegins(ref html);
    }

    public override bool OnHtmlPartialEnds(ref Html parent, ref Html partial, string? format = null, string? expression = null)
    {
        switch (attributeStatus)
        {
            case AttributeStatus.None:
                if (partial.Key?.Length > 0)
                {
                    Writer.Inject($"""
                        <!--{partial.Key}-->
                        """);
                }
                break;

            case AttributeStatus.InProgress:
                Writer.Inject($"""
                    " {partial.Key}
                    """);
                attributeStatus = AttributeStatus.None;
                break;

            case AttributeStatus.Pending:
                throw new NotSupportedException("Attributes cannot have nested Htmls");
        }

        keyGenerator.ReturnToParent(parent.Key, parent.Cursor, parent.Length);
        return CompleteFormattedValue();
    }

    public override bool WriteImmutableMarkup(ref Html parent, string literal)
    {
        if (IsFinalAppend(literal) && TryInjectHttpXKernel(literal))
        {
            return true;
        }

        // This makes the assumption that keyholes preceeded with an '=' are 
        // always attributes.  Attributes need different sentinels than regular
        // keyholes and boolean attributes have a few strange rules to follow:
        // https://developer.mozilla.org/en-US/docs/Glossary/Boolean/HTML
        if (literal.EndsWith('='))
        {
            attributeStatus = AttributeStatus.Pending;
            deferredLiteral = literal;
            return CompleteStringLiteral(literal.Length);
        }

        return base.WriteImmutableMarkup(ref parent, literal);
    }

    public override bool WriteMutableValue(ref Html parent, string value) => WriteMutableString(ref parent, value);
    public override bool WriteMutableValue(ref Html parent, bool value) => WriteMutableBool(ref parent, value);
    public override bool WriteMutableValue(ref Html parent, Color value, string? format = null) => WriteMutableColor(ref parent, value, format);
    public override bool WriteMutableValue(ref Html parent, Uri value, string? format = null) => WriteMutableString(ref parent, value.ToString()); // TODO: Memory allocation!
    public override bool WriteMutableValue<T>(ref Html parent, T value, string? format = null)
        // where T : struct, IUtf8SpanFormattable // (from base)
    {
        // Wraps the mutable value with two comment tags
        // to separate it from any neighboring text.
        // At the end of the body an inline script registers them 
        // because we can't rely on id= or document.getElementById().
        // It should end up looking like this:
        // $"<!-- -->{value:format}<!--keyabc-->"

        // TODO: Does Writer.GetSpan() need a length?  What's the max length of all T's?

        var key = keyGenerator.GetNextKey();
        int length;
        switch (attributeStatus)
        {
            case AttributeStatus.None:
                Writer.Inject($"<!-- -->");

                value.TryFormat(Writer.GetSpan(), out length, format, null);
                Writer.Advance(length);

                Writer.Inject($"<!--{key}-->");
                break;

            case AttributeStatus.Pending:
                HandleDeferredLiteral();
                Writer.Inject($"\"");

                value.TryFormat(Writer.GetSpan(), out length, format, null);
                Writer.Advance(length);

                Writer.Inject($"""
                    " {key}
                    """);
                // status jumps from .Pending to .None because the whole 
                // attribute is just one value, not a bunch of keyholes+literals.
                attributeStatus = AttributeStatus.None;
                break;

            case AttributeStatus.InProgress:
                // No sentinels.  This keyhole is a part of a larger attribute
                // composed of multiple keyholes+literals.  Write only the value.
                value.TryFormat(Writer.GetSpan(), out length, format, null);
                Writer.Advance(length);
                break;
        }

        return CompleteFormattedValue();
    }

    private bool WriteMutableString(ref Html parent, string value)
    {
        // Strings have no format strings.

        var key = keyGenerator.GetNextKey();
        switch (attributeStatus)
        {
            case AttributeStatus.None:
                Writer.Inject($"""
                    <!-- -->{value}<!--{key}-->
                    """);
                break;

            case AttributeStatus.Pending:
                HandleDeferredLiteral();
                Writer.Inject($"""
                    "{value}" {key}
                    """);
                // status jumps from .Pending to .None because the whole 
                // attribute is just one value, not a bunch of keyholes+literals.
                attributeStatus = AttributeStatus.None;
                break;

            case AttributeStatus.InProgress:
                // No sentinels.  This keyhole is a part of a larger attribute
                // composed of multiple keyholes+literals.  Write only the value.
                Encoding.UTF8.GetBytes(value, Writer);
                break;
        }

        return CompleteFormattedValue();
    }

    private bool WriteMutableBool(ref Html parent, bool value)
    {
        var key = keyGenerator.GetNextKey();
        switch (attributeStatus)
        {
            case AttributeStatus.None:
                var b = value ? "true" : "false";
                Writer.Inject($"""
                    <!-- -->{b}<!--{key}-->
                    """);
                break;

            case AttributeStatus.Pending:
                var attributeName = HandleDeferredLiteral(isBooleanAttribute: true);
                if (value)
                {
                    Writer.Inject($"""
                         {attributeName} {key}="{attributeName}"
                        """);
                }
                else
                {
                    Writer.Inject($"""
                         {key}="{attributeName}"
                        """);
                }
                // status jumps from .Pending to .None because the whole 
                // attribute is just one value, not a bunch of keyholes+literals.
                attributeStatus = AttributeStatus.None;
                break;

            case AttributeStatus.InProgress:
                // No sentinels.  This keyhole is a part of a larger attribute
                // composed of multiple keyholes+literals.  Write only the value.
                Encoding.UTF8.GetBytes(value ? "true" : "false", Writer);
                break;
        }

        return CompleteFormattedValue();
    }

    private bool WriteMutableColor(ref Html parent, Color value, string? format = null)
    {
        var key = keyGenerator.GetNextKey();
        int length;
        switch (attributeStatus)
        {
            case AttributeStatus.None:
                Writer.Inject($"<!-- -->");

                value.TryFormat(Writer.GetSpan(), out length, format);
                Writer.Advance(length);

                Writer.Inject($"<!--{key}-->");
                break;

            case AttributeStatus.Pending:
                HandleDeferredLiteral();
                Writer.Inject($"\"");

                value.TryFormat(Writer.GetSpan(), out length, format);
                Writer.Advance(length);

                Writer.Inject($"""
                    " {key}
                    """);
                // status jumps from .Pending to .None because the whole 
                // attribute is just one value, not a bunch of keyholes+literals.
                attributeStatus = AttributeStatus.None;
                break;

            case AttributeStatus.InProgress:
                // No sentinels.  This keyhole is a part of a larger attribute
                // composed of multiple keyholes+literals.  Write only the value.
                value.TryFormat(Writer.GetSpan(), out length, format);
                Writer.Advance(length);
                break;
        }

        return CompleteFormattedValue();
    }

    private void HandleDeferredLiteral()
    {
        ArgumentNullException.ThrowIfNull(deferredLiteral);

        Encoding.UTF8.GetBytes(deferredLiteral, Writer);
    }

    private ReadOnlySpan<char> HandleDeferredLiteral(bool isBooleanAttribute = true)
    {
        if (!isBooleanAttribute)
        {
            HandleDeferredLiteral();
            return [];
        }

        ArgumentNullException.ThrowIfNull(deferredLiteral);

        // This string literal will look something like `...<input type="checkbox" checked=`
        // Note: We know they always end with `=`.
        int indexBeforeAttribute = deferredLiteral.LastIndexOf(' ');
        ArgumentOutOfRangeException.ThrowIfLessThan(indexBeforeAttribute, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(indexBeforeAttribute, deferredLiteral.Length - 2);

        Encoding.UTF8.GetBytes(deferredLiteral.AsSpan(..indexBeforeAttribute), Writer);
        return deferredLiteral.AsSpan((indexBeforeAttribute + 1)..^1);
    }

    public override bool WriteEventListener(ref Html parent, Action listener, string? format = null, string? expression = null) => WriteEventListener(ref parent, includeEventArg: false, format);
    public override bool WriteEventListener(ref Html parent, Action<Event> listener, string? format = null, string? expression = null) => WriteEventListener(ref parent, includeEventArg: true, format);
    public override bool WriteEventListener(ref Html parent, Func<Task> listener, string? format = null, string? expression = null) => WriteEventListener(ref parent, includeEventArg: false, format);
    public override bool WriteEventListener(ref Html parent, Func<Event, Task> listener, string? format = null, string? expression = null) => WriteEventListener(ref parent, includeEventArg: true, format);
    private bool WriteEventListener(ref Html parent, bool includeEventArg, string? format = null)
    {
        if (deferredLiteral != null)
            HandleDeferredLiteral();

        var key = keyGenerator.GetNextKey();
        if (includeEventArg && format != null)
        {
            Writer.Inject($"""
                "rpc('{key}',event,'{format ?? ""}')"
                """);
        }
        else if (includeEventArg && format == null)
        {
            Writer.Inject($"""
                "rpc('{key}',event)"
                """);
        }
        else
        {
            Writer.Inject($"""
                "rpc('{key}')"
                """);
        }
        
        attributeStatus = AttributeStatus.None;
        return CompleteFormattedValue();
    }

    public override bool WriteMutableElement<T>(ref Html parent, Html.Enumerable<T> partials, string? format = null, string? expression = null)
    {
        // TODO: Under the hood this calls `IEnumerable<T>.Count<T>()`.  If it does not
        // also implement ICollection then it will iterate in order to find the count
        // which will instantiate Html values too early thus breaking all the things.
        var itemCount = partials.Count;

        // Reserve a keyhole to represent the loop itself
        var key = keyGenerator.GetNextKey();

        int i = 0, index = keyGenerator.WriteHead;
        keyGenerator.CreateNewGeneration(key, itemCount);

        // Note: foreach calls `enumerator.Current` which creates new `Html`s which 
        // triggers `OnHtmlPartialBegins` and `OnHtmlPartialEnds` (above) to be called.
        var enumerator = partials.GetEnumerator();
        while (enumerator.MoveNext())
        {
            var current = enumerator.Current;

            keyGenerator.ReturnToParent(key, i * 2 - 1, itemCount);

            Writer.Inject($"""
                <!--{keyGenerator.GetNextKey()}-->
                """);

            i++;
        }

        keyGenerator.ReturnToParent(parent.Key, parent.Cursor, parent.Length);
        return CompleteFormattedValue();
    }

    // Fear not, this Dictionary will not grow unbounded.  
    // Since it's only ever called from AppendLiteral, that means the universe of
    // possible keys is finite - only what is already compiled into the executable.
    private static readonly Dictionary<string, int> jsInjectionPoint = [];
    private static readonly string KERNEL_JS = 
        new StreamReader(System.Reflection.Assembly
            .GetExecutingAssembly()
            .GetManifestResourceStream("Web4.Kernel.js")!
        )
        .ReadToEnd();
        // .Replace("\n", "")
        // .Replace("  ", "");

    private bool TryInjectHttpXKernel(string literal)
    {
        // If there are zero mutable keys, then we can skip this.
        if (FormattedCount <= 1)
        {
            return false;
        }

        // Inject the necessary JavaScript before the end of the </body> tag.
        if (!jsInjectionPoint.TryGetValue(literal, out int index))
        {
            // Avoid expensive operation?
            index = literal.IndexOf("</body>");
            jsInjectionPoint[literal] = index;
        }
        if (index >= 0)
        {
            var beforeBody = literal.AsSpan(0, index);
            var afterBody = literal.AsSpan(index, literal.Length - index);

            if (window.Listeners.Count > 0)
            {
                Writer.Inject($"{beforeBody}\n<!-- Web4 kernel injected -->\n<script>");
                foreach (var listener in window.Listeners)
                    Writer.Inject($"\n{listener.Html}");
                Writer.Inject($"\n\n{KERNEL_JS}{Debug.JS}</script>\n{afterBody}");
            }
            else
            {
                Writer.Inject($"{beforeBody}\n<!-- Web4 kernel injected -->\n<script>\n{KERNEL_JS}{Debug.JS}</script>\n{afterBody}");
            }


            CompleteStringLiteral(literal.Length);
            return true;
        }

        return false;
    }
}