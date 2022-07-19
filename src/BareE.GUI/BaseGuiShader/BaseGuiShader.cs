using BareE.EZRend.ImageShader.FullscreenTexture;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Veldrid;

namespace BareE.GUI
{
    public class BaseGuiShader : MultiTextureShader<BaseGuiVertex>
    {
        public BaseGuiShader() : this("BareE.GUI.BaseGuiShader.BaseGui") { }
        public BaseGuiShader(string partial) : base(partial, 2){}

        public Vector2 resolution { get { return this.commondata.u_resolution; } set { this.commondata.u_resolution = value; } }
        public Vector2 mousepos { get { return this.commondata.u_mouse; } set { this.commondata.u_mouse = value; } }
        public Vector3 campos { get { return this.commondata.u_campos; } set { this.commondata.u_campos = value; } }
        int time { get { return this.commondata.time; } set { this.commondata.time = value; } }

        public void SetStyleTexture(GraphicsDevice device, Texture tex)
        {
            this.SetTexture(0, device, tex);
        }
        public void SetFontTexture(GraphicsDevice device, Texture tex)
        {
            this.SetTexture(1, device, tex);
        }
    }

}
