namespace Web4.Dom
{
    namespace Events.Subsets
    {
        public interface IXY : ISubset, IView
        {
            new const string Format = "x,y";

            /// <summary>
            /// Alias for MouseEvent.clientX.
            /// </summary>
            double X { get; }

            /// <summary>
            /// Alias for MouseEvent.clientY.
            /// </summary>
            double Y { get; }
        }
    }
}