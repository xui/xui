using System.Runtime.CompilerServices;
using static Web4.Events.Aliases.Subsets;

namespace Web4
{
    namespace Events.Subsets
    {
        public interface IWidthHeight : ISubset, IView
        {
            new const string Format = "width,height";

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

    public ref partial struct Html
    {
        public bool AppendFormatted(
            Action<WidthHeight> listener, 
            string? format = WidthHeight.Format, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);

        public bool AppendFormatted(
            Func<WidthHeight, Task> listener, 
            string? format = WidthHeight.Format, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);
    }
}