using BareE.Rendering;

using System.Collections.Generic;
using System.Numerics;

using Veldrid;

namespace BareE.EZRend
{
    public class RenderSequence : IRenderUnit
    {
        private SortedList<int, IRenderUnit> RenderList = new SortedList<int, IRenderUnit>();

        public IRenderUnit Add(IRenderUnit r)
        {
            return Add(RenderList.Count + 1, r);
        }

        public IRenderUnit Add(int priority, IRenderUnit r)
        {
            RenderList.Add(priority, r);
            return r;
        }

        public void SetOutputDescription(OutputDescription desc)
        {
            foreach (var v in RenderList)
                v.Value.SetOutputDescription(desc);
        }

        public void CreateResources(GraphicsDevice device)
        {
            foreach (var kvp in RenderList)
                kvp.Value.CreateResources(device);
        }

        public void Render(Framebuffer Trgt, CommandList cmds, ISceneDataProvider scenedata, Matrix4x4 camMat, Matrix4x4 ModelMatrix)
        {
            foreach (var kvp in RenderList)
                kvp.Value.Render(Trgt, cmds, scenedata, camMat, ModelMatrix);
        }

        public void Update(GraphicsDevice device)
        {
            foreach (var kvp in RenderList)
                kvp.Value.Update(device);
        }
    }
}