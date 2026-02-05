namespace Web4.Dom.Events.Subsets;

public interface ITargetSubset<T> : ISubset, IViewSubset
{
    new const string TRIM = "target";

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