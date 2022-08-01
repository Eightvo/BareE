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
        ConsoleSystem cSys;
        GUI.BareEGUISystem guiSys;
        public override void Load(Instant Instant, GameState State, GameEnvironment Env)
        {
            cSys = (ConsoleSystem)this.Systems.Enqueue(new ConsoleSystem(), 0);
            cSys.IsShowingMainMenuBar = false;
            guiSys = (GUI.BareEGUISystem)this.Systems.Enqueue(new BareE.GUI.BareEGUISystem(new System.Drawing.Size(Env.Window.Resolution.Width, Env.Window.Resolution.Height)),1);
            guiSys.AddWindow("Message", AssetManager.ReadFile(@"./Assets/Widgets/MessageBox.widget"));
            State.Messages.AddListener<ChangeSetting>(HandleSettingChange);

        }
        private bool HandleSettingChange(Messages.ChangeSetting msg, GameState state, Instant instant)
        {
            guiSys.SetResolution(Newtonsoft.Json.JsonConvert.DeserializeObject<Vector2>(msg.Value));
            return true;
        }

    }
}
