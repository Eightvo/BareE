using System.Drawing;

namespace BareE.GUI.TextRendering
{
    public struct GlyphData
    {
        public int Advance { get; set; }
        public int BearingX { get; set; }
        public int Drop { get; set; }
        public int PixelX { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        internal Rectangle Rect
        {
            get
            {
                return new Rectangle()
                {
                    X = PixelX,
                    Y = 0,
                    Height = this.Height,
                    Width = this.Width
                };
            }
        }
    }

}
