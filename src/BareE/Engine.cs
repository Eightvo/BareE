using BareE.GameDev;
using BareE.Messages;

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using Veldrid.Sdl2;

namespace BareE
{
    public class Engine
    {
        private Game ActiveGame;

        private GameEnvironment ActiveEnvironment
        { get { return ActiveGame.Environment; } }

        private GameState ActiveState
        { get { return ActiveGame.State; } }

        private bool isRunning = true;
        private GameSceneBase onDeckScene;
        private GameState onDeckState;
        private bool isTransitioning = false;
        private object MessageDispatchLock = new object();

        private void LoadGameSceneInBackground(Messages.TransitionScene transition, GameState state, Instant instant)
        {
            try
            {
                transition.State.Messages.AddListener<Messages.ExitGame>(HandleExitGameMessage);
                transition.State.Messages.AddListener<Messages.TransitionScene>(HandleTransitionScene);

                transition.Scene.DoLoad(instant, transition.State, ActiveEnvironment);
                transition.Scene.DoInitialize(instant, transition.State, ActiveEnvironment);
                onDeckScene = transition.Scene;
                onDeckState = transition.State;
            }catch(Exception e)
            {
                Log.EmitError(e);
            }
            finally{

            }
        }

        private void DispatchMessagesInBackground(MessageQueue queue, Instant instant, GameState state)
        {
            lock (MessageDispatchLock)
            {
                queue.ProcessMessages(instant, state);
            }
        }

        private bool HandleTransitionScene(Messages.TransitionScene transition, GameState state, Instant instant)
        {
            if (!isTransitioning)
                isTransitioning = true;
            else
                return true;
            Task.Run(() => LoadGameSceneInBackground(transition, transition.State, instant));
            return true;
        }

        private bool HandleExitGameMessage(Messages.ExitGame msg, GameState state, Instant instant)
        {
            isRunning = false;
            return true;
        }

        /// <summary>
        /// Runs a Game.
        /// </summary>
        /// <param name="game"></param>
        public void Run(Game game)
        {
            ActiveGame = game;
            //ImGuiNET.ImGui.GetIO().ConfigFlags = ImGuiNET.ImGuiConfigFlags.
            game.Environment.Window.Window.Resized += Window_Resized;
            Instant instant = game.State.Clock.CaptureInstant();
            game.State.Messages.AddListener<Messages.ExitGame>(HandleExitGameMessage);
            game.State.Messages.AddListener<Messages.TransitionScene>(HandleTransitionScene);
            game.ActiveScene.DoLoad(instant, game.State, game.Environment);
            game.ActiveScene.DoInitialize(instant, game.State, game.Environment);
            Sdl2Events.Subscribe(HandleEvent);
            float cummulativeDelta = 0;
            while (isRunning && game.Environment.Window.Window.Exists)
            {
                if (onDeckScene != null)
                {
                    ActiveGame.ActiveScene.Dispose();
                    ActiveGame.ActiveScene = onDeckScene;
                    ActiveGame.State = onDeckState;
                    onDeckScene = null;
                    isTransitioning = false;
                }
                game.State.Clock.AdvanceTick();
                instant = game.State.Clock.CaptureInstant();
                cummulativeDelta += instant.TickDelta;
                if (Monitor.TryEnter(MessageDispatchLock))
                {
                    try
                    {
                        Task.Run(() => DispatchMessagesInBackground(game.State.Messages, instant, game.State));
                    }
                    finally
                    {
                        Monitor.Exit(MessageDispatchLock);
                    }
                }
                game.ActiveScene.DoUpdate(instant, game.State, game.Environment);
                var ss = game.Environment.Window.Window.PumpEvents();
                game.Environment.Window.IGR.Update((float)instant.TickDelta / 1000f, ss);
                game.ActiveScene.DoRender(instant, game.State, game.Environment);
            }

            game.Environment.Window.Window.Close();
            game.ActiveScene.Dispose();
            Log.EmitTrace($"Exit game");
        }

        private void Window_Resized()
        {
            ActiveGame.Environment.Window.Width = ActiveGame.Environment.Window.Window.Width;
            ActiveGame.Environment.Window.Height= ActiveGame.Environment.Window.Window.Height;

        }

