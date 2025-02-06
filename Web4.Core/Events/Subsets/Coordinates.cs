using System.Runtime.CompilerServices;

namespace Web4
{
    namespace Events
    {
        public partial interface OneLevelRemoved
        {
            public partial interface Subsets
            {
                public interface Coordinates : XY, ClientXY, MovementXY, OffsetXY, PageXY, ScreenXY
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
    }
}