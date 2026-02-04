namespace Web4.Dom
{
    namespace Events.Subsets
    {
        public interface IPreventDefaultSubset : ISubset
        {
            const string TRIM = "preventDefault";

            /// <summary>
            /// Cancels the event (if it is cancelable).
            /// </summary>
            void PreventDefault()
            {
            }
        }
    }
}