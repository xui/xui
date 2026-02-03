using static Web4.Dom.Events.Aliases.Subsets;

namespace Web4.Dom
{
    namespace Events.Subsets
    {
        public interface ICoordinates : ISubset, XY, ClientXY, MovementXY, OffsetXY, PageXY, ScreenXY, IView
        {
            new const string TRIM = $"{XY.TRIM},{ClientXY.TRIM},{MovementXY.TRIM},{OffsetXY.TRIM},{PageXY.TRIM},{ScreenXY.TRIM}";
        }
    }
}