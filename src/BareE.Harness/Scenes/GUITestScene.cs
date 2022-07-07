using BareE.GameDev;
using BareE.Messages;
using BareE.Rendering;
using BareE.Systems;

using System.Collections.Generic;
using System.Numerics;

using Veldrid;

using IG = ImGuiNET.ImGui;

namespace BareE.Harness.Scenes
{
    public class GUITestScene : GameSceneBase
    {
        public override void Load(Instant Instant, GameState State, GameEnvironment Env)
        {
            this.Systems.Enqueue(new ConsoleSystem(), 0);
            this.Systems.Enqueue(new BareE.GUI.BareEGUISystem(new System.Drawing.Size(Env.Window.Resolution.Width, Env.Window.Resolution.Height)),1);
        }
    }
}
