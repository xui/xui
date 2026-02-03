namespace Web4.Dom
{
    namespace Events.Subsets
    {
        public interface IModifierAlt : ISubset, IView
        {
            new const string TRIM = "altKey";

            /// <summary>
            /// Returns true if the alt key was down when the event was fired.
            /// </summary>
            bool AltKey { get; }
        }
    }
}