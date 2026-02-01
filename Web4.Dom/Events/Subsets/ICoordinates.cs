using static Web4.Dom.Events.Aliases.Subsets;

namespace Web4.Dom
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