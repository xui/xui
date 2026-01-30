namespace Web4
{
    namespace Events.Subsets
    {
        public interface IPreventDefault : ISubset
        {
            const string Format = "preventDefault";

            /// <summary>
            /// Cancels the event (if it is cancelable).
            /// </summary>
            void PreventDefault()
            {
            }
        }
    }
}