using System.Collections.Generic;

using Box2 = Veldrid.Rectangle;

namespace BareE.DataStructures
{
    public class SpatialPartion
    {
        public static int _currID = 0;
        private int ID = ++_currID;

        private bool _filled;

        private bool Filled
        {
            get
            {
                return _filled;
            }
        }

        public Box2 Space;
        private SpatialPartion[] Subspaces;

        public SpatialPartion(Box2 space, bool filled = false)
        {
            Space = space;
            _filled = filled;
        }

        public Box2 AddRectangle(System.Drawing.Size size)
        {
            return AddRectangle(new Box2(0, 0, size.Width, size.Height));
        }

        public Box2 AddRectangle(Box2 footprint)
        {
            Box2 _nullBox2 = default(Box2);
            if (footprint.Width > Space.Width || footprint.Height > Space.Height)
                return _nullBox2;
            if (Filled) return _nullBox2;

            if (Subspaces == null)
            {
                Subspaces = new SpatialPartion[3];

                Box2 filledArea = new Box2(Space.Left, Space.Top, footprint.Width, footprint.Height);
                Subspaces[0] = new SpatialPartion(filledArea, true);

                Box2 unfilledRightSide = new Box2(Space.Left + footprint.Width, Space.Top, Space.Width - footprint.Width, footprint.Height);
                if (unfilledRightSide.Width <= 0) unfilledRightSide.HasArea();
                Subspaces[1] = new SpatialPartion(unfilledRightSide);

                Box2 unfilledBotSide = new Box2(Space.Left, Space.Top + footprint.Height, Space.Width, Space.Height - footprint.Height);
                if (unfilledBotSide.Height <= 0) unfilledBotSide.HasArea();
                Subspaces[2] = new SpatialPartion(unfilledBotSide);
                return filledArea;
            }
            for (int i = 1; i < Subspaces.Length; i++)
            {
                Box2 result = Subspaces[i].AddRectangle(footprint);
                if (result.HasArea()) return result;
            }
            return _nullBox2;
        }

        public void Crop()
        {
            int mX = 0;
            int mY = 0;
            foreach (Box2 r in GetFilledRegions())
            {
                if (r.Left + r.Width > mX) mX = (int)r.Left + (int)r.Width;
                if (r.Top + r.Height > mY) mY = (int)r.Top + (int)r.Height;
            }
            Space = new Box2(0, 0, mX, mY);
        }

        public List<Box2> GetFilledRegions()
        {
            List<Box2> ret = new List<Box2>();
            ret.AddRange(GetFilledRegions(this));
            return ret;
        }

        private List<Box2> GetFilledRegions(SpatialPartion region)
        {
            //Console.WriteLine(region.ID);
            List<Box2> regions = new List<Box2>();
            if (region.Filled)
                regions.Add(region.Space);

            if (region.Subspaces != null)
            {
                for (int i = 0; i < region.Subspaces.Length; i++)
                    regions.AddRange(region.Subspaces[i].GetFilledRegions(region.Subspaces[i]));
            }
            return regions;
        }
    }
}