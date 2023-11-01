using System;
using System.Collections.Generic;
using System.Numerics;

using Veldrid.Sdl2;

namespace BareE.GameDev
{
    /// <summary>
    /// Maps Input to numeric values for actionability via alias.
    /// Multiple Keys may map to the same alias, however multiple aliases can not map to the same key(s).
    /// The values returned depend on the type of Input as defined in the InputConfiguration.
    /// An Axis Input will return a value between [-1,1] while a button will return a value between [0,1]
    /// </summary>
    public class InputHandler
    {
        #region Static

        public static Dictionary<String, ControlGroupModel> ControlGroupDefs = new Dictionary<string, ControlGroupModel>();

        /// <summary>
        /// Append a set of Control definitions from a file to existing control definitions.
        /// </summary>
        /// <param name="filename"></param>
        public static void LoadFromConfig(String filename)
        {
            ControlGroupModel[] groups;
            groups = Newtonsoft.Json.JsonConvert.DeserializeObject<ControlGroupModel[]>(AssetManager.ReadFile(filename));
            foreach (var v in groups)
            {
                ControlGroupDefs.Add(v.Group, v);
            }
        }
        public static void LoadGroup(String src)
        {
            ControlGroupModel group;
            group = Newtonsoft.Json.JsonConvert.DeserializeObject<ControlGroupModel>(src);
            if (ControlGroupDefs.ContainsKey(group.Group))
                ControlGroupDefs[group.Group] = group;
            else
                ControlGroupDefs.Add(group.Group, group);
        }

        /// <summary>
        /// Construct an InputHandler instance that maps aliases to values according to the specified definition groups.
        /// </summary>
        /// <param name="groups"></param>
        /// <returns></returns>
        public static InputHandler Build(params string[] groups)
        {
            InputHandler ret = new InputHandler();
            foreach (var groupname in groups)
            {
                if (!ControlGroupDefs.ContainsKey(groupname))
                {
                    Log.EmitError(new Exception($"Couldn't find control group named {groupname}"));
                    continue;
                }
                ControlGroupModel cgm = ControlGroupDefs[groupname];
                foreach (var cntrlDef in cgm.Controls)
                {
                    AddFromDef(ret, cntrlDef);
                }
            }
            /*
            InputHandler.AddControl("Throttle", new PairControl(
    new InputAlias() { Alias = "Throttle", Source = InputSource.Keyboard, SourceKey = (int)Veldrid.Sdl2.SDL_Scancode.SDL_SCANCODE_S },
    new InputAlias() { Alias = "Throttle", Source = InputSource.Keyboard, SourceKey = (int)Veldrid.Sdl2.SDL_Scancode.SDL_SCANCODE_W }
    ));
            InputHandler.AddControl("Rudder", new PairControl(
                new InputAlias() { Alias = "Rudder", Source = InputSource.Keyboard, SourceKey = (int)Veldrid.Sdl2.SDL_Scancode.SDL_SCANCODE_A },
                new InputAlias() { Alias = "Rudder", Source = InputSource.Keyboard, SourceKey = (int)Veldrid.Sdl2.SDL_Scancode.SDL_SCANCODE_D }
                ));
                */

            return ret;
        }

        private static void AddFromDef(InputHandler hndlr, ControlDefModel def)
        {
            InputControl cntrl = null;
            var isInverted = false;
            switch (def.ControlType.ToLower())
            {
                case "button":
                    cntrl = AddButtonFromDef(def, ref isInverted);
                    break;

                case "axis":
                    var defStr = def.Def;
                    if (defStr.StartsWith("-"))
                    {
                        isInverted = true;
                        defStr = defStr.Substring(1);
                    }

                    cntrl = AddAxisFromDef(def, cntrl, defStr);
                    if (isInverted)
                        cntrl.Invert = true;
                    break;
            }

            float dzMin = (def.DZMin == 0 ? cntrl.DeadZoneBounds.X : def.DZMin);
            float dzMax = (def.DZMax == 0 ? cntrl.DeadZoneBounds.Y : def.DZMax);
            cntrl.DeadZoneBounds = new Vector2(dzMax, dzMax);
            hndlr.AddControl(def.Alias, cntrl);
        }

