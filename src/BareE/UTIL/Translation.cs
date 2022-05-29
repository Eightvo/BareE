using BareE.DataStructures;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BareE.UTIL
{
    public class Translator
    {
        [JsonPropertyName("")]
        public String Unkown { get; set; }
        public Dictionary<String, String> Phrases { get; set; }

    }
    public static class Translation
    {
        static String ActiveTranslation = "En-US";
        public static IEnumerable<String> AvailableTranslators { get { return Translations.Keys.ToList(); } }
        static Dictionary<String, AttributeCollection> Translations;
        static Translation()
        {
        }
        public static void Initialize()
        {
            Translations = new Dictionary<string, AttributeCollection>();
            foreach (var d in System.IO.Directory.GetDirectories($"./Assets/Translations"))
            {
                foreach (var f in System.IO.Directory.GetFiles(d))
                {
                    ReadFile(f);
                }
            }
        }
        static void ReadFile(String filename)
        {
            AttributeCollection ac = AttributeCollectionDeserializer.FromAsset(filename);

            foreach (var k in ac.Attributes)
            {
                if (!Translations.ContainsKey(k.AttributeName))
                    Translations.Add(k.AttributeName, (AttributeCollection)k.Value);
                else
                {
                    Translations[k.AttributeName].Merge((AttributeCollection)k.Value);
                }

            }
        }
        public static String Translate(String txt, object[] args)
        {
            String phrase = (String)Translations[ActiveTranslation][$"PHRASE.{txt}"];
            List<String> tArgs = new List<String>();
            foreach (var a in args)
            {
                switch (a)
                {
                    case int i: tArgs.Add(i.ToString()); break;
                    case decimal d: tArgs.Add(d.ToString()); break;
                    case long l: tArgs.Add(l.ToString()); break;
                    case double db: tArgs.Add(a.ToString()); break;
                    case float f: tArgs.Add(f.ToString()); break;
                    case string s:
                        {
                            String r = (String)Translations[ActiveTranslation][s];
                            if (String.IsNullOrEmpty(r))
                                r = (String)Translations[ActiveTranslation]["Default"];
                            tArgs.Add((String)Translations[ActiveTranslation][s]); break;
                        }
                    default:
                        tArgs.Add((String)Translations[ActiveTranslation]["Default"]);
                        break;
                }
            }
            return String.Format(phrase, tArgs.ToArray());
        }
    }
}
