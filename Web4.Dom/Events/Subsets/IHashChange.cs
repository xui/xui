namespace Web4.Dom
{
    namespace Events.Subsets
    {
        public interface IHashChange : ISubset, IView
        {
            new const string Format = "newUrl,oldUrl";

            /// <summary>
            /// The new URL to which the window is navigating.
            /// </summary>
            string NewUrl { get; }

            /// <summary>
            /// The previous URL from which the window was navigated.
            /// </summary>
            string OldUrl { get; }

        }
    }
}