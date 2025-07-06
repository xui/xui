using Web4.Transports;

namespace Web4;

public ref struct DiffUtil<T>(
    Keyhole[] bufferBefore,
    Keyhole[] bufferAfter)
        where T : struct, IMutationBatch, allows ref struct
{
    public readonly void Run(ref T mutationBatch)
    {
        using var perf = Debug.PerfCheck("DiffUtil.Run"); // TODO: Remove PerfCheck

        Span<Keyhole> partialBefore = bufferBefore.AsSpan(0, bufferBefore[0].Length);
        Span<Keyhole> partialAfter = bufferAfter.AsSpan(0, bufferAfter[0].Length);

        DiffPartials(ref mutationBatch, string.Empty, partialBefore, partialAfter);

        mutationBatch.Commit();
    }

    private readonly void DiffPartials(ref T mutationBatch, string key, Span<Keyhole> partialBefore, Span<Keyhole> partialAfter)
    {
        // The first thing to compare is the easiest (and fastest).
        // If the two partials have a different quantity of keyholes, 
        // then it's impossible that they are the same.  
        // So replace the whole partial.
        // Speed: fast!  One operation comparing two ints!
        if (partialBefore.Length != partialAfter.Length)
        {
            mutationBatch.UpdatePartial(key, partialBefore, partialAfter);
            return; // shortcircuit, no need to iterate through these partials or traverse deeper
        }

        // --- IMMUTABLES ---
        // Traverse every even index – these are guaranteed to be string literals only.
        // If any of the string literals do not match up that means this whole partial 
        // and all its children must be replaced.
        // Usually this is the result of switch-expressions or ternary-conditionals (?:):
        //   • $"<div>{ value switch { 1 => MyComponent1(), 2 => MyComponent2(), ... } }</div>"
        //   • $"<div>{ (condition ? MyComponent1() : MyComponent2()) }</div>" 
        // Caveat:
        // Don't forget about Hot Reload (DEBUG only) which can cheat the
        // compiler guarantees that InterpolatedStringHandlers gives us where
        // we can expect the exact same string literals at each keyhole every time.
        // Speed: fast!  One operation comparing two pointers!  
        for (int i = 0; i < partialAfter.Length; i += 2)
        {
            ref Keyhole keyholeBefore = ref partialBefore[i];
            ref Keyhole keyholeAfter = ref partialAfter[i];

            // Note: We must use `Object.ReferenceEquals` and NOT `str1 == str2` since the latter 
            // will try comparing char by char if the pointers don't match and thanks to the 
            // compiler guarantees around string literals, string interning, and InterpolatedStringHandler,
            // comparing pointers is enough to guarantee equality in this rare context. 
            // (This is especially helpful when the two strings are several kilobytes in length!)
            if (!Object.ReferenceEquals(keyholeBefore.StringLiteral, keyholeAfter.StringLiteral))
            {
                mutationBatch.UpdatePartial(key, partialBefore, partialAfter);
                return; // shortcircuit, no need to check mutables for changes or traverse deeper
            }
        }

        // --- MUTABLES ---
        // Traverse every odd index – these are guaranteed to be a mutable keyholes only.
        for (int i = 1; i < partialAfter.Length; i += 2)
        {
            ref Keyhole keyholeBefore = ref partialBefore[i];
            ref Keyhole keyholeAfter = ref partialAfter[i];

            // TODO: How much faster is it without the switch?  There's already a switch inside .Equals()!

            switch (keyholeAfter.Type)
            {
                case FormatType.String:
                case FormatType.Boolean:
                case FormatType.Color:
                case FormatType.Uri:
                case FormatType.Integer:
                case FormatType.Long:
                case FormatType.Float:
                case FormatType.Double:
                case FormatType.Decimal:
                case FormatType.DateTime:
                case FormatType.DateOnly:
                case FormatType.TimeSpan:
                case FormatType.TimeOnly:
                    if (!Keyhole.Equals(ref keyholeBefore, ref keyholeAfter))
                    {
                        mutationBatch.UpdateValue(key, ref keyholeBefore, ref keyholeAfter);
                    }
                    break;
                case FormatType.Html:
                    var childBefore = bufferBefore.AsSpan(keyholeBefore.ChildIndices);
                    var childAfter = bufferAfter.AsSpan(keyholeAfter.ChildIndices);
                    // Recursively traverse deeper, then come back and continue these siblings.
                    DiffPartials(ref mutationBatch, keyholeAfter.Key, childBefore, childAfter);
                    break;
                case FormatType.Attribute:
                case FormatType.EventListener:
                case FormatType.Enumerable:
                    // TODO: Implement
                    break;
                case FormatType.StringLiteral:
                    throw new InvalidOperationException("It should be impossible to find a StringLiteral in an odd index");
                default:
                    throw new InvalidOperationException("It should be impossible to find a non-mutable value in an odd index");
            }
        }
    }
}













public readonly partial struct Diff(Keyhole[] snapshotBefore, Keyhole[] snapshotAfter)
{
    // Note to self.  Perhaps this can be an enumerator
    // which contains not just .Current but actually both keyholes/indexes?
    // Oooooo, that would work so nicely with MoveNext() right?

    public IEnumerable<int> GetIndexes()
    {
        int index = 0;

        ref Keyhole beforeKeyhole = ref snapshotBefore[index];
        ref Keyhole afterKeyhole = ref snapshotAfter[index];

        int beforeLength = beforeKeyhole.Length;
        int afterLength = afterKeyhole.Length;

        if (beforeLength != afterLength)
        {
            yield return index;
            yield break;
        }

        int beforeStart = beforeKeyhole.Integer;
        int afterStart = afterKeyhole.Integer;

        foreach (int item in DiffPartials(beforeStart..beforeLength, afterStart..afterLength))
            yield return item;
    }

    private IEnumerable<int> DiffPartials(Range rangeBefore, Range rangeAfter)
    {
        int indexBefore = rangeBefore.Start.Value;
        int indexAfter = rangeAfter.Start.Value;
        for (; indexAfter < rangeAfter.End.Value; indexBefore++, indexAfter++)
        {
            ref Keyhole keyholeBefore = ref snapshotBefore[indexBefore];
            ref Keyhole keyholeAfter = ref snapshotAfter[indexAfter];

            switch (keyholeAfter.Type)
            {
                // TODO: Implement
                case FormatType.StringLiteral:
                    continue;
                case FormatType.String:
                case FormatType.Boolean:
                case FormatType.Color:
                case FormatType.Uri:
                case FormatType.Integer:
                case FormatType.Long:
                case FormatType.Float:
                case FormatType.Double:
                case FormatType.Decimal:
                case FormatType.DateTime:
                case FormatType.DateOnly:
                case FormatType.TimeSpan:
                case FormatType.TimeOnly:
                    if (!Keyhole.Equals(ref keyholeBefore, ref keyholeAfter))
                        yield return indexAfter;
                    continue;
                case FormatType.Html:
                    var childBefore = keyholeBefore.Integer..(keyholeBefore.Integer + keyholeBefore.Length);
                    var childAfter = keyholeAfter.Integer..(keyholeAfter.Integer + keyholeAfter.Length);
                    foreach (var item in DiffPartials(childBefore, childAfter))
                        yield return item;
                    continue;
                // TODO: Implement
                case FormatType.View:
                case FormatType.Attribute:
                case FormatType.EventListener:
                case FormatType.Enumerable:
                    continue;
            }
        }
    }
}