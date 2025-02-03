using System.Runtime.CompilerServices;

namespace Web4;

public partial interface Events
{
    public interface Mouse : UI, Subsets.Buttons, Subsets.Coordinates, Subsets.Modifiers, Subsets.RelatedTarget
    {
    }
}
