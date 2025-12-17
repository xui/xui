using System.Buffers;
using System.Runtime.CompilerServices;

namespace Web4.Composers;

public abstract class ResultComposer<T> : BaseComposer
{
    public abstract T Result { get; }

    public new T Compose([InterpolatedStringHandlerArgument("")] Html html)
    {
        // ^ That's the root Html getting passed in above.
        // By the time you've reached this line, the work is done.
        T result = Result;
        html.Dispose();
        return result;
    }
}