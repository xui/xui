namespace Web4
{
    namespace Events.Subsets
    {
        public interface IOffsetXY : ISubset, IView
        {
            new const string Format = "offsetX,offsetY";
            
            /// <summary>
            /// The X coordinate of the mouse pointer relative to the position of the padding edge of the target node.
            /// </summary>
            double OffsetX { get; }

            /// <summary>
            /// The Y coordinate of the mouse pointer relative to the position of the padding edge of the target node.
            /// </summary>
            double OffsetY { get; }
        }
    }
}