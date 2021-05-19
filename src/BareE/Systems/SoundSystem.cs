using BareE.GameDev;
using BareE.Messages;

using System;
using System.Collections.Generic;

namespace BareE.Systems
{
    [MessageAttribute("PlaySFX")]
    public struct PlaySFX
    {
        public PlaySFX(String title) { Resource = title; }
        public String Resource;
    }


    public class SoundSystem:GameDev.GameSystem
    {
        Dictionary<String, System.Media.SoundPlayer> soundboard;
        System.Media.SoundPlayer player = new System.Media.SoundPlayer();
        private bool doEmitSound(PlaySFX req, GameState state, Instant instant)
        {
            state.Messages.AddMsg(new ConsoleInput($"Playing {req.Resource}"));
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
