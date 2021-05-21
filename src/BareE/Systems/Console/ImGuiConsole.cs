using BareE.Components;
using BareE.DataStructures;
using BareE.GameDev;
using BareE.Messages;

using ImGuiNET;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

using Veldrid;

namespace BareE.Systems
{
    /// <summary>
    /// A build in System providing some terminal like functionality such
    /// as basic text output, input and allowing the use of ConsoleCommand
    /// </summary>
    public partial class ConsoleSystem : GameSystem
    {
        private bool Echo = true;
        private bool PropagateToLog = false;
        private RingBuffer<String> _outputBuffer;
        private RingBuffer<String> _history;
        private RingBuffer<String> _temp;
        private Entity _CmdRoot;
        private int _cursorPosition;
        private StringBuilder _buffer;

        private Widgets.BareE.Widgets.Credits CreditsWidget;

        private int currHistPtr = 0;
        //        ECS.ECContainer ECC;

        private String cmdText = String.Empty;

        public bool IsShowingConsoleWindow = true;

        private bool cycleModeRequested;

        public override void Update(Instant Instant, GameState State, GameEnvironment Env)
        {
            if (cycleModeRequested)
            {
                cycleModeRequested = false;
                switch (Env.DisplayMode)
                {
                    case DisplayMode.MonitorOnly:
                        Env.DisplayMode = DisplayMode.Emulate;
                        break;

                    case DisplayMode.Emulate:
                        Env.DisplayMode = DisplayMode.MonitorOnly;
                        break;

                    case DisplayMode.Mirror:
                        Env.DisplayMode = DisplayMode.VROnly;
                        break;

                    case DisplayMode.VROnly:
                        Env.DisplayMode = DisplayMode.Mirror;
                        break;
                }
            }
        }

        private IEnumerable<object> getChildEnts(Dictionary<int, List<Entity>> pTree, Dictionary<int, String> nNames, EntityComponentContext ecc, int parent, int depth = 0)
        {
            if (!pTree.ContainsKey(parent)) yield break;
            foreach (var v in pTree[parent])
            {
                if (v == null) continue;
                yield return $"{("".PadLeft(2 * depth))}{v.Id} {(nNames.ContainsKey(v.Id) ? nNames[v.Id] : "Anon")}";
                foreach (var s in getChildEnts(pTree, nNames, ecc, v.Id, depth + 1))
                    yield return s;
            };
        }

        private void CreateCommands(GameState state)
        {
            _CmdRoot = state.ECC.SpawnEntity("CmdRoot",
            new ConsoleCommandSet()
            {
                SetName = "BareE",
                Commands = new ConsoleCommand[]
                {
                    new ConsoleCommand()
                    {
                        Callback = new Func<string, GameState, Instant, object[]>((a, s, i) =>
                        {
                            _outputBuffer.Clear();
                            return new object[] { String.Empty };
                        }),
                        Cmd = "Cls",
                        HelpText = "Clear Console"
                    },
                    new ConsoleCommand()
                    {
                        Callback = new Func<string, GameState, Instant, object[]>((a, s, i) =>
                        {
                            cycleModeRequested = true;
                            return new object[] { true };
                        }),
                        Cmd = "CycleMode",
                        HelpText = "Change Mode"
                    },
                    new ConsoleCommand()
                    {
                        Callback = new Func<string, GameState, Instant, object[]>((a, s, i) =>
                        {
                            List<String> ret = new List<string>();
                            foreach (var v in ComponentCache.ComponentAliasMap.Keys())
                                ret.Add(v);
                            return ret.ToArray();
                        }),
                        Cmd = "ListComps",
                        HelpText = "List All known components"
                    },
                    new ConsoleCommand()
                    {
                        Callback = new Func<string, GameState, Instant, object[]>((a, s, i) =>
                        {
                            List<String> ret = new List<string>();
                            foreach (var v in MessageQueue._messageAliasMap.Keys())
                                ret.Add(v);
                            return ret.ToArray();
                        }),
                        Cmd = "ListMsg",
                        HelpText = "List All known messagess"
                    },
                    new ConsoleCommand()
                    {
                        Callback = new Func<string, GameState, Instant, object[]>(DescMsgFunc),
                        Cmd = "DescMsg",
                        HelpText = "Show structure of a message"
                    },
                    new ConsoleCommand()
                    {
                        Callback = new Func<string, GameState, Instant, object[]>(DescCompFunc),
                        Cmd = "DescComp",
                        HelpText = "Show structure of a Component"
                    },
                    new ConsoleCommand()
                    {
                        Callback = new Func<string, GameState, Instant, object[]>(EmitMsgFunc),
                        Cmd = "EmitMsg",
                        HelpText = "Emit a message"
                    },
                    new ConsoleCommand()
                    {
                        Callback = new Func<String, GameState, Instant, object[]>(listallents),
                        Cmd = "ListEnts",
                        HelpText = "List all parent entities."
                    },
                    new ConsoleCommand()
                    {
                        Callback = new Func<String, GameState, Instant, object[]>(DescEntFunc),
                        Cmd = "DescEnt",
                        HelpText = "List components on entity"
                    },
                    new ConsoleCommand()
                    {
                        Callback = new Func<String, GameState, Instant, object[]>(DescEntCompFunc),
                        Cmd = "DescEntComp",
                        HelpText = "Shows values of the component of an entity"
                    },
                    new ConsoleCommand()
                    {
                        Callback = new Func<String, GameState, Instant, object[]>(ListAssetsFunc),
                        Cmd = "listassets",
                        HelpText = "Shows values of the component of an entity"
                    },
                    new ConsoleCommand()
                    {
                        Callback = new Func<String, GameState, Instant, object[]>((a,g,i)=>{ CreditsWidget.IsVisible=true; return new object[]{ "Done"}; }),

                        Cmd = "Credits",
                        HelpText = "Shows Credits"
                    }
                }
            });
        }

