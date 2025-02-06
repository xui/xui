namespace Web4.Events;

public partial interface OneLevelRemoved
{
    public interface Keyboard: UI, Subsets.Modifiers, Subsets.IsComposing, Subsets.Keys
    {
    }
}