        private static InputControl AddAxisFromDef(ControlDefModel def, InputControl cntrl, string defStr)
        {
            switch (def.Src)
            {
                case InputSource.Keyboard:
                    String k1 = defStr.Substring(0, def.Def.IndexOf(']') + 1);
                    String k2 = def.Def.Substring(k1.Length);
                    cntrl = new PairControl(
                        new InputAlias() { Alias = def.Alias, Source = def.Src, SourceKey = ReadKeyFromConfig(def.Src, k1) },
                        new InputAlias() { Alias = def.Alias, Source = def.Src, SourceKey = ReadKeyFromConfig(def.Src, k2) }
                        );
                    break;

                case InputSource.Gamepad:
                    cntrl = new SoloControl(new InputAlias() { Alias = def.Alias, Source = def.Src, SourceKey = ReadKeyFromConfig(def.Src, defStr) });
                    break;

                case InputSource.Mouse:
                    if (String.Compare(defStr, "wheel", true) == 0)
                    {
                        cntrl = new MouseWheelAxis(new InputAlias() { Alias = def.Alias, Source = def.Src, SourceKey = MouseWheelID });
                    }
                    else if (String.Compare(defStr, "x", true) == 0)
                    {
                        cntrl = new MouseWheelAxis(new InputAlias() { Alias = def.Alias, Source = def.Src, SourceKey = MouseMoveXID });
                    }
                    else if (String.Compare(defStr, "y", true) == 0)
                    {
                        cntrl = new MouseWheelAxis(new InputAlias() { Alias = def.Alias, Source = def.Src, SourceKey = MouseMoveYID });
                    }
                    else
                    {
                        throw new NotImplementedException("Only mouse wheel for mouse axises");
                    }

                    break;
            }

            return cntrl;
        }

        private static InputControl AddButtonFromDef(ControlDefModel def, ref bool isInverted)
        {
            InputControl cntrl;
            if (def.Def.StartsWith("-"))
                isInverted = true;
            cntrl = new SoloControl(new InputAlias()
            {
                Alias = def.Alias,
                Source = def.Src,
                SourceKey = ReadKeyFromConfig(def.Src, isInverted ? def.Def.Substring(1) : def.Def)
            });
            cntrl.Invert = isInverted;
            return cntrl;
        }

        private static int ReadKeyFromConfig(InputSource src, String def)
        {
            String d = def;
            int s = 0;
            int e = d.Length;
            if (d.StartsWith('['))
                s = 1;
            if (d.EndsWith(']'))
                e = e - 1;
            d = d.Substring(s, e - s);
            switch (src)
            {
                case InputSource.Keyboard:
                    return ReadKeyboardKeyFromConfig(d);
                case InputSource.Gamepad:
                    return ReadGamepadKeyFromConfig(d);
                case InputSource.Mouse:
                    return ReadMouseKeyFromConfig(d);
            }
            return 0;
        }

        private static int ReadMouseKeyFromConfig(string d)
        {
            switch (d.ToLower())
            {
                case "1":
                    return (int)SDL_MouseButton.X1;

                case "2":
                    return (int)SDL_MouseButton.X2;

                default:
                    SDL_MouseButton btn;
                    if (Enum.TryParse<SDL_MouseButton>(d, true, out btn))
                    {
                        return (int)(btn);
                    }
                    Log.EmitError(new Exception($"Could not parse mousepad source {d}"));
                    return 0;
            }
        }

        private static int ReadGamepadKeyFromConfig(String d)
        {
            SDL_GameControllerAxis axis;
            if (Enum.TryParse<SDL_GameControllerAxis>(d, true, out axis))
            {
                return (int)axis;
            }
            SDL_GameControllerButton btn;
            if (Enum.TryParse<SDL_GameControllerButton>(d, true, out btn))
            {
                return (int)SDL_GameControllerAxis.Max + (int)(btn);
            }
            Log.EmitError(new Exception($"Could not parse gamepad source {d}"));
            return 0;
        }
        private static int ReadKeyboardKeyFromConfig(string d)
        {
            switch (d.ToLower())
            {
                case "esc":
                    return (int)SDL_Scancode.SDL_SCANCODE_ESCAPE;

                case "ret":
                case "enter":
                    return (int)SDL_Scancode.SDL_SCANCODE_RETURN;

                default:
                {
                    SDL_Scancode SC;
                    if (!Enum.TryParse<SDL_Scancode>($"SDL_SCANCODE_{d.ToLower()}", true, out SC))
                    {
                        Log.EmitError(new Exception($"Could not parse keyboard key source {d.ToLower()}"));
                        return 0;
                    }
                    return (int)SC;
                }
            }
        }

        #endregion Static

        #region Instance

        private Dictionary<String, uint> CurrentSince = new Dictionary<string, uint>();
        private Dictionary<String, float> CurrentValues = new Dictionary<string, float>();
        private Dictionary<InputSource, Dictionary<int, InputAlias>> AliasedControls = new Dictionary<InputSource, Dictionary<int, InputAlias>>();
        private Dictionary<String, List<InputControl>> Controls = new Dictionary<string, List<InputControl>>();
        //HashSet<String> __NO_AUTO_UNSET_EVENT_ALIASES = new HashSet<string>();

