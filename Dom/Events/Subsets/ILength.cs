namespace Web4
{
    namespace Events.Subsets
    {
        public interface ILength : ISubset, IView
        {
            new const string Format = "length";

            /// <summary>
            /// Returns an integer representing the number of data items stored in the Storage object.
            /// </summary>
            int Length { get; }
        }
    }
}