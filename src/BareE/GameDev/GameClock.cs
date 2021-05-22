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

        /// <summary>
        /// Current Game Tick
        /// </summary>
        public long GameTick { get { return _gameTick; } }

        /// <summary>
        /// Number of milliseconds since the game was first started.
        /// </summary>
        public long SessionDuration { get { return totalDuration.ElapsedMilliseconds; } }

        /// <summary>
        /// Number of milliseconds since the game was started while the game was not paused.
        /// </summary>
        public long EffectiveDuration { get { return totalDuration.ElapsedMilliseconds - currentPauseLength.ElapsedMilliseconds; } }

        /// <summary>
        /// Returns true if the paused state has been activated.
        /// </summary>
        public bool IsPaused { get { return currentPauseLength.IsRunning; } }

        public GameClock()
        {
            totalDuration = new Stopwatch();
            currentPauseLength = new Stopwatch();
            totalDuration.Start();
        }

        /// <summary>
        /// Activate the paused state.
        /// </summary>
        public void Pause()
        {
            if (currentPauseLength.IsRunning)
                return;
            currentPauseLength.Start();
        }

        /// <summary>
        /// Deactivate the paused state.
        /// </summary>
        public void Unpause()
        {
            if (!currentPauseLength.IsRunning)
                return;
            currentPauseLength.Stop();
        }

        /// <summary>
        /// Current Turn.
        /// </summary>
        public int Turn { get; private set; }

        /// <summary>
        /// Increase Turn
        /// </summary>
        public void AdvanceTurn()
        {
            Turn++; _turnStartTime = SessionDuration;
        }

        public void AdvanceTick()
        {
            _gameTick++; prevTickLength = SessionDuration - _tickStartTime; _tickStartTime = SessionDuration;
        }

        /// <summary>
        /// Obtain a stable snapshot of time.
        /// </summary>
        /// <returns></returns>
        public Instant CaptureInstant()
        {
            return new Instant(SessionDuration, EffectiveDuration, IsPaused, Turn, prevTickLength);
        }
    }

    /// <summary>
    /// Represents a stable snapshot of
    /// </summary>
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