        private void HandleEvent(ref SDL_Event ev)
        {
            //if (ev.type!= SDL_EventType.MouseMotion)
            //    Log.EmitTrace($"<event> Window:[{ev.windowID}] Type:{ev.type} Timestamp:{ev.timestamp}");
            switch (ev.type)
            {
                //case SDL_EventType.Quit:
                //     ActiveGame.ExecutionState = GameExecutionState.Quitting;
                case SDL_EventType.FirstEvent:
                case SDL_EventType.Terminating:

                case SDL_EventType.AudioDeviceAdded:
                case SDL_EventType.AudioDeviceRemoved:
                case SDL_EventType.ControllerDeviceAdded:
                case SDL_EventType.ControllerDeviceRemoved:
                case SDL_EventType.ControllerDeviceRemapped:
                case SDL_EventType.JoyDeviceAdded:
                case SDL_EventType.JoyDeviceRemoved:
                case SDL_EventType.KeyMapChanged:

                case SDL_EventType.DidEnterBackground:
                case SDL_EventType.DidEnterForeground:

                case SDL_EventType.DollarGesture:
                case SDL_EventType.DollarRecord:

                case SDL_EventType.ClipboardUpdate:
                case SDL_EventType.DropBegin:
                case SDL_EventType.DropComplete:
                case SDL_EventType.DropFile:
                case SDL_EventType.DropText:

                case SDL_EventType.FingerDown:
                case SDL_EventType.FingerUp:
                case SDL_EventType.JoyHatMotion:
                    break;

                case SDL_EventType.JoyAxisMotion:
                case SDL_EventType.JoyBallMotion:
                case SDL_EventType.FingerMotion:
                    break;

                case SDL_EventType.ControllerAxisMotion:
                    {
                        SDL_ControllerAxisEvent wEvent = Unsafe.As<SDL_Event, SDL_ControllerAxisEvent>(ref ev);
                        // Console.WriteLine($"Axis: {wEvent.axis} {wEvent.value} {wEvent.which} {wEvent.type} {wEvent.timestamp}");
                        ActiveState.Input.HandleGamepadAxisEvent(wEvent);
                    }
                    break;

                case SDL_EventType.ControllerButtonDown:
                case SDL_EventType.ControllerButtonUp:
                case SDL_EventType.JoyButtonDown:
                case SDL_EventType.JoyButtonUp:
                    {
                        SDL_ControllerButtonEvent wEvent = Unsafe.As<SDL_Event, SDL_ControllerButtonEvent>(ref ev);
                        ActiveState.Input.HandleGamepadButtonEvent(wEvent);
                    }
                    break;

                case SDL_EventType.KeyUp:
                case SDL_EventType.KeyDown:
                    {
                        if (ImGuiNET.ImGui.GetIO().WantCaptureKeyboard) break;
                        SDL_KeyboardEvent wEvent = Unsafe.As<SDL_Event, SDL_KeyboardEvent>(ref ev);
                        ActiveState.Input.HandleKeyboardEvent(wEvent);
                    }
                    break;

                case SDL_EventType.MouseButtonDown:
                case SDL_EventType.MouseButtonUp:
                    {
                        SDL_MouseButtonEvent wEvent = Unsafe.As<SDL_Event, SDL_MouseButtonEvent>(ref ev);
                        ActiveState.Input.HandleMouseButtonEvent(wEvent);
                    }
                    break;

                case SDL_EventType.MouseMotion:
                    {
                        SDL_MouseMotionEvent wEvent = Unsafe.As<SDL_Event, SDL_MouseMotionEvent>(ref ev);
                        ActiveState.Input.HandleMouseMove(wEvent);
                    }
                    break;

                case SDL_EventType.MouseWheel:
                    {
                        SDL_MouseWheelEvent wEvent = Unsafe.As<SDL_Event, SDL_MouseWheelEvent>(ref ev);
                        ActiveState.Input.HandleMouseWheelAxis(wEvent);
                    }
                    break;

                case SDL_EventType.LastEvent:
                case SDL_EventType.LowMemory:
                case SDL_EventType.MultiGesture:
                case SDL_EventType.RenderDeviceReset:
                case SDL_EventType.RenderTargetsReset:
                case SDL_EventType.TextEditing:
                case SDL_EventType.TextInput:
                case SDL_EventType.UserEvent:
                case SDL_EventType.WillEnterBackground:
                case SDL_EventType.WillEnterForeground:
                case SDL_EventType.SysWMEvent:
                    break;

                case SDL_EventType.WindowEvent:
                    {
                    }
                    break;
            }
        }
    }
}