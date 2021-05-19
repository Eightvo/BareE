
using BareE.GameDev;

using System;

namespace BareE.Harness
{
    public class Program
    {
        public static void Main(String[] args)
        {
            var engine = new BareE.Engine();
            var g = new TestGame(new GameState(), GameEnvironment.Load());
            engine.Run(g);
        }

    }
}
