using System.Buffers;
using System.Runtime.CompilerServices;

namespace Web4.Composers;

public abstract class ResultComposer<T> : BaseComposer
{
    protected abstract T Result { get; }

    public new T Compose([InterpolatedStringHandlerArgument("")] Html html)
    {
        // ^ That's the root Html getting passed in above.
        // By the time you've reached this line, the templating work has already completed.
        
        // Hang onto the result before html.Dispose() resets this class.
        T result = Result;

        // Calls composer.Reset()
        html.Dispose();

        // Do something interesting with the result.
        return result;
    }
}