using BareE.DataStructures;
using BareE.GameDev;
using BareE.Messages;

using System;
using System.Collections.Generic;
using System.Linq;

namespace BareE.Systems
{
    public partial class ConsoleSystem
    {
        object[] EmitMsgFunc(String a, GameState s, Instant i)
        {
            a = a.Trim();
            var sInd = a.Trim().IndexOf(' ');
            if (sInd < 0) return new String[] { "Expected <MsgName> <JsonDef>" };
            var typeName = a.Substring(0, sInd).Trim();
            var jsonDef = a.Substring(sInd).Trim();
            var v = MessageQueue._messageAliasMap[typeName];

            List<String> ret = new List<string>();
            if (v == null) return (new String[] { $"Could not find message {a}" });

            try
            {
                var msg = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonDef, v.OriginatingType);
                s.Messages.EmitRealTimeDelayedMessage(1, msg, i);
                ret.Add("Done");
            }
            catch (Exception e)
            {
                ret.Add(e.Message);
            }

            ret.Add(a);
            return ret.ToArray();
        }

        object[] ListAssetsFunc(String args, GameState state, Instant instant)
        {
            List<String> ret = new List<string>();
            foreach(var v in AssetManager.AllFiles(args.Trim()))
            {
                ret.Add(v);
            }
            return ret.ToArray();
        }

        object[] DescMsgFunc(String args, GameState state, Instant instant)
        {
            var v = MessageQueue._messageAliasMap[args.Trim()];
            if (v == null) return (new String[] { $"Could not find message {args}" });
            List<String> ret = new List<string>();
            foreach (var prop in v.OriginatingType.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly).OrderBy(x => x.Name))
            {
                ret.Add($"[{prop.FieldType.Name}][{prop.Name}]");
            }
            ret.Add(args);
            return ret.ToArray();
        }
        object[] DescCompFunc(String args, GameState state, Instant instant)
        {
            var v = ComponentCache.ComponentAliasMap[args.Trim()];

            List<String> ret = new List<string>();
            if (v == null) return (new String[] { $"Could not find component {args}" });
            foreach (var prop in v.OriginatingType.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly).OrderBy(x => x.Name))
            {
                ret.Add($"[{prop.PropertyType.Name}][{prop.Name}]");
            }
            ret.Add(args);
            return ret.ToArray();
        }
        private object[] listallents(string a, GameState s, Instant i)
        {
            bool parentsOnly = a.Trim() != "-a";
            var ret = new List<object>();

            Dictionary<int, List<Entity>> psuedoTree = new Dictionary<int, List<Entity>>();
            Dictionary<int, String> nodeNames = new Dictionary<int, string>();
            foreach (var v in s.ECC.Entities.Keys())
            {
                nodeNames.Add(s.ECC.Entities[v].Id, v);
            }
            foreach (Entity v in s.ECC.Entities.Values())
            {
                if (v == null) continue;
                if (!psuedoTree.ContainsKey(v.Parent))
                    psuedoTree.Add(v.Parent, new List<Entity>());
                psuedoTree[v.Parent].Add(v);
            }
            foreach (var v in getChildEnts(psuedoTree, nodeNames, s.ECC, 0))
            {
                ret.Add(v);
            }
            ret.Insert(0, $"Found {ret.Count}");
            return ret.ToArray();
        }
        object[] DescEntFunc(string a, GameState s, Instant i)
        {
            var ret = new List<object>();
            a = a.Trim();
            int entId;
            Entity ent;
            if (!int.TryParse(a, out entId))
            {
                ent = s.ECC.Entities[a];
            }
            else
            {
                ent = s.ECC.Entities[entId];
            }
            if (ent == null) return new object[] { $"no entity {a} found." };
            foreach (var v in s.ECC.Components.GetComponentsByEntity(ent))
            {
                ret.Add(v.GetType().Name);
            }
            return ret.ToArray();
        }
        object[] DescEntCompFunc(string a, GameState s, Instant i)
        {
            var ret = new List<object>();
            String entRef=String.Empty;
            String compRef=String.Empty;

            Queue<LexerToken> tokenQ = new Queue<LexerToken>();
            foreach (var t in Lexer.DefaultLexer.Tokenize(a))
                tokenQ.Enqueue(t);
            while (tokenQ.Peek().Type == LexerToken.LexerTokenType.Whitespace)
                tokenQ.Dequeue();
            switch (tokenQ.Peek().Type)
            {
                case LexerToken.LexerTokenType.Integer_Literal:
                case LexerToken.LexerTokenType.Character_Literal:
                case LexerToken.LexerTokenType.String_Literal:
                case LexerToken.LexerTokenType.Identifier:
                    entRef = tokenQ.Dequeue().Text;
                    break;
                default:
                    return new String[] { "Expected <ent> <component>" };

            }
            while (tokenQ.Peek().Type == LexerToken.LexerTokenType.Whitespace)
                tokenQ.Dequeue();
            switch (tokenQ.Peek().Type)
            {
                case LexerToken.LexerTokenType.Integer_Literal:
                case LexerToken.LexerTokenType.Character_Literal:
                case LexerToken.LexerTokenType.String_Literal:
                case LexerToken.LexerTokenType.Identifier:
                    compRef = tokenQ.Dequeue().Text;
                    break;
                default:
                    return new String[] { "Expected <ent> <component>" };

            }

            return descEntComp(entRef, compRef, s);
        }

        object[] descEntComp(String entRef, String compRef, GameState state)
        {
            int entId;
            Entity ent;
            if (!int.TryParse(entRef, out entId))
                ent = state.ECC.Entities[entRef];
            else
                ent = state.ECC.Entities[entId];

            var v = ComponentCache.ComponentAliasMap[compRef];
            if (ent == null) return new String[] { $"Couldn't find entity {entRef}" };
            var cv = state.ECC.Components.GetComponent(ent, v.CTypeID);
            if (cv == null)
                return new String[] { "<NULL>" };
            return new String[]
            {
                Newtonsoft.Json.JsonConvert.SerializeObject(cv, Newtonsoft.Json.Formatting.Indented)
            };

        }

      
    }
}
