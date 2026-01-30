namespace Web4
{
    namespace Events.Subsets
    {
        public interface IError : ISubset, IView
        {
            new const string Format = "error";

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