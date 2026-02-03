namespace Web4.Dom
{
    namespace Events.Subsets
    {
        public interface IPageXY : ISubset, IView
        {
            new const string TRIM = "pageX,pageY";

            /// <summary>
            /// The X coordinate of the mouse pointer relative to the whole document.
            /// </summary>
            double PageX { get; }

            /// <summary>
            /// The Y coordinate of the mouse pointer relative to the whole document.
            /// </summary>
            double PageY { get; }
        }
    }
}