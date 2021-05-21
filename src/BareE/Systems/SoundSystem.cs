using BareE.GameDev;
using BareE.Messages;

using System;
using System.Collections.Generic;

namespace BareE.Systems
{
    [MessageAttribute("PlaySFX")]
    public struct PlaySFX
    {
        public PlaySFX(String title)
        {
            Resource = title;
        }

        public String Resource;
    }

    /// <summary>
    /// A built in system allowing the use of sound effects. 
    /// Responds to PlaySFX messages.
    /// </summary>
    public class SoundSystem : GameDev.GameSystem
    {
        private Dictionary<String, System.Media.SoundPlayer> soundboard;
        private System.Media.SoundPlayer player = new System.Media.SoundPlayer();

        private bool doEmitSound(PlaySFX req, GameState state, Instant instant)
        {
            state.Messages.EmitMsg(new ConsoleInput($"Playing {req.Resource}"));
            if (!soundboard.ContainsKey(req.Resource))
                soundboard.Add(req.Resource, new System.Media.SoundPlayer(AssetManager.FindFileStream(req.Resource)));
            soundboard[req.Resource].Play();
            return true;
        }

        public override void Load(Instant Instant, GameState State, GameEnvironment Env)
        {
            soundboard = new Dictionary<string, System.Media.SoundPlayer>();
            State.Messages.AddListener<PlaySFX>(doEmitSound);
        }
    }
}