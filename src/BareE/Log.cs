using System;

namespace BareE
{
    public static class Log
    {
        public static bool WriteTraceToConsole { get; set; } = true;
        public static bool WriteExceptionToConsole { get; set; } = true;

        public static void EmitError(Exception e)
        {
            if (WriteExceptionToConsole)
                Console.WriteLine(e);

            System.Media.SoundPlayer soundplayer = new System.Media.SoundPlayer();
        }

        internal static void EmitTrace(string v)
        {
            if (WriteTraceToConsole)
                Console.WriteLine(v);
        }
    }
}