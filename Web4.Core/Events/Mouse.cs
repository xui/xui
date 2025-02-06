namespace Web4.Events;

public partial interface OneLevelRemoved
{
    public interface Mouse : UI, Subsets.Buttons, Subsets.Coordinates, Subsets.Modifiers, Subsets.RelatedTarget
    {
    }
}
