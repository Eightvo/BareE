using BareE.EZRend.VertexTypes;
using BareE.Rendering;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BareE.EZRend
{
    public class ColoredLineShader:LineShader<Float3_Float4>
    {
        public ColoredLineShader() : base("BareE.EZRend.Novelty.ColoredLines.Line")
        {
            
        }
    }
}
