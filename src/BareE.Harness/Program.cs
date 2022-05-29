using BareE.Calculator;
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
        public static void LoadZone(String filename)
        {

            List<AttributeCollection> zoneReferences = new List<AttributeCollection>();
            AttributeCollection zoneBiome = null;
            //MapData zoneMap = null;
            EntityComponentContext ecc = null;
            using (BinaryReader rdr = new BinaryReader(new FileStream(filename, FileMode.Open)))
            {
                if (rdr.ReadBoolean())
                    while (rdr.ReadBoolean())
                        zoneReferences.Add((AttributeCollection)rdr.ReadAttributeCollection());
                if (rdr.ReadBoolean())
                    zoneBiome = (AttributeCollection)rdr.ReadAttributeCollection();

                if (rdr.ReadBoolean())
                {
                    var h=rdr.ReadInt32();
                    var w=rdr.ReadInt32();
                    for (int i = 0; i < h * w; i++) rdr.ReadSingle();
                    for (int i = 0; i < h*3 * w*3; i++) rdr.ReadSingle();
                }
                //if (rdr.ReadBoolean())
                //    zoneMap = MapData.ReadFromStream(rdr);

                if (rdr.ReadBoolean())
                {
                    ecc = EntityComponentContext.ReadFromStream(rdr);
                }
            }
        }

        public static void Main(String[] args)
        {
            var engine = new BareE.Engine();
            var g = new TestGame(new GameState(), GameEnvironment.Load());
            engine.Run(g);
        }
    }
}