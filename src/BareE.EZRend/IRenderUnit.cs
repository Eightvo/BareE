﻿using BareE.Rendering;

using EZRend.ImageShader.FullscreenTexture;

using System;
using System.Numerics;

using Veldrid;

namespace EZRend
{

    public interface ISceneDataProvider
    {
        ambientLightData AmbientLight { get; set; }
        pointLightData PointLights { get; set; }
        CommonData CommonData { get; set; }
        T SceneData<T>(String key);
    }
    public class DefaultSceneDataProvider : ISceneDataProvider
    {

        public ambientLightData AmbientLight { get; set; }
        public pointLightData PointLights { get; set; }
        public CommonData CommonData{ get; set; }

        public T SceneData<T>(String key) { return default(T); }

        public DefaultSceneDataProvider()
        {
            AmbientLight = new ambientLightData();
            PointLights = new pointLightData();
            CommonData = new CommonData();
        }
        public DefaultSceneDataProvider(ambientLightData ald, pointLightData pld):this(ald,pld, new CommonData())
        {

        }
        public DefaultSceneDataProvider(CommonData com):this(new ambientLightData(), new pointLightData(), com)
        {

        }
        public DefaultSceneDataProvider(ambientLightData ald, pointLightData pld, CommonData com)
        {
            AmbientLight = ald;
            PointLights = pld;
            CommonData = com;
        }
    }

    public interface IRenderUnit
    {
        void CreateResources(GraphicsDevice device);
        void Update(GraphicsDevice device);
        void Render(Framebuffer Trgt, CommandList cmds, ISceneDataProvider sceneData,Matrix4x4 CamMatrix, Matrix4x4 ModelMatrix);
        void SetOutputDescription(OutputDescription odesc);

    }

    //public interface IProvideVertexLayoutDescription
    //{
    //    uint SizeInBytes { get; }
    //    VertexLayoutDescription GetVertexLayoutDescription(uint instanceStepRate = 0);
    //}

}
