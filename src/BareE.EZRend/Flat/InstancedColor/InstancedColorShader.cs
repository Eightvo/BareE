using BareE.EZRend.VertexTypes;
using BareE.Rendering;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Veldrid;

namespace BareE.EZRend.Flat
{

    public class InstancedColorShader:InstancedShader<Float3,Float4_Float4>
    {
        public InstancedColorShader() : base("BareE.EZRend.Flat.InstancedColor.InstancedColor") { }

    }
}
