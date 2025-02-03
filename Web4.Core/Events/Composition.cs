using System.Runtime.CompilerServices;

namespace Web4;

public partial interface Events
{
    public interface Composition : UI, Subsets.Data
    {
        new const string Format = 
            UI.Format + "," + 
            Subsets.Data.Format;
    }
}
