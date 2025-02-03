using System.Runtime.CompilerServices;

namespace Web4;

public partial interface Events
{
    public interface Focus : UI, Subsets.RelatedTarget
    {
        new const string Format = 
            UI.Format + "," + 
            Subsets.RelatedTarget.Format;
    }
}
