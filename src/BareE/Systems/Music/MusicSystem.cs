using BareE.Components;
using BareE.GameDev;
using BareE.Messages;

using System;
using System.Collections.Generic;

using SharpAudio;
using SharpAudio.Codec;

namespace BareE.Systems
{
    /// <summary>
    /// A built in class allowing the use of Audio via a radio like interface.
    /// Responds to PlaySong Messages in the following manner:
    /// If station and song is specified then the station will be set and that specific song will be played.
    /// If a station is specified, the station will be set and a song from that station will be played.
    /// If neither a station nor a song is specifed a song from the current station will be played.
    /// </summary>
    public unsafe class MusicSystem : GameDev.GameSystem
    {
        private Radio Radio;
        private String CurrentStation;
        private int currentSongIndex = 0;
        AudioEngine musicEngine;

        SoundStream activeMusic;
        float currentVolume = 0.25f;
        bool allowMusic = true;

        public MusicSystem(string radiofile) : this(Newtonsoft.Json.JsonConvert.DeserializeObject<Radio>(AssetManager.ReadFile(radiofile)))
        {
            
        }

        public MusicSystem(Radio radio)
        {
            Radio = new Radio()
            {
                Stations = new Dictionary<string, Station>(radio.Stations, StringComparer.InvariantCultureIgnoreCase)
            };
        }

        private Queue<String> _recentlyPlayedSongs = new Queue<string>();
        private String _nextSongToPlay;

        private Random rng = new Random();

        private bool handlePlaySong(PlaySong msg, GameState state, Instant instant)
        {

            if (msg.Pause)
                if (activeMusic.State == SoundStreamState.Playing)
                    activeMusic.State = SoundStreamState.Paused;
                else if (activeMusic.State == SoundStreamState.Paused)
                    activeMusic.State = SoundStreamState.Playing;


            if (!String.IsNullOrEmpty(msg.Station))
                CurrentStation = msg.Station;

            var song = msg.filename;
            if (String.IsNullOrEmpty(CurrentStation))
                if (String.IsNullOrEmpty(song))
                    return true;

            switch (Radio.Stations[CurrentStation].PlayOrder)
            {
                case RadioStationPlayOrder.Sequential:
                    currentSongIndex = (currentSongIndex + 1) % (Radio.Stations[CurrentStation].PlayList.Length);
                    song = Radio.Stations[CurrentStation].PlayList[currentSongIndex];
                    break;

                case RadioStationPlayOrder.Shuffle:
                    while (String.IsNullOrEmpty(song))
                    {
                        song = Radio.Stations[CurrentStation].PlayList[rng.Next(Radio.Stations[CurrentStation].PlayList.Length)];
                        if (_recentlyPlayedSongs.Contains(song)) song = String.Empty;
                        while (_recentlyPlayedSongs.Count > (Radio.Stations[CurrentStation].PlayList.Length / 2.0f))
                            _recentlyPlayedSongs.Dequeue();
                    }
                    break;
            }

            _recentlyPlayedSongs.Enqueue(song);
            _nextSongToPlay = song;
            return true;
        }

        private bool handleChangeSetting(ChangeSetting setting, GameState state, Instant instant)
        {
            switch (setting.Setting.ToLower())
            {
                case "music_volume":
                    currentVolume = float.Parse(setting.Value);
                    if (activeMusic != null)
                        activeMusic.Volume = currentVolume;
                    break;
                case "music_enabled":
                    allowMusic = bool.Parse(setting.Value);
                    if (!allowMusic)
                    {
                        if (activeMusic.IsPlaying)
                        {
                            activeMusic.Stop();
                            activeMusic.Dispose();
                        }
                    }
                    break;
            }
            return true;
        }

        public override void Load(Instant Instant, GameState State, GameEnvironment Env)
        {

            musicEngine = AudioEngine.CreateDefault();
            
            State.Messages.AddListener<PlaySong>(handlePlaySong);
            State.Messages.AddListener<ChangeSetting>(handleChangeSetting);
            State.ECC.SpawnEntity("MusicCmds", new ConsoleCommandSet()
            {
                SetName = "Music",
                Commands = new ConsoleCommand[]
                {
                    new ConsoleCommand()
                    {
                       Cmd="DescRadio",
                       HelpText="Show Radio stations and songs",
                       Callback=new Func<string, GameState, Instant, object[]>((a,g,i)=>
                       {
                           List<Object> ret=new List<object>();
                           ret.Add($"Current Station: {CurrentStation}");
                           ret.Add($"{Newtonsoft.Json.JsonConvert.SerializeObject(Radio, Newtonsoft.Json.Formatting.Indented)}");

                           return ret.ToArray();
                       })
                    }
                }
            });
        }

        private bool playerHasFinished = false;

        private void Player_PlaybackFinished(object sender, EventArgs e)
        {
            playerHasFinished = true;
        }

        public override void Update(Instant Instant, GameState State, GameEnvironment Env)
        {
            if (playerHasFinished)
            {
                if (!String.IsNullOrEmpty(CurrentStation))
                {
                    if (Radio.Stations[CurrentStation].SongDelay == 0)
                        handlePlaySong(PlaySong.Next(), State, Instant);
                    else
                    {
                        State.Messages.EmitRealTimeDelayedMessage(Radio.Stations[CurrentStation].SongDelay, PlaySong.Next(), Instant);
                    }
                }
                playerHasFinished = false;
            }
            if (!String.IsNullOrEmpty(_nextSongToPlay))
            {
                if (activeMusic!=null && activeMusic.State== SoundStreamState.Playing)
                {
                    activeMusic.Stop();

 //                   activeMusic.Dispose();
                }
                activeMusic = new SoundStream(System.IO.File.OpenRead(_nextSongToPlay), musicEngine);
                System.Threading.Tasks.Task.Run(() =>
                {

                    System.Threading.Thread.Sleep(100);
                    activeMusic.Volume = currentVolume;
                    if (allowMusic)
                        activeMusic.Play();

                });
                
//                player.Play(_nextSongToPlay);
                _nextSongToPlay = null;
            }
            if (activeMusic!=null)
            switch(activeMusic.State)
            {
                case SoundStreamState.Playing:
                    break;
                case SoundStreamState.PreparePlay:
                    break;
                case SoundStreamState.TrackFinished:
                    playerHasFinished = true;
                    break;
                case SoundStreamState.Idle:
                    break;
            }
        }

        public override void Unload()
        {
           // if (player.Playing)
          //      player.Stop();
        }
    }
}