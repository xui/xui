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

    private readonly bool Recurse(ref Keyhole parent, Span<Keyhole> oldSpan, Span<Keyhole> newSpan)
    {
        if (CompareLengths(ref parent, oldSpan, newSpan)) return false;
        if (CompareImmutables(ref parent, oldSpan, newSpan)) return false;
        if (CompareMutables(ref parent, oldSpan, newSpan)) return false;
        return false;
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
                keyholes.SetAttribute(
                    parent.Key,
                    newSpan
                );
            else if (parent.Format is null)
                keyholes.SetNode(
                    newBuffer,
                    parent.Key,
                    newSpan
                );
            else
                keyholes.SetNode(
                    newBuffer,
                    parent.Key,
                    newSpan,
                    (false ? "web4-rev-" : "web4-fwd-", parent.Key)
                ); // TODO: lineNumber logic here!

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
                    keyholes.SetAttribute(
                        parent.Key,
                        newSpan
                    );
                else if (parent.Format == null)
                    keyholes.SetNode(
                        newBuffer,
                        parent.Key,
                        newSpan
                    );
                else
                    keyholes.SetNode(
                        newBuffer,
                        parent.Key,
                        newSpan,
                        (false ? "web4-rev-" : "web4-fwd-", parent.Key)
                    ); // TODO: lineNumber logic here!

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

            bool shortCircuit = newKeyhole.Type switch
            {
                KeyholeType.String or
                KeyholeType.Boolean or
                KeyholeType.Color or
                KeyholeType.Uri or
                KeyholeType.Integer or
                KeyholeType.Long or
                KeyholeType.Float or
                KeyholeType.Double or
                KeyholeType.Decimal or
                KeyholeType.DateTime or
                KeyholeType.DateOnly or
                KeyholeType.TimeSpan or
                KeyholeType.TimeOnly =>
                    CompareMutable(ref parent, ref oldKeyhole, ref newKeyhole),
                KeyholeType.Enumerable =>
                    CompareEnumerable(ref parent, ref oldKeyhole, ref newKeyhole),
                KeyholeType.Html or
                KeyholeType.Attribute =>
                    Recurse(ref newKeyhole, oldBuffer.AsSpan(oldKeyhole.Sequence), newBuffer.AsSpan(newKeyhole.Sequence)),
                KeyholeType.EventListener =>
                    false, // Event listeners never need to be diff'd, their only purpose is for lookup.
                KeyholeType.StringLiteral =>
                    throw new InvalidOperationException("It should be impossible to find a StringLiteral in an odd index"),
                _ =>
                    throw new InvalidOperationException("Unnamed enum values are not supported")
            };

            if (shortCircuit)
                return true;
        }

        // Allow deeper recursion.
        return false;
    }

    private readonly bool CompareMutable(ref Keyhole parent, ref Keyhole oldKeyhole, ref Keyhole newKeyhole)
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
                keyholes.SetText(newKeyhole.Key, ref newKeyhole);
            }
        }

        // Allow deeper recursion.
        return false;
    }

    private readonly bool CompareEnumerable(ref Keyhole parent, ref Keyhole oldKeyhole, ref Keyhole newKeyhole)
    {
        var oldItems = oldBuffer.AsSpan(oldKeyhole.Sequence);
        var newItems = newBuffer.AsSpan(newKeyhole.Sequence);
        var minLength = Math.Min(oldItems.Length, newItems.Length);

        var tagChanges = oldItems.Length != newItems.Length ? 1 : 0;
        if (newKeyhole.Format is not null)
        {
            for (var d = 0; d < minLength; d++)
            {
                ref var oldItem = ref oldItems[d];
                ref var newItem = ref newItems[d];
                if (oldItem.Tag != newItem.Tag)
                    if (++tagChanges > 1)
                        break;
            }
        }
        bool shouldUseTransition = tagChanges > 1;

        for (var d = 0; d < minLength; d++)
        {
            ref var oldItem = ref oldItems[d];
            ref var newItem = ref newItems[d];

            if (shouldUseTransition && oldItem.Tag != newItem.Tag && oldItem.Tag is not null && newItem.Tag is not null)
            {
                var newPartial = newBuffer.AsSpan(newItem.Sequence);
                keyholes.SetNode(
                    newBuffer,
                    newItem.Key,
                    newPartial,
                    ("web4-move-", newItem.Tag.GetHashCode()),
                    ("web4-move-", oldItem.Tag.GetHashCode())
                );
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
                ref var newItem = ref newItems[d];
                var newItemSpan = newBuffer.AsSpan(newItem.Sequence);

                if (newKeyhole.Format is null || newItem.Tag is null)
                    keyholes.PushNode(newBuffer, newKeyhole.Key, newItemSpan, newItem.Key);
                else
                    keyholes.PushNode(newBuffer, newKeyhole.Key, newItemSpan, newItem.Key, ("web4-move-", newItem.Tag.GetHashCode()));
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
                if (newKeyhole.Format is null || item.Tag is null)
                    keyholes.PopNode(item.Key);
                else
                    keyholes.PopNode(item.Key, ("web4-move-", item.Tag.GetHashCode()));
            }
        }

        // Allow deeper recursion.
        return false;
    }
}
