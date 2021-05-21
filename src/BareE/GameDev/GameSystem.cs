using System.Numerics;

using Veldrid;

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
    }
}