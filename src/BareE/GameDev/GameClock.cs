using System.Diagnostics;

namespace BareE.GameDev
{
    public class GameClock
    {
        private Stopwatch totalDuration;
        private Stopwatch currentPauseLength;
        private long _turnStartTime = 0;
        private long _tickStartTime = 0;
        private long prevPauseLength;
        private long prevTickLength = 0;
        private long _gameTick;
        public long GameTick { get { return _gameTick; } }
        public long SessionDuration { get { return totalDuration.ElapsedMilliseconds; } }
        public long EffectiveDuration { get { return totalDuration.ElapsedMilliseconds - currentPauseLength.ElapsedMilliseconds; } }
        public bool IsPaused { get { return currentPauseLength.IsRunning; } }

        public GameClock()
        {
            totalDuration = new Stopwatch();
            currentPauseLength = new Stopwatch();
            totalDuration.Start();
        }

        public void Pause()
        {
            if (currentPauseLength.IsRunning)
                return;
            currentPauseLength.Start();
        }

        public void Unpause()
        {
            if (!currentPauseLength.IsRunning)
                return;
            currentPauseLength.Stop();
        }

        public int Turn { get; private set; }

        public void AdvanceTurn()
        {
            Turn++; _turnStartTime = SessionDuration;
        }

        public void AdvanceTick()
        {
            _gameTick++; prevTickLength = SessionDuration - _tickStartTime; _tickStartTime = SessionDuration;
        }

        public Instant CaptureInstant()
        {
            return new Instant(SessionDuration, EffectiveDuration, IsPaused, Turn, prevTickLength);
        }
    }

    public struct Instant
    {
        /// <summary>
        /// Total MS of session since start of game.
        /// </summary>
        public long SessionDuration { get; private set; }

        /// <summary>
        /// Total MS of unpaused session since start of game.
        /// </summary>
        public long EffectiveDuration { get; private set; }

        /// <summary>
        /// Total MS since previous tick caputure.
        /// </summary>
        public long TickDelta { get; private set; }

        public bool IsPaused { get; private set; }
        public int Turn { get; private set; }

        public Instant(long sessionDuration, long effectiveDuration, bool isPaused, int turn, long pvTDur)
        {
            SessionDuration = sessionDuration;
            EffectiveDuration = effectiveDuration;
            IsPaused = isPaused;
            TickDelta = pvTDur;
            Turn = turn;
        }
    }
}