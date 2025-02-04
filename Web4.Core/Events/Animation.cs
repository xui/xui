namespace Web4;

public partial interface Events
{
    public interface Animation : Base, Subsets.Animation
    {
        /// <summary>
        /// A string containing the value of the animation-name that generated the animation.
        /// </summary>
        string AnimationName { get; }
    }
}
