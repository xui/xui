using System.Runtime.CompilerServices;
using static Web4.Events.Aliases.Subsets;

namespace Web4
{
    namespace Events.Subsets
    {
        public interface IXY : X, Y
        {
            new const string Format = Event.Subsets.X.Format + "," + Event.Subsets.Y.Format;
        }
    }

    public ref partial struct Html
    {
        public bool AppendFormatted(
            Action<XY> listener, 
            string? format = XY.Format, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);

        public bool AppendFormatted(
            Func<XY, Task> listener, 
            string? format = XY.Format, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);
    }
}