        public bool AddControl(String controlName, InputControl cntrl, bool overwrite = false)
        {
            if (!Controls.ContainsKey(controlName))
                Controls.Add(controlName, new List<InputControl>());
            Controls[controlName].Add(cntrl);
            foreach (var v in cntrl.GetChildAliases())
                AddAlias(v);
            return true;
        }

        private bool AddAlias(InputAlias alias, bool overwrite = false)
        {
            if (!AliasedControls.ContainsKey(alias.Source))
                AliasedControls.Add(alias.Source, new Dictionary<int, InputAlias>());

            if (!AliasedControls[alias.Source].ContainsKey(alias.SourceKey))
            {
                AliasedControls[alias.Source].Add(alias.SourceKey, alias);
                return true;
            }
            if (!overwrite)
                return false;
            AliasedControls[alias.Source][alias.SourceKey] = alias;
            return true;
        }

        /// <summary>
        /// Get the value of a control by alias.
        /// </summary>
        /// <param name="alias"></param>
        /// <returns></returns>
        public float this[string alias]
        {
            get
            {
                if (!Controls.ContainsKey(alias))
                    return 0;
                foreach (var c in Controls[alias])
                {
                    var v = c.GetControlValue(ref CurrentValues);
                    var cV = Math.Abs(v);
                    if (Math.Abs(cV) > c.DeadZoneBounds.X)
                    {
                        if (cV > c.DeadZoneBounds.Y)
                        {
                            return Math.Sign(v);
                        }
                        return v;
                    }
                }
                return 0;
            }
        }

        /*
        public float this[string alias, uint time]
        {
            get
            {
                if (!CurrentSince.ContainsKey(alias))
                    return 0;
//                if (!Controls.ContainsKey(alias))
//                    return 0;
                if (CurrentSince[alias] < time)
                    return 0;
                foreach (var c in Controls[alias])
                {
                    var v = c.GetControlValue(ref CurrentValues);
                    var cV = Math.Abs(v);
                    if (Math.Abs(cV) > c.DeadZoneBounds.X)
                    {
                        if (cV > c.DeadZoneBounds.Y)
                        {
                            return Math.Sign(v);
                        }
                        return v;
                    }
                }
                return 0;
            }
        }
*/

        /// <summary>
        /// Get a 2D vector from a pair of aliases.
        /// </summary>
        /// <param name="aliasX"></param>
        /// <param name="aliasY"></param>
        /// <returns></returns>
        public Vector2 this[string aliasX, string aliasY]
        {
            get
            {
                return new Vector2(this[aliasX], this[aliasY]);
            }
        }

        /// <summary>
        /// Read a value and ensure the value is not consumed again until the input is triggered again.
        /// </summary>
        /// <param name="alias"></param>
        /// <returns></returns>
        public float ReadOnce(String alias)
        {
            var r = this[alias];
            CurrentValues[alias] = 0;
            return r;
        }

        /// <summary>
        /// Read a 2D vector from a pair of Values and ensure that value is not consumed again until the input is triggered again.
        /// </summary>
        /// <param name="alias1"></param>
        /// <param name="alias2"></param>
        /// <returns></returns>
        public Vector2 ReadOnce(String alias1, String alias2)
        {
            var r = this[alias1, alias2];
            CurrentValues[alias1] = 0;
            CurrentValues[alias2] = 0;
            return r;
        }

        private IEnumerable<InputAlias> GetAliases()
        {
            foreach (var Src in AliasedControls.Values)
            {
                foreach (var alias in Src.Values)
                {
                    yield return alias;
                }
            }
            yield break;
        }

        //Handles Events and puts the values in the proper alias

        #region TrackingInput

        //int ANYKEY = 0;
        /// <summary>
        /// Returns true if any input was pressed. This includes non-aliased inputs.
        /// </summary>
        public bool ANYKEY
        {
            get
            {
                return downHashes.Count > 0;
            }
        }

        private object __LOCKOBJ__ = new object();

        private HashSet<int> downHashes = new HashSet<int>();

        private void SetAliasValue(InputAlias alias, float value, uint timestamp)
        {
            lock (__LOCKOBJ__)
            {
                if (!CurrentValues.ContainsKey(alias.Alias))
                {
                    CurrentValues.Add(alias.Alias, value);
                    CurrentSince.Add(alias.Alias, timestamp);
                }
                else
                {
                    CurrentValues[alias.Alias] = value;
                    CurrentSince[alias.Alias] = timestamp;
                }
            }
        }

