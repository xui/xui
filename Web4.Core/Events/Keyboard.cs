using System.Runtime.CompilerServices;

namespace Web4;

public partial interface Events
{
    public interface Keyboard: UI, Subsets.Modifiers, Subsets.IsComposing, Subsets.Keys
    {
        new const string Format = "code,key,location,repeat," + 
            UI.Format + "," + 
            Subsets.IsComposing.Format + "," +
            Subsets.Modifiers.Format + "," + 
            Subsets.Keys.Format;
    }
}
