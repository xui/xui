namespace Web4
{
    namespace Events.Subsets
    {
        public interface ITarget<T> : ISubset, IView
        {
            new const string Format = "target";

            /// <summary>
            /// A reference to the object to which the event was originally dispatched.
            /// </summary>
            EventTarget<T> Target { get; }
        }

        public interface EventTarget<T>
        {
            public string ID { get; }
            public string Name { get; }
            public string Type { get; }
            public bool? Checked { get; }
            public T Value { get; }
        }
    }
}