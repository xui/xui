using Web4.Transports;

namespace Web4;

public ref struct DiffUtil(Keyhole[] bufferBefore, Keyhole[] bufferAfter)
{
    public static T CreateBatch<T>(Keyhole[] bufferBefore, Keyhole[] bufferAfter)
        where T : struct, IMutationBatch, allows ref struct
    {
        using var perf = Debug.PerfCheck("DiffUtil.CreateBatch"); // TODO: Remove PerfCheck

        ref Keyhole firstBefore = ref bufferBefore[0];
        ref Keyhole firstAfter = ref bufferAfter[0];
        Span<Keyhole> partialBefore = bufferBefore.AsSpan(..firstBefore.KeyholeCount);
        Span<Keyhole> partialAfter = bufferAfter.AsSpan(..firstAfter.KeyholeCount);

        T mutationBatch = default(T);
        var diffUtil = new DiffUtil(bufferBefore, bufferAfter);
        diffUtil.DiffKeyholeSpans(ref mutationBatch, string.Empty, partialBefore, partialAfter);
        mutationBatch.Commit();

        return mutationBatch;
    }

    private readonly void DiffKeyholeSpans<T>(
        ref T mutationBatch,
        string key,
        Span<Keyhole> partialBefore,
        Span<Keyhole> partialAfter,
        bool isSpanAnAttribute = false)
            where T : struct, IMutationBatch, allows ref struct
    {
        // The first thing to compare is the easiest (and fastest).
        // If the two spans have a different quantity of keyholes, 
        // then it's impossible that they are the same.  
        // So replace the whole span.
        // Speed: fast!  One operation comparing two ints!
        if (partialBefore.Length != partialAfter.Length)
        {
            if (isSpanAnAttribute)
                mutationBatch.UpdateAttribute(key, partialBefore, partialAfter);
            else
                mutationBatch.UpdatePartial(key, partialBefore, partialAfter);
            return; // shortcircuit, no need to iterate through these partials or traverse deeper
        }

        // --- IMMUTABLES (string literals) ---
        // Traverse every even index – these are guaranteed to be string literals only.
        // If any of the string literals do not match up that means this whole sequence 
        // and all its children must be replaced.
        // Usually this is the result of switch-expressions or ternary-conditionals (?:):
        //   • $"<div>{ value switch { 1 => MyComponent1(), 2 => MyComponent2(), ... } }</div>"
        //   • $"<div>{ (condition ? MyComponent1() : MyComponent2()) }</div>" 
        // Caveat: Don't forget about Hot Reload (DEBUG only) which can cheat the
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
                if (isSpanAnAttribute)
                    mutationBatch.UpdateAttribute(key, partialBefore, partialAfter);
                else
                    mutationBatch.UpdatePartial(key, partialBefore, partialAfter);
                return; // shortcircuit, no need to check mutables for changes or traverse deeper
            }
        }

        // --- MUTABLES (state) ---
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
                        if (isSpanAnAttribute)
                        {
                            Span<Keyhole> attrBefore = keyholeBefore.GetAttributeSpan(bufferBefore);
                            Span<Keyhole> attrAfter = keyholeAfter.GetAttributeSpan(bufferAfter);
                            mutationBatch.UpdateAttribute(key, attrBefore, attrAfter);
                            return; // Return early.  This whole attribute will be updated.
                        }
                        else if (keyholeAfter.IsAttributeValue)
                        {
                            mutationBatch.UpdateAttribute(key, ref keyholeBefore, ref keyholeAfter);
                        }
                        else
                        {
                            mutationBatch.UpdateValue(key, ref keyholeBefore, ref keyholeAfter);
                        }
                    }
                    break;
                case KeyholeType.Html:
                case KeyholeType.Attribute:
                    Span<Keyhole> keyholesBefore = bufferBefore.AsSpan(keyholeBefore.HtmlRange);
                    Span<Keyhole> keyholesAfter = bufferAfter.AsSpan(keyholeAfter.HtmlRange);
                    var isAttribute = keyholeAfter.Type == KeyholeType.Attribute;
                    // Recursively traverse deeper, then come back and continue these siblings.
                    DiffKeyholeSpans(ref mutationBatch, keyholeAfter.Key, keyholesBefore, keyholesAfter, isAttribute);
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
}
