using System.Runtime.CompilerServices;

namespace Web4;

public partial interface Events
{
    public interface Keyboard: UI, Subsets.Modifiers, Subsets.IsComposing, Subsets.Keys
    {
    }
}
