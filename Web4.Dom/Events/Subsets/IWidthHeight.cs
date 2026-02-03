namespace Web4.Dom
{
    namespace Events.Subsets
    {
        public interface IWidthHeight : ISubset, IView
        {
            new const string TRIM = "width,height";

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