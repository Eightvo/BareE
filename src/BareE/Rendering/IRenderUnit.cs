using System;
using System.Numerics;

using Veldrid;

namespace BareE.Rendering
{
    public interface ISceneDataProvider
    {
        AmbientLightData AmbientLight { get; set; }
        pointLightData PointLights { get; set; }
        CommonData CommonData { get; set; }

        T SceneData<T>(String key);
    }

    public class DefaultSceneDataProvider : ISceneDataProvider
    {
        public AmbientLightData AmbientLight { get; set; }
        public pointLightData PointLights { get; set; }
        public CommonData CommonData { get; set; }

        public T SceneData<T>(String key)
        {
            return default(T);
        }

        public DefaultSceneDataProvider()
        {
            AmbientLight = new AmbientLightData();
            PointLights = new pointLightData();
            CommonData = new CommonData();
        }

        public DefaultSceneDataProvider(AmbientLightData ald, pointLightData pld) : this(ald, pld, new CommonData())
        {
        }

        public DefaultSceneDataProvider(CommonData com) : this(new AmbientLightData(), new pointLightData(), com)
        {
        }

        public DefaultSceneDataProvider(AmbientLightData ald, pointLightData pld, CommonData com)
        {
            AmbientLight = ald;
            PointLights = pld;
            CommonData = com;
        }
    }

    /// <summary>
    /// An interface representing a GPU pipeline to execute a set of shaders.
    /// </summary>
    public interface IRenderUnit
    {
        void CreateResources(GraphicsDevice device);

        void Update(GraphicsDevice device);

        void Render(Framebuffer Trgt, CommandList cmds, ISceneDataProvider sceneData, Matrix4x4 CamMatrix, Matrix4x4 ModelMatrix);

        void SetOutputDescription(OutputDescription odesc);
    }
    /// <summary>
    /// An interface representing a structure to be used as a vertex buffer element.
    /// </summary>
    public interface IProvideVertexLayoutDescription
    {
        uint SizeInBytes { get; }

        VertexLayoutDescription GetVertexLayoutDescription(uint instanceStepRate = 0);
    }
}