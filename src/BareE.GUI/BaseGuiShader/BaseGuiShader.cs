using BareE.EZRend.ImageShader.FullscreenTexture;
using BareE.Rendering;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Rectangle = SixLabors.ImageSharp.Rectangle;

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

        List<Tuple<int, int, Rectangle>> vertSets = new List<Tuple<int, int, Rectangle>>();
        public void ClearVertSets() { vertSets.Clear(); }
        public override void Clear()
        {
            base.Clear();
            ClearVertSets();
        }
        public void EndVertSet(Rectangle scissorRect)
        {
            if (vertSets.Count == 0)
            {
                vertSets.Add(new Tuple<int, int, Rectangle>(0, verts.Count, scissorRect));
                return;
            }
            var pVertSet = vertSets[vertSets.Count - 1];
            var start = pVertSet.Item1 + pVertSet.Item2;
            vertSets.Add(new Tuple<int, int, Rectangle>(start, verts.Count - start, scissorRect));
        }

        public void SetStyleTexture(GraphicsDevice device, Texture tex)
        {
            this.SetTexture(0, device, tex);
        }
        public void SetFontTexture(GraphicsDevice device, Texture tex)
        {
            this.SetTexture(1, device, tex);
        }

        public override void Render(Framebuffer Trgt, CommandList cmds, ISceneDataProvider sceneData, Matrix4x4 camMat, Matrix4x4 ModelMatrix)
        {
            if (VertexBuffer == null) return;
            BaseRender(Trgt, cmds, sceneData, camMat, ModelMatrix);
            //cmds.Draw((uint)verts.Count, 1, 0, 0);
            
            var lst = 0;
            foreach(var vSet in vertSets)
            {
                var Rect = vSet.Item3;
                var Start = vSet.Item1;
                var End = vSet.Item2;
                cmds.SetScissorRect(0, (uint)Rect.X, (uint)Rect.Y, (uint)Rect.Width, (uint)Rect.Height);
                cmds.Draw((uint)(End), 1, (uint)Start, 0);
                lst = Start + End;
            }
            if (verts.Count-lst>0)
            {
                cmds.Draw((uint)(verts.Count-lst), 1, (uint)lst, 0);
            }
        }
    }

}
