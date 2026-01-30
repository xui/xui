using System.Runtime.CompilerServices;
using static Web4.Events.Aliases.Subsets;

namespace Web4
{
    namespace Events.Subsets
    {
        public interface ICoordinates : ISubset, XY, ClientXY, MovementXY, OffsetXY, PageXY, ScreenXY, IView
        {
            new const string Format = 
                XY.Format + "," + 
                ClientXY.Format + "," + 
                MovementXY.Format + "," + 
                OffsetXY.Format + "," + 
                PageXY.Format + "," + 
                ScreenXY.Format;
        }
    }
}