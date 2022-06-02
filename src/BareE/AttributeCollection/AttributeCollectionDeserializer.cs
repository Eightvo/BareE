using BareE;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BareE.DataStructures
{
    public static class AttributeCollectionDeserializer
    {
        public static Func<String,ParserState, String> TranslateReference = null;
        public static Func<BareE.Lexer> CreateLexer = null;
        public static String RawFile(String asset, AttributeCollection stateData = null)
        {
            return AssetManager.ReadFile(asset);

        }
        public static AttributeCollection FromAsset(String asset, object statData)
        {
            AttributeCollection ac = new AttributeCollection();
            ac["Data"] = statData;
            return FromAsset(asset, ac);
        }

        public static AttributeCollection FromAsset(String asset, AttributeCollection stateData = null)
        {
            if (stateData==null)
                stateData = new AttributeCollection();
            
            ParserState state = new ParserState() {
                Asset = asset,
                Data = stateData
            };

            AttributeCollection ac = new AttributeCollection();
            BareE.Lexer lexer;
            if (CreateLexer == null) lexer = new BareE.Lexer();
            else lexer = CreateLexer();

            var txt = String.Empty;
            try
            {
                txt = AssetManager.ReadFile(asset);
            } catch( Exception e)
            {
                throw new Exception($"Exception Creating From Asset {asset}");
            }
            var tokenList = lexer.Tokenize(txt).GetEnumerator();
            tokenList.MoveNext();
            ac = (AttributeCollection)ConsumeObject(tokenList, state);
            if (tokenList.MoveNext())
                Unexpected("End of File", tokenList.Current, state);
            return ac;
        }
        public static T FromAsset<T>(String asset, AttributeCollection stateData = null)
            where T : AttributeCollectionBase, new()
        {
            if (stateData == null)
                stateData = new AttributeCollection();
            ParserState state = new ParserState()
            {
                Asset = asset,
                Data = stateData
            };

            BareE.Lexer lexer = new BareE.Lexer();
            var tokenList = lexer.Tokenize(AssetManager.ReadFile(asset)).GetEnumerator();
            return ReadTokens<T>(tokenList, state);
        }
        public static AttributeCollection FromSrc(String src, object statData)
        {
            AttributeCollection ac = new AttributeCollection();
            ac["Data"] = statData;
            return FromSrc(src, ac);
        }
        public static AttributeCollection FromSrc(String src, AttributeCollection stateData = null)
        {
            if (stateData == null)
                stateData = new AttributeCollection();

            ParserState state = new ParserState()
            {
                Asset = src,
                Data = stateData
            };

            AttributeCollection ac = new AttributeCollection();
            BareE.Lexer lexer;
            if (CreateLexer==null) lexer=  new BareE.Lexer();
            else lexer= CreateLexer();

            var tokenList = lexer.Tokenize(src).GetEnumerator();
            tokenList.MoveNext();
            ac = (AttributeCollection)ConsumeObject(tokenList, state);
            if (tokenList.MoveNext())
                Unexpected("End of File", tokenList.Current, state);
            return ac;
        }
        public static T FromSrc<T>(String src, AttributeCollection stateData = null)
            where T : AttributeCollectionBase, new()
        {
            if (stateData == null)
                stateData = new AttributeCollection();
            ParserState state = new ParserState()
            {
                Asset = src,
                Data = stateData
            };

            BareE.Lexer lexer = new BareE.Lexer();
            var tokenList = lexer.Tokenize(src).GetEnumerator();
            return ReadTokens<T>(tokenList, state);
        }

        private static T ReadTokens<T>(IEnumerator<LexerToken> tokens, ParserState state)
             where T : AttributeCollectionBase, new()
        {

            AttributeCollection ac = new AttributeCollection();
            tokens.MoveNext();
            ac = (AttributeCollection)ConsumeObject(tokens, state);
            if (tokens.MoveNext())
                Unexpected("End of File", tokens.Current, state);
            var ret = new T();
            ret.UseData(ac);
            return ret;

        }



        public class ParserState
        {
            public String Asset { get; set; }
            public Dictionary<String, List<LexerToken>> macros = new Dictionary<String, List<LexerToken>>( StringComparer.CurrentCultureIgnoreCase);
            public AttributeCollection Data = new AttributeCollection();
        }

        private static void ConsumeWhitespace(IEnumerator<LexerToken> tokens, ParserState state)
        {
            while (tokens.Current.Type == LexerToken.LexerTokenType.Whitespace ||
                  tokens.Current.Type == LexerToken.LexerTokenType.Comment ||
                  (tokens.Current.Type == LexerToken.LexerTokenType.Operator && String.IsNullOrEmpty(tokens.Current.Text)) ||
                  (tokens.Current.Type == LexerToken.LexerTokenType.Unknown && tokens.Current.Text=="\r"))
            {
                bool mn = tokens.MoveNext();
                if (!mn) return;
            }
        }

        private static void Unexpected(String expected, LexerToken token, ParserState state)
        {
            throw new Exception($"Expected {expected}, found {token.Text} Line:{token.LineNumer}.{token.Character}. [{state.Asset}]");
        }

        private static int ReadInteger(IEnumerator<LexerToken> tokens, ParserState state)
        {
            var i = 0;
            switch (tokens.Current.Text[0])
            {
                default:
                    if (!int.TryParse(tokens.Current.Text, out i))
                        Unexpected("integer", tokens.Current, state);
                    break;
            }
            tokens.MoveNext();
            return i;
        }
        private static char ReadChar(IEnumerator<LexerToken> tokens, ParserState state)
        {
            char c = '?';
            if (tokens.Current.Text.Length == 1)
                c = tokens.Current.Text[0];
            else
            {
                Unexpected("actually just not implemented", tokens.Current, state);
            }
            tokens.MoveNext();
            return c;
        }
        private static bool ReadBoolean(IEnumerator<LexerToken> tokens, ParserState state)
        {
            var ret = false;
            if (tokens.Current.Text == "true")
                ret = true;
            tokens.MoveNext();
            return ret;
        }
        private static float ReadFloat(IEnumerator<LexerToken> tokens, ParserState state)
        {
            float f = 0;
            switch (tokens.Current.Text[0])
            {
                case '-':
                    tokens.MoveNext();
                    if (!float.TryParse(tokens.Current.Text, out f))
                        Unexpected("float", tokens.Current, state);
                    f = -f;
                    break;
                default:
                    if (!float.TryParse(tokens.Current.Text, out f))
                        Unexpected("float", tokens.Current, state);
                    break;
            }
            tokens.MoveNext();
            return f;
        }
        private static String ReadString(IEnumerator<LexerToken> tokens, ParserState state)
        {
            String f = tokens.Current.Text;
            tokens.MoveNext();
            return f;
        }

        private static String ReadReference(IEnumerator<LexerToken> tokens, ParserState state)
        {
            if (tokens.Current.Type == LexerToken.LexerTokenType.String_Literal)
                return ReadString(tokens, state);

            else
            {
                var sb = new StringBuilder();
                //sb.Append(tokens.Current.Text);
                if (tokens.Current.Type != LexerToken.LexerTokenType.Directive)
                {
                    sb.Append(tokens.Current.Text);
                }
                while (tokens.Current.Type== LexerToken.LexerTokenType.Directive || tokens.MoveNext())
                {
                    switch (tokens.Current.Type)
                    {
                        case LexerToken.LexerTokenType.Comment:
                            break;
                        case LexerToken.LexerTokenType.Directive:
                            tokens = ConsumeDirective(tokens, state);
                             sb.Append(tokens.Current.Text);
                            break;
                        case LexerToken.LexerTokenType.String_Literal:
                        case LexerToken.LexerTokenType.Character_Literal:
                        case LexerToken.LexerTokenType.Identifier:
                            sb.Append(tokens.Current.Text);
                            break;
                        case LexerToken.LexerTokenType.Operator:
                        case LexerToken.LexerTokenType.Unknown:
                            if (tokens.Current.Text == "." || tokens.Current.Text == "\\" || tokens.Current.Text == "/" || tokens.Current.Text==":")
                                sb.Append(tokens.Current.Text);
                            else if (tokens.Current.Text == ",")
                                return sb.ToString();
                            else
                                Unexpected("Reference", tokens.Current, state);
                            break;
                        default:
                            return sb.ToString();
                            break;
                    }

                } 
                return sb.ToString();
            }
        }
        private static object ReadVector(IEnumerator<LexerToken> tokens, ParserState state)
        {
            List<float> read = new List<float>();
            object ret=0;
            while(tokens.Current.Text!=">")
            {
                read.Add(ReadFloat(tokens, state));
                ConsumeWhitespace(tokens, state);
                if (tokens.Current.Text == ",")
                {
                    tokens.MoveNext();
                    ConsumeWhitespace(tokens, state);
                }
                //if (!tokens.MoveNext()) Unexpected("End of file", tokens.Current, state);
            }
            switch(read.Count())
            {
                case 2: ret =new Vector2(read[0], read[1]); break;
                case 3: ret =new Vector3(read[0], read[1], read[2]); break;
                case 4: ret =new Vector4(read[0], read[1], read[2],read[3]); break;
                default:
                    Unexpected($"Expected vectors with only 2, 3 or 4 dimentions. Found {read.Count()}", tokens.Current, state);
                    break;
            }
            tokens.MoveNext();
            return ret;
        }

        private static object[] BuildArray(IEnumerator<LexerToken> tokens, ParserState state)
        {
            tokens.MoveNext();
            ConsumeWhitespace(tokens, state);
            List<object> ret = new List<object>();
            while (!(tokens.Current.Type == LexerToken.LexerTokenType.Operator && tokens.Current.Text == "]"))
            {
                if (tokens.Current.Type== LexerToken.LexerTokenType.Operator && tokens.Current.Text==">")
                {
                    tokens.MoveNext();
                    ConsumeWhitespace(tokens, state);
                    var r = ReadReference(tokens, state);
                    var txt=String.Empty;
                    try
                    {
                        txt = AssetManager.ReadFile(r);
                    } catch(Exception e)
                    {
                        throw new Exception($"Issue attempting to read file {r}");
                    }
                    var pre = BareE.Lexer.DefaultLexer.Tokenize(txt).GetEnumerator();
                    var post = state.Asset;
                    state.Asset = r;
                    tokens = Join(pre , tokens, state,post).GetEnumerator();
                    tokens.MoveNext();
                    ConsumeWhitespace(tokens, state);
                }
                ret.Add(ConsumeObject(tokens, state));
                ConsumeWhitespace(tokens, state);
                if (tokens.Current.Type == LexerToken.LexerTokenType.Operator && tokens.Current.Text == ",")
                    tokens.MoveNext();

                ConsumeWhitespace(tokens, state);
            }
            tokens.MoveNext();
            return ret.ToArray();

        }

        private static Object ConsumeObject(IEnumerator<LexerToken> tokens, ParserState state)
        {
            ConsumeWhitespace(tokens, state);
            switch (tokens.Current.Type)
            {
                case LexerToken.LexerTokenType.Operator:
                    {
                        var opTxt = tokens.Current.Text;
                        switch (opTxt)
                        {
                            case "{":
                                return BuildAttributeCollection(tokens, state);
                                break;
                            case "[":
                                return (BuildArray(tokens, state));
                                break;
                            case "@":
                                tokens.MoveNext();
                                ConsumeWhitespace(tokens, state);
                                var r = ReadReference(tokens, state);
                                if (TranslateReference != null)
                                    r = TranslateReference(r,state);
                                return AttributeCollectionDeserializer.FromAsset(r);
                                break;
                            case "<":
                                tokens.MoveNext();
                                ConsumeWhitespace(tokens, state);
                                return (ReadVector(tokens, state));
                                break;
                            default:
                                Unexpected(opTxt, tokens.Current, state);
                                break;
                        }
                        break;
                    }
                case LexerToken.LexerTokenType.Character_Literal:
                    return (ReadChar(tokens, state));
                    break;
                case LexerToken.LexerTokenType.Integer_Literal:
                    return (ReadInteger(tokens, state));
                    break;
                case LexerToken.LexerTokenType.Real_Literal:
                    return (ReadFloat(tokens, state));
                    break;
                case LexerToken.LexerTokenType.Keyword:
                    return ReadBoolean(tokens, state);
                    break;                   
                case LexerToken.LexerTokenType.Identifier:
                case LexerToken.LexerTokenType.String_Literal:
                case LexerToken.LexerTokenType.Unknown:
                case LexerToken.LexerTokenType.FreeForm:
                    return (ReadString(tokens, state));
                    break;
                case LexerToken.LexerTokenType.Directive:
                    tokens = ConsumeDirective(tokens, state);
                    return ConsumeObject(tokens, state);
                    break;

                    break;
            }
            Unexpected("Object", tokens.Current, state);
            return null;

        }
        private static IEnumerator<LexerToken> ConsumeDirective(IEnumerator<LexerToken> tokens, ParserState state)
        {
            //Expecting to be on '#'

            tokens.MoveNext();
            ConsumeWhitespace(tokens, state);
            switch (tokens.Current.Text.ToLower())
            {
                case "begindef":
                case "def":
                case "define":
                    {
                        List<LexerToken> macro = new List<LexerToken>();
                        tokens.MoveNext();
                        ConsumeWhitespace(tokens, state);
                        LexerToken macroIdToken = tokens.Current;
                        //tokens.MoveNext();
                        if (macroIdToken.Type != LexerToken.LexerTokenType.Identifier)
                            Unexpected("Identifier", macroIdToken, state);

                        while (tokens.MoveNext())
                        {
                            if (tokens.Current.Type == LexerToken.LexerTokenType.Directive)
                            {
                                tokens.MoveNext();
                                switch(tokens.Current.Text.ToLower())
                                {
                                    case "enddef":
                                    case "end":
                                        break;
                                    default:
                                        if (String.Compare(tokens.Current.Text, "EndDef", true) != 0)
                                            Unexpected("EndDef", tokens.Current, state);
                                        break;
                                }
                                state.macros.Add(macroIdToken.Text, TrimMacro(macro));
                                tokens.MoveNext();
                                return tokens;
                            }
                            else
                            {
                                macro.Add(tokens.Current);

                            }
                        }
                    }
                    break;
                default:
                    {
                        if (tokens.Current.Type != LexerToken.LexerTokenType.Identifier)
                            Unexpected("BeginDef or Identifier", tokens.Current, state);
                        var macroIdToken = tokens.Current;
                        var macroKey = macroIdToken.Text;
                        //tokens.MoveNext();
                        if (!state.macros.ContainsKey(macroKey))
                            Unexpected("Defined macro", tokens.Current, state);
                        var n = state.Asset;
                        state.Asset = macroKey;
                        List<LexerToken> m = new List<LexerToken>(state.macros[macroKey]);
                        tokens = Join(m.GetEnumerator(), tokens, state, n).GetEnumerator();
                        tokens.MoveNext();
                        return tokens;
                    }

            }
            return tokens;
        }
        private static List<LexerToken> TrimMacro(List<LexerToken> src)
        {
            var last = src.Count - 1;
            var first = 0;
           for(int i=last;i>=0;i--)
            {
                if (src[i].Type != LexerToken.LexerTokenType.Whitespace
                  && src[i].Type != LexerToken.LexerTokenType.Comment)
                {
                    last = i;
                    break;
                }

            }
            if (last <= 0)
                return new List<LexerToken>();
            for(int i=0;i<src.Count;i++)
            {
                if (src[i].Type != LexerToken.LexerTokenType.Whitespace
                  && src[i].Type != LexerToken.LexerTokenType.Comment)
                {
                    first = i;
                    break;
                }
            }

            var ret = new List<LexerToken>();
            for (int i = first; i <= last; i++)
                ret.Add(src[i]);
            return ret;
        }
        private static IEnumerable<LexerToken> Join(IEnumerator<LexerToken> pre, IEnumerator<LexerToken> post, ParserState state, String postSrc)
        {

            while (pre.MoveNext())
                yield return pre.Current;
            state.Asset = postSrc;
            while (post.MoveNext())
                yield return post.Current;
        }

        private static AttributeCollection BuildAttributeCollection(IEnumerator<LexerToken> tokens, ParserState state)
        {
            AttributeCollection ac = new AttributeCollection();
            ConsumeWhitespace(tokens, state);
            if (tokens.Current.Type != LexerToken.LexerTokenType.Operator && tokens.Current.Text != "{")
                Unexpected("{", tokens.Current, state);

            tokens.MoveNext();

            while (tokens.Current.Type != LexerToken.LexerTokenType.Operator &&
                tokens.Current.Text != "}")
            {
                ConsumeWhitespace(tokens, state);
                if (tokens.Current.Type==LexerToken.LexerTokenType.Operator && tokens.Current.Text==">")
                {
                    tokens.MoveNext();
                    ConsumeWhitespace(tokens, state);
                    var r = ReadString(tokens, state);
                    var pre = BareE.Lexer.DefaultLexer.Tokenize(AssetManager.ReadFile(r)).GetEnumerator();
                    var post = state.Asset;
                    state.Asset = r;
                    tokens = Join(pre, tokens, state, post).GetEnumerator();
                    tokens.MoveNext();
                    ConsumeWhitespace(tokens, state);
                }
                if (tokens.Current.Type== LexerToken.LexerTokenType.Directive)
                {
                    ConsumeDirective(tokens, state);
                    continue;
                }
                if (!(tokens.Current.Type == LexerToken.LexerTokenType.String_Literal
                     || tokens.Current.Type == LexerToken.LexerTokenType.Identifier))
                {
                    Unexpected("an Identifier", tokens.Current, state);
                }

                var identifierToken = tokens.Current;
                object value = null;
                tokens.MoveNext();
                ConsumeWhitespace(tokens, state);
                if (tokens.Current.Type != LexerToken.LexerTokenType.Operator)
                    Unexpected(": or @", tokens.Current, state);
                var opText = tokens.Current.Text;
                tokens.MoveNext();
                switch (opText)
                {
                    case ":":
                        value = ConsumeObject(tokens, state);
                        break;
                    case "@":
                        {
                            ConsumeWhitespace(tokens, state);
                            var fk = ReadReference(tokens, state);
                            if (TranslateReference != null) fk = TranslateReference(fk, state);
                            value = AttributeCollectionDeserializer.FromAsset(fk);
                            break;
                        }
                    case "<":
                        {
                            ConsumeWhitespace(tokens, state);
                            var fk = ReadReference(tokens, state);
                            if (TranslateReference != null) fk = TranslateReference(fk, state);
                            value = AttributeCollectionDeserializer.RawFile(fk);
                            break;
                        }
                    default:
                        Unexpected(opText, tokens.Current, state);
                        break;
                }
                ac[identifierToken.Text] = value;
                ConsumeWhitespace(tokens, state);
                if ((tokens.Current.Type == LexerToken.LexerTokenType.Operator && tokens.Current.Text == ","))
                {
                    tokens.MoveNext();
                    ConsumeWhitespace(tokens, state);
                } 
            }
            tokens.MoveNext();
            ConsumeWhitespace(tokens, state);
            return ac;

        }

    }

}
