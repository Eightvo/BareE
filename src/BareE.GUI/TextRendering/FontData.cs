using System.Collections.Generic;

namespace BareE.GUI.TextRendering
{
    public enum FontType
    {
        BitmapFont = 1,
        SignedDistanceFont = 2,
    }
    public struct FontData
    {
        public FontType FontType { get; set; }
        public int LineHeight { get; set; }
        public int BaseLine { get; set; }
        public int TotalWidth { get; set; }
        public int SpaceWidth { get; set; }
        public Dictionary<int, GlyphData> Glyphs { get; set; }
    }

}
