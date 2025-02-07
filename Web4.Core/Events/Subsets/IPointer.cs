using System.Runtime.CompilerServices;
using static Web4.Events.Aliases.Subsets;

namespace Web4
{
    namespace Events.Subsets
    {
        public interface IPointer
        {
            const string Format = "pointerID,twist,pointerType,isPrimary";

            /// <summary>
            /// A unique identifier for the pointer causing the event.
            /// </summary>
            int PointerID { get; }

            /// <summary>
            /// The clockwise rotation of the pointer (e.g. pen stylus) around 
            /// its major axis in degrees, with a value in the range 0 to 359.
            /// </summary>
            double Twist { get; }

            /// <summary>
            /// Indicates the device type that caused the event 
            /// (mouse, pen, touch, etc.).
            /// </summary>
            string PointerType { get; }

            /// <summary>
            /// Indicates if the pointer represents the primary pointer of 
            /// this pointer type.
            /// </summary>
            bool IsPrimary { get; }
        }
    }

    public ref partial struct Html
    {
        public bool AppendFormatted(
            Action<Pointer> listener, 
            string? format = Pointer.Format, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);

        public bool AppendFormatted(
            Func<Pointer, Task> listener, 
            string? format = Pointer.Format, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);
    }
}