using BareE.GameDev;
using BareE.Messages;

using SharpAudio;
using SharpAudio.Codec;

using System;
using System.Collections.Generic;
using System.IO;

namespace BareE.Systems
{
    [MessageAttribute("PlaySFX")]
    public struct PlaySFX
    {
        public PlaySFX(String title)
        {
            Resource = title;
            Volume = 1.0f;
        }

        public String Resource;
        public float Volume;
    }

    /// <summary>
    /// A built in system allowing the use of sound effects.
    /// Responds to PlaySFX messages.
    /// </summary>
    public class SoundSystem : GameDev.GameSystem
    {
        private Dictionary<String, MemoryStream> soundboard;


        AudioEngine sfxEngine;
        SoundSink sfxSink;
        float currentVolume = 0.5f;
        bool allowSFX = true;

        public  bool doEmitSound(PlaySFX req, GameState state, Instant instant)
        {
            if (!allowSFX)
                return true;

            
            if (!soundboard.ContainsKey(req.Resource))
                soundboard.Add(req.Resource, (MemoryStream)AssetManager.FindFileStream(req.Resource));

            var mStr = new MemoryStream(soundboard[req.Resource].ToArray());
            var sfx = new SoundStream(mStr, sfxSink, false);
            System.Threading.Tasks.Task.Run(() => 
            {
                System.Threading.Thread.Sleep(100);
                sfx.Volume = (currentVolume * req.Volume==0?1:req.Volume);//Should be at most 1*1. Assume a sfx wouldn't be called if it were to be silent.
                sfx.Play();
            });
            
            return true;
        }
        private bool handleChangeSettings(ChangeSetting req, GameState state, Instant instant)
        {
            switch (req.Setting.ToLower())
            {
                case "sfx_volume":
                    currentVolume = float.Parse(req.Value);
                    break;
                case "sfx_enabled":
                    allowSFX = bool.Parse(req.Value);
                    break;
            }
            return true;
        }

        public override void Load(Instant Instant, GameState State, GameEnvironment Env)
        {

            sfxEngine = AudioEngine.CreateDefault();
            sfxSink = new SoundSink(sfxEngine, null);
            soundboard = new Dictionary<string, MemoryStream>();
            State.Messages.AddListener<PlaySFX>(doEmitSound);
            State.Messages.AddListener<ChangeSetting>(handleChangeSettings);
        }
    }
}