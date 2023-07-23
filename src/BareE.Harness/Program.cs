using BareE.Calculator;
using BareE.DataStructures;
using BareE.GameDev;
using BareE.GUI.EZText;

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

            AttributeCollectionDeserializer.CreateLexer = createlexer;


            var engine = new BareE.Engine();
            
            var g = new TestGame(new GameState(), GameEnvironment.Load());
            engine.Run(g);
        }
        static Lexer createlexer()
        {
            Lexer r = new Lexer();
            r.AllowNegativeNumbers = true;
            return r;
        }

    }
}