namespace Web4.Events;

public partial interface OneLevelRemoved
{
    public interface Transition : Base, Subsets.Animation
    {
        /// <summary>
        /// A string containing the name CSS property associated with the transition.
        /// </summary>
        string PropertyName { get; }
    }
}
