namespace Web4
{
    namespace Events.Subsets
    {
        public interface IModifierShift : ISubset, IView
        {
            new const string Format = "shiftKey";

            /// <summary>
            /// Returns true if the shift key was down when the event was fired.
            /// </summary>
            bool ShiftKey { get; }
        }
    }
}