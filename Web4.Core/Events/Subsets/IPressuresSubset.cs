using System.Runtime.CompilerServices;
using static Web4.Events.Aliases.Subsets;

namespace Web4
{
    namespace Events.Subsets
    {
        public interface IPressuresSubset
        {
            const string Format = "pressure,tangentialPressure";

            /// <summary>
            /// The normalized pressure of the pointer input in the range 0 to 1, 
            /// where 0 and 1 represent the minimum and maximum pressure the 
            /// hardware is capable of detecting, respectively.
            /// </summary>
            double Pressure { get; }

            /// <summary>
            /// The normalized tangential pressure of the pointer input 
            /// (also known as barrel pressure or cylinder stress) in the 
            /// range -1 to 1, where 0 is the neutral position of the control.
            /// </summary>
            double TangentialPressure { get; }
        }
    }

    public ref partial struct Html
    {
        public bool AppendFormatted(
            Action<Pressures> listener, 
            string? format = Pressures.Format, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);

        public bool AppendFormatted(
            Func<Pressures, Task> listener, 
            string? format = Pressures.Format, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);
    }
}