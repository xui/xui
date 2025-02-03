using System.Runtime.CompilerServices;

namespace Web4;

public partial interface Events
{
    public interface Touch: UI, Subsets.Modifiers, Subsets.Touches
    {
    }
}
