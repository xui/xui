namespace Web4.Dom
{
    namespace Events.Subsets
    {
        public interface IScreenXY : ISubset, IView
        {
            new const string TRIM = "screenX,screenY";

            /// <summary>
            /// The X coordinate of the mouse pointer in screen coordinates.
            /// </summary>
            double ScreenX { get; }

            /// <summary>
            /// The Y coordinate of the mouse pointer in screen coordinates.
            /// </summary>
            double ScreenY { get; }
        }
    }
}