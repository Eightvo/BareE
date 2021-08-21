using BareE.DataStructures;
using BareE.GameDev;

using SharpAudio;
using SharpAudio.Codec;

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace BareE.Harness
{
    public class Program
    {
        public static void Main(String[] args)
        {

            EntityComponentContext ECC = new EntityComponentContext();
            ECC.SpawnFromAsset("ABC", "BareE.Harness.Assets.Def.humanmale.actor");

            var engine = new BareE.Engine();
            var g = new TestGame(new GameState(), GameEnvironment.Load());
            engine.Run(g);
        }
    }
}