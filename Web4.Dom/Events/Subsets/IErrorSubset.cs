namespace Web4.Dom
{
    namespace Events.Subsets
    {
        public interface IErrorSubset : ISubset, IViewSubset
        {
            new const string TRIM = "error";

            string Message { get; }
            string FileName { get; }
            int LineNo { get; }
            int ColNo { get; }
            DOMException Error { get; }
        }

        public record struct DOMException(
            string Name = "",
            string Message = ""
        ) {
            public static readonly DOMException Empty = new();
        }
    }
}