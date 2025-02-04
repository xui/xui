using System.Runtime.CompilerServices;

namespace Web4;

public partial interface Events
{
    public partial interface Subsets
    {
        public interface WidthHeight
        {
            const string Format = "width,height";

            /// <summary>
            /// The width (magnitude on the X axis), in CSS pixels, 
            /// of the contact geometry of the pointer.
            /// </summary>
            int Width { get; }

            /// <summary>
            /// The height (magnitude on the Y axis), in CSS pixels, 
            /// of the contact geometry of the pointer.
            /// </summary>
            int Height { get; }
        }
    }
}

public ref partial struct Html
{
    public bool AppendFormatted(
        Action<Event.Subsets.WidthHeight> listener, 
        string? format = Event.Subsets.WidthHeight.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Func<Event.Subsets.WidthHeight, Task> listener, 
        string? format = Event.Subsets.WidthHeight.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
}