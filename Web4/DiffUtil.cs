namespace Web4;

public ref struct DiffUtil(IKeyholes keyholes, Keyhole[] oldBuffer, Keyhole[] newBuffer)
{
    public static void Diff(IKeyholes keyholes, Keyhole[] oldBuffer, Keyhole[] newBuffer)
    {
        using var perf = Perf.Measure("DiffUtil.Diff"); // TODO: Remove PerfCheck

        ref Keyhole oldFirst = ref oldBuffer[0];
        ref Keyhole newFirst = ref newBuffer[0];
        Span<Keyhole> oldSpan = oldBuffer.AsSpan(..oldFirst.SequenceLength);
        Span<Keyhole> newSpan = newBuffer.AsSpan(..newFirst.SequenceLength);

        var diffUtil = new DiffUtil(keyholes, oldBuffer, newBuffer);
        diffUtil.Recurse(ref newFirst, oldSpan, newSpan);
    }

    private readonly void Recurse(ref Keyhole parent, Span<Keyhole> oldSpan, Span<Keyhole> newSpan)
    {
        if (CompareLengths(ref parent, oldSpan, newSpan)) return;
        if (CompareImmutables(ref parent, oldSpan, newSpan)) return;
        if (CompareMutables(ref parent, oldSpan, newSpan)) return;
    }

    /// <summary>
    /// The first thing to compare is the easiest (and fastest).
    /// If the two spans have a different quantity of keyholes, 
    /// then it's impossible that they are the same.  
    /// So replace the whole span.
    /// </summary>
    private readonly bool CompareLengths(ref Keyhole parent, Span<Keyhole> oldSpan, Span<Keyhole> newSpan)
    {
        if (oldSpan.Length != newSpan.Length)
        {
            if (parent.Type == KeyholeType.Attribute)
                keyholes.SetAttribute(parent.Key, newSpan);
            else if (parent.Format is null)
                keyholes.SetElement(newBuffer, parent.Key, newSpan);
            else
                keyholes.SetElement(newBuffer, parent.Key, newSpan, false); // TODO: lineNumber logic here!

            // Shortcircuit.  No need to finish diffing this span or traverse deeper
            // since this whole span (and possibly its children) will be sent to the browser.
            return true;
        }

        // Allow deeper recursion.
        return false;
    }

    /// <summary>
    /// IMMUTABLES (string literals)
    /// Traverse every even index – these are guaranteed to be string literals only.
    /// If any of the string literals do not match up that means this whole sequence 
    /// and all its children must be replaced.
    /// Usually this is the result of switch-expressions or ternary-conditionals:
    ///   • $"<div>{ value switch { 1 => MyComponent1(), 2 => MyComponent2(), ... } }</div>"
    ///   • $"<div>{ (condition ? MyComponent1() : MyComponent2()) }</div>" 
    /// Caveat: Don't forget about Hot Reload (DEBUG only) which can cheat the
    /// compiler guarantees that InterpolatedStringHandlers gives us where
    /// we can expect the exact same string literals at each keyhole every time.
    /// </summary>
    private readonly bool CompareImmutables(ref Keyhole parent, Span<Keyhole> oldSpan, Span<Keyhole> newSpan)
    {
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
                if (parent.Type == KeyholeType.Attribute)
                    keyholes.SetAttribute(parent.Key, newSpan);
                else if (parent.Format == null)
                    keyholes.SetElement(newBuffer, parent.Key, newSpan);
                else
                    keyholes.SetElement(newBuffer, parent.Key, newSpan, false); // TODO: lineNumber logic here!

                // Shortcircuit.  This whole segment (and possibly its children) will be replaced 
                // so there's no need to diff its mutables or traverse deeper.
                return true;
            }
        }

        // Allow deeper recursion.
        return false;
    }

    /// <summary>
    /// MUTABLES (state)
    /// Traverse every odd index – these are guaranteed to be a mutable keyhole value, i.e. state.
    /// </summary>
    private readonly bool CompareMutables(ref Keyhole parent, Span<Keyhole> oldSpan, Span<Keyhole> newSpan)
    {
        for (int i = 1; i < newSpan.Length; i += 2)
        {
            ref Keyhole oldKeyhole = ref oldSpan[i];
            ref Keyhole newKeyhole = ref newSpan[i];

            // TODO: How much faster is it without the switch?  There's already a switch inside .Equals()!

            switch (newKeyhole.Type)
            {
                case KeyholeType.Html:
                case KeyholeType.Attribute:
                    Recurse(ref newKeyhole, oldBuffer.AsSpan(oldKeyhole.Sequence), newBuffer.AsSpan(newKeyhole.Sequence));
                    break;
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
                    if (CompareMutableValues(ref parent, ref oldKeyhole, ref newKeyhole))
                        return true;
                    break;
                case KeyholeType.Enumerable:
                    if (CompareEnumerable(ref parent, ref oldKeyhole, ref newKeyhole))
                        return true;
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

        // Allow deeper recursion.
        return false;
    }

    private readonly bool CompareMutableValues(ref Keyhole parent, ref Keyhole oldKeyhole, ref Keyhole newKeyhole)
    {
        if (!Keyhole.Equals(ref oldKeyhole, ref newKeyhole))
        {
            if (parent.Type == KeyholeType.Attribute)
            {
                // This keyhole's value is part of a sequence of keyholes that comprises this attribute.
                // Find the start of this sequence, then grab the sequence's full span.
                ref var startKeyhole = ref newBuffer[newKeyhole.SequenceStart];
                keyholes.SetAttribute(parent.Key, newBuffer.AsSpan(startKeyhole.Sequence));

                // Shortcircuit.  No need to diff the rest of this span.
                // This whole attribute sequence will be updated.
                return true;
            }
            else if (newKeyhole.IsValueAnAttribute)
            {
                keyholes.SetAttribute(newKeyhole.Key, ref newKeyhole);
            }
            else
            {
                keyholes.SetTextNode(newKeyhole.Key, ref newKeyhole);
            }
        }

        // Allow deeper recursion.
        return false;
    }

    private readonly bool CompareEnumerable(ref Keyhole parent, ref Keyhole oldKeyhole, ref Keyhole newKeyhole)
    {
        var transition = newKeyhole.Format;
        var oldItems = oldBuffer.AsSpan(oldKeyhole.Sequence);
        var newItems = newBuffer.AsSpan(newKeyhole.Sequence);
        var minLength = Math.Min(oldItems.Length, newItems.Length);
        var tagChanges = 0;
        for (var d = 0; d < minLength; d++)
        {
            ref var oldItem = ref oldItems[d];
            ref var newItem = ref newItems[d];
            if (oldItem.Tag != newItem.Tag)
                if (++tagChanges > 1)
                    break;
        }

        for (var d = 0; d < minLength; d++)
        {
            ref var oldItem = ref oldItems[d];
            ref var newItem = ref newItems[d];

            if (tagChanges > 1 && oldItem.Tag != newItem.Tag && oldItem.Tag is not null && newItem.Tag is not null)
            {
                var newPartial = newBuffer.AsSpan(newItem.Sequence);
                keyholes.SetElement(newBuffer, newItem.Key, newPartial, newItem.Tag, oldItem.Tag);
            }
            else
            {
                Recurse(ref newItem, oldBuffer.AsSpan(oldItem.Sequence), newBuffer.AsSpan(newItem.Sequence));
            }
        }

        if (oldItems.Length < newItems.Length)
        {
            // The new enumerable has more items than the old one. 
            // These items can simply be appended to the end.  
            // This will not violate any keyhole's positional stability
            // or cause any keyname collisions.
            for (var d = minLength; d < newItems.Length; d++)
            {
                ref var priorItem = ref newItems[d - 1];
                ref var newItem = ref newItems[d];
                var newItemSpan = newBuffer.AsSpan(newItem.Sequence);

                keyholes.AddElement(newBuffer, priorItem.Key, newItem.Key, newItemSpan, transition);
            }
        }
        else if (oldItems.Length > newItems.Length)
        {
            // The old enumerable has more items than the new one. 
            // These items can simply be removed.  
            // This will not violate any keyhole's positional stability.
            for (var d = minLength; d < oldItems.Length; d++)
            {
                ref var item = ref oldItems[d];
                keyholes.RemoveElement(item.Key, transition);
            }
        }

        // TODO: Handle when oldItems.Length = 0.  
        // Looks like it will need to resemble <if> where it drops in a placeholder.

        // Allow deeper recursion.
        return false;
    }
}
