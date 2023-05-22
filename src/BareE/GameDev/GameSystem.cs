using System;
using System.Numerics;

using Veldrid;
using Veldrid.Sdl2;

namespace BareE.GameDev
{
    /// <summary>
    /// Allows extensibility of reusable Game Systems.
    /// </summary>
    public abstract class GameSystem
    {
        public virtual void Load(Instant Instant, GameState State, GameEnvironment Env)
        {
        }

        public virtual void Initialize(Instant Instant, GameState State, GameEnvironment Env)
        {
        }

        public virtual void Update(Instant Instant, GameState State, GameEnvironment Env)
        {
        }

        public virtual void RenderEye(Instant Instant, GameState State, GameEnvironment Env, Matrix4x4 eyeMat, Framebuffer outbuffer, CommandList cmds)
        {
        }

        public virtual void RenderHud(Instant Instant, GameState State, GameEnvironment Env, Framebuffer outbuffer, CommandList cmds)
        {
        }

        public virtual void Unload()
        {
        }

        public virtual void OnHudRefresh(Instant instant, GameState state, GameEnvironment env)
        {
            
        }
        public virtual void OnResize(Instant instant, GameState state, GameEnvironment env)
        {

        }
        public virtual bool HandleControllerAxisMotion(SDL_ControllerAxisEvent axisEvent)
        {
            return false;
        }
        public virtual bool HandleControllerButtonEvent(SDL_ControllerButtonEvent axisEvent) { return false; }
        public virtual bool HandleKeyboardEvent(SDL_KeyboardEvent keyboardEvent) { return false; }
        public virtual bool HandleMouseButtonEvent(SDL_MouseButtonEvent mouseButtonEvent) { return false; }
        public virtual bool HandleMouseMotion(SDL_MouseMotionEvent mouseMotionEvent) { return false; }
        public virtual bool HandleMouseWheel(SDL_MouseWheelEvent mouseWheelEvent) { return false; }

    }
}