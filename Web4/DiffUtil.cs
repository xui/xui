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
                    if (!Keyhole.Equals(ref keyholeBefore, ref keyholeAfter))
                    {
                        if (!keyholeAfter.IsMemberOfHtmlAttribute)
                        {
                            mutationBatch.UpdateValue(key, ref keyholeBefore, ref keyholeAfter);
                        }
                        else
                        {
                            Span<Keyhole> attrBefore = GetAttributeSpan(ref keyholeBefore, bufferBefore);
                            Span<Keyhole> attrAfter = GetAttributeSpan(ref keyholeAfter, bufferAfter);
                            mutationBatch.UpdateAttribute(key, attrBefore, attrAfter);
                            return; // Return early.  This whole attribute will be updated.
                        }
                    }
                    break;
                case KeyholeType.Html:
                case KeyholeType.Attribute:
                    Span<Keyhole> keyholesBefore = bufferBefore.AsSpan(keyholeBefore.ChildIndices);
                    Span<Keyhole> keyholesAfter = bufferAfter.AsSpan(keyholeAfter.ChildIndices);
                    // Recursively traverse deeper, then come back and continue these siblings.
                    DiffPartials(ref mutationBatch, keyholeAfter.Key, keyholesBefore, keyholesAfter);
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
                    throw new InvalidOperationException("It should be impossible to find a non-mutable value in an odd index");
            }
        }
    }

    private static Span<Keyhole> GetAttributeSpan(ref Keyhole keyhole, Keyhole[] buffer)
    {
        var start = keyhole.AttributeStartIndex;
        ref var startKeyhole = ref buffer[start];
        var end = start + startKeyhole.Length;
        return buffer.AsSpan(start..end);
    }
}
