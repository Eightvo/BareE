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

            EntityComponentContext ecc = new EntityComponentContext();
            ecc.SpawnEntity("ABC", new Components.EZModel());

            EntityComponentContext ecc2 = new EntityComponentContext();
            ecc2.SpawnEntity("QRS", new BareE.Components.Pos() { Position = new System.Numerics.Vector3(10, 0, 0) }); ;

            ecc.Merge(ecc2);

            var qEnt = ecc.Entities["QRS"];
            Console.WriteLine($"{ecc.Components.GetComponent<BareE.Components.Pos>(qEnt).Position}");

            var engine = new BareE.Engine();
            var g = new TestGame(new GameState(), GameEnvironment.Load());
            engine.Run(g);
        }
    }
}