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



            LoadZone(@"G:\TestData\PGonGen\zone1.zone");

            AttributeCollection myColl = new AttributeCollection();
            AttributeCollection myColl2 = new AttributeCollection();
            myColl["Asset"] = "Asset.One.Two";
            myColl["Age"] = 5;
            myColl["Percent"] = 0.3f;
            myColl["V"] = new System.Numerics.Vector2(10, 320);

            AttributeCollection inner = new AttributeCollection();
            inner["What"] = "Who";
            inner["How"] = 4;
            myColl["I"] = inner;

            Console.WriteLine($"{myColl["I.How"]}");


            using (BinaryWriter wtr = new BinaryWriter(new FileStream(@"G:\TestData\TestS.bin", FileMode.OpenOrCreate)))
            {
                wtr.WritePrimative(myColl);
            }
            using (BinaryReader rdr = new BinaryReader(new FileStream(@"G:\TestData\TestS.bin", FileMode.Open)))
            {
                myColl2=rdr.ReadAttributeCollection();
            }


            Console.WriteLine($"{myColl2["I.How"]}");
            EntityComponentContext ecc = new EntityComponentContext();
            ecc.SpawnEntity("ABC", new Components.EZModel());

            EntityComponentContext ecc2 = new EntityComponentContext();
            ecc2.SpawnEntity(new BareE.Components.Pos() { Position = new System.Numerics.Vector3(10, 0, 0) });

            ecc.Merge(ecc2);

            using (BinaryWriter wtr = new BinaryWriter(new FileStream(@"G:\TestData\TestS.bin", FileMode.OpenOrCreate)))
            {
                EntityComponentContext.WriteToStream(wtr, ecc);
            }
            using (BinaryReader rdr = new BinaryReader(new FileStream(@"G:\TestData\TestS.bin", FileMode.Open)))
            {
                EntityComponentContext ecc3 = EntityComponentContext.ReadFromStream(rdr);
            }

            var qEnt = ecc.Entities["QRS"];
            Console.WriteLine($"{ecc.Components.GetComponent<BareE.Components.Pos>(qEnt).Position}");

            var engine = new BareE.Engine();
            var g = new TestGame(new GameState(), GameEnvironment.Load());
            engine.Run(g);
        }
    }
}