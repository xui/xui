using System.Runtime.CompilerServices;

namespace Web4;

public partial interface Events
{
    public partial interface Subsets
    {
        public interface Target<T> where T : unmanaged, IParsable<T>
        {
            const string Format = "target";

            /// <summary>
            /// A reference to the object to which the event was originally dispatched.
            /// </summary>
            EventTarget<T> Target { get; }
        }
    }
}

public ref partial struct Html
{
    public bool AppendFormatted(
        Func<Event.Subsets.Target<int>, int> listener, 
        string? format = Event.Subsets.Target<int>.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendAmbiguous(GetArgName(expression), listener, listener, format, expression);

    public bool AppendFormatted(
        Func<Event.Subsets.Target<long>, long> listener, 
        string? format = Event.Subsets.Target<long>.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendAmbiguous(GetArgName(expression), listener, listener, format, expression);

    public bool AppendFormatted(
        Func<Event.Subsets.Target<float>, float> listener, 
        string? format = Event.Subsets.Target<float>.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendAmbiguous(GetArgName(expression), listener, listener, format, expression);

    public bool AppendFormatted(
        Func<Event.Subsets.Target<double>, double> listener, 
        string? format = Event.Subsets.Target<double>.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendAmbiguous(GetArgName(expression), listener, listener, format, expression);

    public bool AppendFormatted(
        Func<Event.Subsets.Target<decimal>, decimal> listener, 
        string? format = Event.Subsets.Target<decimal>.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendAmbiguous(GetArgName(expression), listener, listener, format, expression);

    public bool AppendFormatted(
        Func<Event.Subsets.Target<DateTime>, DateTime> listener, 
        string? format = Event.Subsets.Target<DateTime>.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendAmbiguous(GetArgName(expression), listener, listener, format, expression);

    // Note: Browsers only return dates, no TimeSpans.
}