namespace Web4.Events;

public partial interface OneLevelRemoved
{
    public interface Animation : Base, Subsets.Animation
    {
        /// <summary>
        /// A string containing the value of the animation-name that generated the animation.
        /// </summary>
        string AnimationName { get; }
    }
}