        internal void HandleKeyboardEvent(SDL_KeyboardEvent wEvent)
        {
            var dHIndx = 10000 + (int)wEvent.keysym.scancode;
            if (wEvent.state == 0)
                downHashes.Remove(dHIndx);
            else
                downHashes.Add(dHIndx);

            if (!AliasedControls.ContainsKey(InputSource.Keyboard)) return;
            if (!AliasedControls[InputSource.Keyboard].ContainsKey((int)(wEvent.keysym.scancode))) return;
            var alias = AliasedControls[InputSource.Keyboard][(int)wEvent.keysym.scancode];
            SetAliasValue(alias, wEvent.state, wEvent.timestamp);
        }

        internal void HandleGamepadButtonEvent(SDL_ControllerButtonEvent wEvent)
        {
            var dHIndx = 5000 + (int)(wEvent.button) + (int)SDL_GameControllerAxis.Max;
            if (wEvent.state == 0)
                downHashes.Remove(dHIndx);
            else
                downHashes.Add(dHIndx);

            if (!AliasedControls.ContainsKey(InputSource.Gamepad)) return;
            if (!AliasedControls[InputSource.Gamepad].ContainsKey((int)(wEvent.button) + (int)(SDL_GameControllerAxis.Max)))
                return;
            var alias = AliasedControls[InputSource.Gamepad][(int)(wEvent.button) + (int)SDL_GameControllerAxis.Max];
            SetAliasValue(alias, wEvent.state, wEvent.timestamp);
        }

        internal void HandleGamepadAxisEvent(SDL_ControllerAxisEvent wEvent)
        {
            if (!AliasedControls.ContainsKey(InputSource.Gamepad)) return;
            if (!AliasedControls[InputSource.Gamepad].ContainsKey((int)(wEvent.axis))) return;
            var alias = AliasedControls[InputSource.Gamepad][((int)wEvent.axis)];
            SetAliasValue(alias, Normalize(wEvent.value), wEvent.timestamp);
        }

        internal void HandleMouseButtonEvent(SDL_MouseButtonEvent wEvent)
        {
            var dHIndx = 1000 + (int)(wEvent.button);
            if (wEvent.state == 0)
                downHashes.Remove(dHIndx);
            else
                downHashes.Add(dHIndx);
            if (!AliasedControls.ContainsKey(InputSource.Mouse)) return;
            if (!AliasedControls[InputSource.Mouse].ContainsKey((int)(wEvent.button))) return;
            var alias = AliasedControls[InputSource.Mouse][((int)wEvent.button)];
            SetAliasValue(alias, wEvent.state, wEvent.timestamp);
        }

        private const int MouseWheelID = 1000;
        private const int MouseMoveXID = 1500;
        private const int MouseMoveYID = 1501;
        
        internal void HandleMouseMove(SDL_MouseMotionEvent mEvent)
        {
            if (!AliasedControls.ContainsKey(InputSource.Mouse)) return;
            var MDelta = new Vector2(mEvent.xrel, mEvent.yrel);
            Console.WriteLine($"{MDelta.X} {MDelta.Y}");

            if (Math.Abs(MDelta.X) > 0 && AliasedControls[InputSource.Mouse].ContainsKey(MouseMoveXID))
            {
                var alias = AliasedControls[InputSource.Mouse][MouseMoveXID];
                SetAliasValue(alias, Math.Sign(MDelta.X), mEvent.timestamp);
            }
            if (Math.Abs(MDelta.Y) > 0 && AliasedControls[InputSource.Mouse].ContainsKey(MouseMoveYID))
            {
                var alias = AliasedControls[InputSource.Mouse][MouseMoveYID];

                SetAliasValue(alias, Math.Sign(MDelta.Y), mEvent.timestamp);
            }
        }

        internal void HandleMouseWheelAxis(SDL_MouseWheelEvent wEvent)
        {
            if (!AliasedControls.ContainsKey(InputSource.Mouse)) return;
            if (!AliasedControls[InputSource.Mouse].ContainsKey(MouseWheelID)) return;
            var alias = AliasedControls[InputSource.Mouse][MouseWheelID];
            SetAliasValue(alias, Math.Sign(wEvent.y), wEvent.timestamp);
        }

        private float Normalize(short value)
        {
            return value < 0
                ? -(value / (float)short.MinValue)
                : (value / (float)short.MaxValue);
        }

        #endregion TrackingInput

        #endregion Instance
    }
}