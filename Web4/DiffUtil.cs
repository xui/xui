using Web4.Transports;

namespace Web4;

public ref struct DiffUtil(Keyhole[] oldBuffer, Keyhole[] newBuffer)
{
    public static T CreateBatch<T>(Keyhole[] oldBuffer, Keyhole[] newBuffer)
        where T : struct, IMutationBatch, allows ref struct
    {
        using var perf = Debug.PerfCheck("DiffUtil.CreateBatch"); // TODO: Remove PerfCheck

        ref Keyhole oldFirst = ref oldBuffer[0];
        ref Keyhole newFirst = ref newBuffer[0];
        Span<Keyhole> oldSpan = oldBuffer.AsSpan(..oldFirst.KeyholeCount);
        Span<Keyhole> newSpan = newBuffer.AsSpan(..newFirst.KeyholeCount);

        T mutationBatch = default(T);
        var diffUtil = new DiffUtil(oldBuffer, newBuffer);
        diffUtil.DiffKeyholeSpans(ref mutationBatch, string.Empty, oldSpan, newSpan);
        mutationBatch.Commit();

        return mutationBatch;
    }

    private readonly void DiffKeyholeSpans<T>(
        ref T mutationBatch,
        string key,
        Span<Keyhole> oldSpan,
        Span<Keyhole> newSpan,
        bool isSpanAnAttribute = false)
            where T : struct, IMutationBatch, allows ref struct
    {
        // The first thing to compare is the easiest (and fastest).
        // If the two spans have a different quantity of keyholes, 
        // then it's impossible that they are the same.  
        // So replace the whole span.
        if (oldSpan.Length != newSpan.Length)
        {
            if (isSpanAnAttribute)
                mutationBatch.UpdateAttribute(key, oldSpan, newSpan);
            else
                mutationBatch.UpdatePartial(key, oldSpan, newSpan);

            // Shortcircuit.  No need to finish diffing this span or traverse deeper
            // since this whole span (and possibly its children) will be sent to the browser.
            return;
        }

        // --- IMMUTABLES (string literals) ---
        // Traverse every even index – these are guaranteed to be string literals only.
        // If any of the string literals do not match up that means this whole sequence 
        // and all its children must be replaced.
        // Usually this is the result of switch-expressions or ternary-conditionals:
        //   • $"<div>{ value switch { 1 => MyComponent1(), 2 => MyComponent2(), ... } }</div>"
        //   • $"<div>{ (condition ? MyComponent1() : MyComponent2()) }</div>" 
        // Caveat: Don't forget about Hot Reload (DEBUG only) which can cheat the
        // compiler guarantees that InterpolatedStringHandlers gives us where
        // we can expect the exact same string literals at each keyhole every time.
        for (int i = 0; i < newSpan.Length; i += 2)
        {
            ref Keyhole oldKeyhole = ref oldSpan[i];
            ref Keyhole newKeyhole = ref newSpan[i];

            // Note: We must use `Object.ReferenceEquals` and NOT `str1 == str2` since the latter 
            // will try comparing char by char if the pointers don't match.  Thanks to the 
            // compiler guarantees around string literals, string interning, and InterpolatedStringHandler,
            // comparing pointers is enough to guarantee equality in this rare context. 
            // (This is especially helpful when the two strings are several kilobytes in length!)
            if (!Object.ReferenceEquals(oldKeyhole.StringLiteral, newKeyhole.StringLiteral))
            {
                if (isSpanAnAttribute)
                    mutationBatch.UpdateAttribute(key, oldSpan, newSpan);
                else
                    mutationBatch.UpdatePartial(key, oldSpan, newSpan);
                
                // Shortcircuit.  This whole segment (and possibly its children) will be replaced 
                // so there's no need to diff its mutables or traverse deeper.
                return;
            }
        }

        // --- MUTABLES (state) ---
        // Traverse every odd index – these are guaranteed to be a mutable keyholes only.
        for (int i = 1; i < newSpan.Length; i += 2)
        {
            ref Keyhole oldKeyhole = ref oldSpan[i];
            ref Keyhole newKeyhole = ref newSpan[i];

            // TODO: How much faster is it without the switch?  There's already a switch inside .Equals()!

            switch (newKeyhole.Type)
            {
                case KeyholeType.String:
                case KeyholeType.Boolean:
                case KeyholeType.Color:
                case KeyholeType.Uri:
                case KeyholeType.Integer:
                case KeyholeType.Long:
                case KeyholeType.Float:
                case KeyholeType.Double:
                case KeyholeType.Decimal:
                case KeyholeType.DateTime:
                case KeyholeType.DateOnly:
                case KeyholeType.TimeSpan:
                case KeyholeType.TimeOnly:
                    if (!Keyhole.Equals(ref oldKeyhole, ref newKeyhole))
                    {
                        if (isSpanAnAttribute)
                        {
                            Span<Keyhole> oldAttr = oldKeyhole.GetAttributeSpan(oldBuffer);
                            Span<Keyhole> newAttr = newKeyhole.GetAttributeSpan(newBuffer);
                            mutationBatch.UpdateAttribute(key, oldAttr, newAttr);
                            return; // Return early.  This whole attribute will be updated.
                        }
                        else if (newKeyhole.IsAttributeValue)
                        {
                            mutationBatch.UpdateAttribute(key, ref oldKeyhole, ref newKeyhole);
                        }
                        else
                        {
                            mutationBatch.UpdateValue(key, ref oldKeyhole, ref newKeyhole);
                        }
                    }
                    break;
                case KeyholeType.Html:
                case KeyholeType.Attribute:
                    var oldKeyholes = oldBuffer.AsSpan(oldKeyhole.HtmlRange);
                    var newKeyholes = newBuffer.AsSpan(newKeyhole.HtmlRange);
                    var isAttribute = newKeyhole.Type == KeyholeType.Attribute;
                    // Recursively traverse deeper, then come back and continue these siblings.
                    DiffKeyholeSpans(ref mutationBatch, newKeyhole.Key, oldKeyholes, newKeyholes, isAttribute);
                    break;
                case KeyholeType.Enumerable:
                    // TODO: Implement
                    break;
                case KeyholeType.EventListener:
                    // Event listeners never need to be diff'd.  
                    // Their only purpose is for lookup.
                    break;
                case KeyholeType.StringLiteral:
                    throw new InvalidOperationException("It should be impossible to find a StringLiteral in an odd index");
                default:
                    throw new InvalidOperationException("KeyholeType not supported.  This is very unexpected.");
            }
        }
    }
}
