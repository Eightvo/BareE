using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BareE.GUI.Widgets
{
    public class BPoint
    {
        SixLabors.ImageSharp.Point _point { get; set; }
        String ptStr { get; set; } = null;

        public BPoint(String val)
        {
            ptStr = val;
        }
        public BPoint(int x, int y)
        {
            _point = new SixLabors.ImageSharp.Point(x, y);
            ptStr = null;
        }
        public BPoint(SixLabors.ImageSharp.Point pt)
        {
            this._point = pt;
            ptStr = null;
        }
        //public SixLabors.ImageSharp.Point CalculatePoint(SixLabors.ImageSharp.Rectangle Widget)
        public static implicit operator BPoint(SixLabors.ImageSharp.Point pt)
        {
            return new BPoint(pt);
        }
//        public static implicit operator SixLabors.ImageSharp.Point (BPoint pt)
//        {
//            return pt.Point;
//        }

    }
}
