using BareE.Messages;

using System;

namespace BareE.Messages
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
}