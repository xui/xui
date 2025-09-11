namespace Web4;

public ref struct DiffUtil(Keyhole[] oldBuffer, Keyhole[] newBuffer)
{
    public static T CreateBatch<T>(Keyhole[] oldBuffer, Keyhole[] newBuffer)
        where T : struct, IMutationBatch, allows ref struct
    {
        using var perf = Debug.PerfCheck("DiffUtil.CreateBatch"); // TODO: Remove PerfCheck

        ref Keyhole oldFirst = ref oldBuffer[0];
        ref Keyhole newFirst = ref newBuffer[0];
        Span<Keyhole> oldSpan = oldBuffer.AsSpan(..oldFirst.SequenceLength);
        Span<Keyhole> newSpan = newBuffer.AsSpan(..newFirst.SequenceLength);

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
        bool isSpanAnAttribute = false,
        string? transition = null)
            where T : struct, IMutationBatch, allows ref struct
    {
        // The first thing to compare is the easiest (and fastest).
        // If the two spans have a different quantity of keyholes, 
        // then it's impossible that they are the same.  
        // So replace the whole span.
        if (oldSpan.Length != newSpan.Length)
        {
            if (isSpanAnAttribute)
                mutationBatch.SetAttribute(key, oldSpan, newSpan);
            else
                mutationBatch.SetElement(key, oldSpan, newSpan, transition);

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
                    mutationBatch.SetAttribute(key, oldSpan, newSpan);
                else
                    mutationBatch.SetElement(key, oldSpan, newSpan, transition);
                
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
                            // This keyhole's value is part of a sequence of keyholes that comprises this attribute.
                            // Find the start of this sequence, then grab the sequence's full span.
                            ref var oldStart = ref oldBuffer[oldKeyhole.SequenceStart];
                            ref var newStart = ref newBuffer[newKeyhole.SequenceStart];
                            mutationBatch.SetAttribute(
                                key,
                                oldKeyholes: oldBuffer.AsSpan(oldStart.Sequence),
                                newKeyholes: newBuffer.AsSpan(newStart.Sequence)
                            );

                            // Shortcircuit.  No need to diff the rest of this span.
                            // This whole attribute sequence will be updated.
                            return;
                        }
                        else if (newKeyhole.IsValueAnAttribute)
                        {
                            mutationBatch.SetAttribute(ref oldKeyhole, ref newKeyhole);
                        }
                        else
                        {
                            mutationBatch.SetTextNode(ref oldKeyhole, ref newKeyhole);
                        }
                    }
                    break;
                case KeyholeType.Html:
                case KeyholeType.Attribute:
                    // Recursively traverse deeper, then come back and continue these siblings.
                    DiffKeyholeSpans(
                        ref mutationBatch,
                        key: newKeyhole.Key,
                        oldSpan: oldBuffer.AsSpan(oldKeyhole.Sequence),
                        newSpan: newBuffer.AsSpan(newKeyhole.Sequence),
                        isSpanAnAttribute: newKeyhole.Type == KeyholeType.Attribute,
                        transition: newKeyhole.Format
                    );
                    break;
                case KeyholeType.Enumerable:
                    var oldItems = oldBuffer.AsSpan(oldKeyhole.Sequence);
                    var newItems = newBuffer.AsSpan(newKeyhole.Sequence);
                    transition = newKeyhole.Format;
                    int minLength = Math.Min(oldItems.Length, newItems.Length);
                    for (int d = 0; d < minLength; d++)
                    {
                        ref var oldItem = ref oldItems[d];
                        ref var newItem = ref newItems[d];
                        if (oldItem.Tag != newItem.Tag)
                        {
                            // Resend all items.  Tags not matching could be an indication of 
                            // something added, something removed, or something moved.  
                            // Instead of running Myers diff algorithm (costly) and manually
                            // re-mapping every keyhole (which must remain positionally stable), 
                            // this work can be offloaded to the browser via its View Transitions API.

                            // mutationBatch.SetElement(key, oldSpan, newSpan, transition);
                            return;
                        }
                    }
                    
                    if (oldItems.Length < newItems.Length)
                    {
                        // The new enumerable has more items than the old one. 
                        // These items can simply be appended to the end.  
                        // This will not violate any keyhole's positional stability
                        // or cause any keyname collisions.
                        for (int d = minLength; d < newItems.Length; d++)
                        {
                            ref var priorItem = ref newItems[d - 1];
                            ref var newItem = ref newItems[d];
                            var newItemSpan = newBuffer.AsSpan(newItem.Sequence);

                            mutationBatch.AddElement(newItem.Key, priorItem.Key, newItemSpan, transition);
                        }
                    }
                    else if (oldItems.Length > newItems.Length)
                    {
                        // The old enumerable has more items than the new one. 
                        // These items can simply be removed.  
                        // This will not violate any keyhole's positional stability.
                        for (int d = minLength; d < oldItems.Length; d++)
                        {
                            ref var item = ref oldItems[d];
                            mutationBatch.RemoveElement(item.Key, transition);
                        }
                    }
                    
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
