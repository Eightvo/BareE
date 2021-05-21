using BareE.Components;
using BareE.GameDev;
using BareE.Messages;

using Newtonsoft.Json.Converters;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BareE.Systems
{
    [MessageAttribute("PlaySong")]
    public struct PlaySong
    {
        public PlaySong(String station, String title)
        {
            Station = station; filename = title; Pause = false; Stop = false;
        }

        public String filename;
        public bool Pause;
        public bool Stop;
        public String Station;

        public static PlaySong Paused()
        {
            return new PlaySong() { Pause = true };
        }

        public static PlaySong UnPaused()
        {
            return new PlaySong() { Pause = false };
        }

        public static PlaySong Next()
        {
            return new PlaySong();
        }

        public static PlaySong Stopped()
        {
            return new PlaySong { Stop = true };
        }

        public static PlaySong SetStation(String station)
        {
            return new PlaySong() { Station = station };
        }
    }

    public class MusicSystem : GameDev.GameSystem
    {
        private NetCoreAudio.Player player;
        private Radio Radio;
        private String CurrentStation;
        private int currentSongIndex = 0;

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

        private Random rng = new Random();

        private bool handlePlaySong(PlaySong msg, GameState state, Instant instant)
        {
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

            player.Play(song);
            return true;
        }

        public override void Load(Instant Instant, GameState State, GameEnvironment Env)
        {
            player = new NetCoreAudio.Player();
            player.PlaybackFinished += Player_PlaybackFinished;
            State.Messages.AddListener<PlaySong>(handlePlaySong);
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
            }); ; ;
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
                        // Console.WriteLine($"{Radio.Stations[CurrentStation].SongDelay} {Instant.SessionDuration}");
                        State.Messages.EmitRealTimeDelayedMessage(Radio.Stations[CurrentStation].SongDelay, PlaySong.Next(), Instant);
                    }
                }
                playerHasFinished = false;
            }
        }

        public override void Unload()
        {
            if (player.Playing)
                player.Stop();
        }
    }

    public enum RadioStationPlayOrder
    {
        Sequential,
        Shuffle
    }

    public class Radio
    {
        public Dictionary<String, Station> Stations { get; set; }
    }

    public class Station
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public RadioStationPlayOrder PlayOrder { get; set; }

        public int SongDelay { get; set; }
        public string[] PlayList { get; set; }
    }
}