        public override void Load(Instant instant, GameState state, GameEnvironment environment)
        {
            _outputBuffer = new RingBuffer<string>(20);
            _history = new RingBuffer<string>(10);
            _temp = new RingBuffer<string>(10);

            CreateCommands(state);
        }

        public override void Initialize(Instant instant, GameState state, GameEnvironment environment)
        {
            state.Messages.AddListener<ConsoleInput>(ProcessConsoleInput);
            CreditsWidget = new Widgets.BareE.Widgets.Credits();
        }

        public override void RenderEye(Instant instant, GameState state, GameEnvironment env, Matrix4x4 eyeMat, Framebuffer Target, CommandList commands)
        { }

        public unsafe override void RenderHud(Instant Instant, GameState State, GameEnvironment env, Framebuffer Target, CommandList cmds)
        {
            CreditsWidget.Render(Instant, State, env);
            if (!IsShowingConsoleWindow)
                return;
            ImGui.Begin($"Console");
            var cellSize = ImGui.CalcTextSize("W");
            ImGui.SetWindowSize(new Vector2(cellSize.X * 50, cellSize.Y * 30.5f), ImGuiCond.Appearing);
            //ImGui.SetWindowSize(new Vector2(500, 300f), ImGuiCond.Appearing);
            ImGui.SetWindowSize(new Vector2(ImGui.GetWindowSize().X, cellSize.Y * 30.5f));
            for (int i = 0; i < _outputBuffer.Capacity - _outputBuffer.Count; i++)
                ImGui.Text("");
            foreach (String s in _outputBuffer.Enumerate())
                ImGui.Text($"{s}");

            ImGui.PushItemWidth(-1);
            ImGuiInputTextCallback callback = (data) =>
            {
                var v = new ImGuiNET.ImGuiInputTextCallbackDataPtr(data);
                if (v.EventFlag == ImGuiInputTextFlags.CallbackHistory && _history.Count > 0)
                {
                    v.DeleteChars(0, v.BufTextLen);
                    v.InsertChars(0, _history[_history.Count - (1 + currHistPtr)]);
                    if (v.EventKey == ImGuiKey.UpArrow)
                        currHistPtr += 1;
                    if (v.EventKey == ImGuiKey.DownArrow)
                        currHistPtr -= 1;
                    currHistPtr = Math.Min(_history.Count - 1, Math.Max(0, currHistPtr));
                }
                return 0;
            };
            if (ImGui.InputText("##CmdLineTB", ref cmdText, 200, ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.CallbackHistory, callback))
            {
                _history.Push(cmdText);
                currHistPtr = 0;
                State.Messages.EmitMsg(new ConsoleInput() { Text = cmdText });
                cmdText = String.Empty;
            }
            if (ImGui.IsWindowFocused(ImGuiFocusedFlags.RootWindow) && !ImGui.GetIO().WantCaptureKeyboard)
            {
                ImGui.SetKeyboardFocusHere(0);
            }
            ImGui.End();
        }

        private void SendOutput(String txt)
        {
            _outputBuffer.Push(txt);
            if (PropagateToLog) Log.EmitTrace(txt);
        }

        public bool ProcessConsoleInput(ConsoleInput msg, GameState state, Instant instant)
        {
            if (Echo && !msg.System)
                _outputBuffer.Push($"  >{msg.Text}");

            if (String.Compare(msg.Text, "BOOT", true) == 0 && msg.System)
            {
                SendOutput("Initial Startup sequence detected");
                SendOutput("");
                SendOutput(@" EEEE *     * @@@@");
                SendOutput(@" E  E **   ** @  @");
                SendOutput(@"  EE   *   *  @  @");
                SendOutput(@" E  E  ** **  @  @");
                SendOutput(@" EEEE   ***   @@@@");
                SendOutput(@"-----  BareE -----");
                SendOutput("? to list commands");
                return false;
            }
            if (String.Compare(msg.Text, "?", true) == 0)
            {
                foreach (var v in state.ECC.Components.GetEntitiesByComponentType<ConsoleCommandSet>().OrderBy(x => x.Value.SetName))
                {
                    SendOutput($"CommandSet: [{v.Value.SetName}]");
                    foreach (var c in v.Value.Commands)
                    {
                        SendOutput($"  {c.Cmd} :   {c.HelpText}");
                    }
                }
            }

            var cmd = msg.Text;
            if (msg.Text.Trim().IndexOf(' ') >= 0)
                cmd = msg.Text.Substring(0, msg.Text.IndexOf(' '));
            var args = msg.Text.Substring(cmd.Length);
            foreach (var v in state.ECC.Components.GetEntitiesByComponentType<ConsoleCommandSet>())
            {
                foreach (var c in v.Value.Commands)
                {
                    if (String.Compare(cmd, c.Cmd, true) == 0)
                    {
                        var results = c.Callback(args, state, instant);
                        if (results != null)
                        {
                            foreach (var r in results)
                            {
                                SendOutput($"{r}");
                            }
                        }
                    }
                }
            }

            return false;
        }
    }
}