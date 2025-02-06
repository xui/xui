using System.Runtime.CompilerServices;

namespace Web4
{
    namespace Events
    {
        public partial interface OneLevelRemoved
        {
            public partial interface Subsets
            {
                public interface Pressures
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
        }
    }

    public ref partial struct Html
    {
        public bool AppendFormatted(
            Action<Event.Subsets.Pressures> listener, 
            string? format = Event.Subsets.Pressures.Format, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);

        public bool AppendFormatted(
            Func<Event.Subsets.Pressures, Task> listener, 
            string? format = Event.Subsets.Pressures.Format, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);
    }
}