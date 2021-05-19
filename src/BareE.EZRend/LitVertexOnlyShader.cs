using BareE.Rendering;

using BareE.EZRend.ImageShader.FullscreenTexture;

using System;
using System.Collections.Generic;
using System.Numerics;

using Veldrid;

namespace BareE.EZRend
{
    public class LitVertexOnlyShader<V> : VertexOnlyShader<V>
        where V: unmanaged, IProvideVertexLayoutDescription
    {
        public LitVertexOnlyShader(String partial) : base(partial) { }
        public LitVertexOnlyShader(String vert, String frag) : base(vert, frag) { }

        ambientLightData ald;
        pointLightData pld;
        CommonData commondata;


        DeviceBuffer lightDataBuffer;
        DeviceBuffer pointLightDataBuffer;
        DeviceBuffer dataBuffer;

        ResourceLayout LightsLayout;
        ResourceSet LightsSet;

        public override void UpdateBuffers(CommandList cmds)
        {
            cmds.UpdateBuffer(lightDataBuffer, 0, ald);
            cmds.UpdateBuffer(pointLightDataBuffer, 0, pld);
            cmds.UpdateBuffer(dataBuffer, 0, commondata);
            base.UpdateBuffers(cmds);
        }
        public override void Render(Framebuffer Trgt, CommandList cmds, ISceneDataProvider sceneData, Matrix4x4 CameraMatrix, Matrix4x4 ModelMatrix)
        {
            ald = sceneData.AmbientLight;
            pld = sceneData.PointLights;
            commondata = sceneData.CommonData;
            base.Render(Trgt, cmds, sceneData, CameraMatrix, ModelMatrix);
        }
        public override void CreateResources(GraphicsDevice device)
        {
            lightDataBuffer = device.ResourceFactory.CreateBuffer(new BufferDescription(ambientLightData.Size, BufferUsage.UniformBuffer));
            lightDataBuffer.Name = "AbmientLightDataBuffer";

            pointLightDataBuffer = device.ResourceFactory.CreateBuffer(new BufferDescription(pointLightData.Size, BufferUsage.UniformBuffer));
            pointLightDataBuffer.Name = "PointLightDataBuffer";

            dataBuffer = device.ResourceFactory.CreateBuffer(new BufferDescription(CommonData.Size, BufferUsage.UniformBuffer));
            dataBuffer.Name = "CommonData";

            LightsLayout = device.ResourceFactory.CreateResourceLayout(
                                new ResourceLayoutDescription(
                                        new ResourceLayoutElementDescription("AmbientLightData",
                                        ResourceKind.UniformBuffer, ShaderStages.Fragment | ShaderStages.Vertex),
                                        new ResourceLayoutElementDescription("PointLightData",
                                        ResourceKind.UniformBuffer, ShaderStages.Fragment | ShaderStages.Vertex),
                                        new ResourceLayoutElementDescription("CommonData",
                                        ResourceKind.UniformBuffer, ShaderStages.Fragment | ShaderStages.Vertex)
                                    )
                                );
            LightsSet = device.ResourceFactory.CreateResourceSet(
                    new ResourceSetDescription(LightsLayout,
                    lightDataBuffer,
                    pointLightDataBuffer,
                    dataBuffer
                    ));
            LightsSet.Name = "Lights";

            base.CreateResources(device);
        }

        protected override IEnumerable<ResourceLayout> CreateResourceLayout()
        {
            foreach(var rl in base.CreateResourceLayout())
                yield return rl;
            yield return LightsLayout;
        }
        public override IEnumerable<ResourceSet> GetResourceSets()
        {
            foreach (var rs in base.GetResourceSets())
                yield return rs;
            yield return LightsSet;
        }
    }

}
