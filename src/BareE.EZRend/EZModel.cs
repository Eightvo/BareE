using BareE.Rendering;

using System;
using System.Collections.Generic;

using Veldrid;

namespace BareE.EZRend
{
    public class EZModel
    {
        public Dictionary<String, IRenderUnit> Meshes = new Dictionary<string, IRenderUnit>(StringComparer.InvariantCultureIgnoreCase);

        public void AddMesh(String name, IRenderUnit renderable)
        {
            var nxt = 2;
            var nxtName = name;
            while (Meshes.ContainsKey(nxtName))
            {
                nxtName = $"{name}{nxt}";
                nxt += 1;
            }
            Meshes.Add(nxtName, renderable);
        }

        public void CreateResource(OutputDescription outputdesc, GraphicsDevice device)
        {
            foreach (var r in Meshes.Values)
            {
                r.SetOutputDescription(outputdesc);
                r.CreateResources(device);
            }
        }
    }
}