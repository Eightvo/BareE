using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using static BareE.LexerToken;

namespace BareE
{
    public class Lexer
    {
        public bool KeywordsCaseInsensitive { get; set; }
        public static Lexer DefaultLexer = new Lexer();

        public IEnumerable<LexerToken> Tokenize(String query)
        {
            return Tokenize(query, Encoding.ASCII);
        }

        public IEnumerable<LexerToken> Tokenize(String query, Encoding encoding)
        {
            ActiveEncoding = encoding;
            LineNumber = 0;
            Character = 0;
            using (StreamReader rdr = new StreamReader(new MemoryStream(encoding.GetBytes(query))))
            {
                foreach (var ret in ConsumeInput(rdr))
                    yield return ret;
            }
        }

        public HashSet<String> Keywords = new HashSet<string>()
        {
            "abstract", "as",       "base",       "bool",      "break",
    "byte",     "case",     "catch",      "char",      "checked",
    "class",    "const",    "continue",   "decimal",   "default",
    "delegate", "do",       "double",     "else",      "enum",
    "event",    "explicit", "extern",     "false",     "finally",
    "fixed",    "float",    "for",        "foreach",   "goto",
    "if",       "implicit", "in",         "int",       "interface",
    "internal", "is",       "lock",       "long",      "namespace",
    "new",      "null",     "object",     "operator",  "out",
    "override", "params",   "private",    "protected", "public",
    "readonly", "ref",      "return",     "sbyte",     "sealed",
    "short",    "sizeof",   "stackalloc", "static",    "string",
    "struct",   "switch",   "this",       "throw",     "true",
    "try",      "typeof",   "uint",       "ulong",     "unchecked",
    "unsafe",   "ushort",   "using",      "virtual",   "void",
    "volatile", "while"
        };

        public HashSet<String> Operators = new HashSet<string>() {
    "{",  "}",  "[",  "]",  "(",   ")",  ".",  ",",  ":",  ";",
    "+",  "-",  "*",  "/",  "%",   "&",  "|",  "^",  "!",  "~",
    "=",  "<",  ">",  "?",  "??",  "::", "++", "--", "&&", "||",
    "->", "==", "!=", "<=", ">=",  "+=", "-=", "*=", "/=", "%=",
    "&=", "|=", "^=", "<<", "<<=", "=>",
    ">>", ">>=", "@" };

        public bool OperatorMatch(String partialOperator)
        {
            foreach (String s in Operators)
            {
                if (s.StartsWith(partialOperator)) return true;
            }
            return false;
        }

        private Encoding ActiveEncoding;
        private int LineNumber;
        private int Character;

        private char MoveNext(StreamReader rdr)
        {
            Character += 1;
            var nxt = (char)rdr.Read();

            if (nxt == '\u000D' && !rdr.EndOfStream && (char)rdr.Peek() == '\u000A')
                rdr.Read();
            if (nxt.isNewLineChar())
            {
                Character = 0;
                LineNumber++;
            }
            if (nxt == '\uFFFF')
                nxt = '\r';
            //Console.Write($"Read:{nxt}");
            return nxt;
        }

        private LexerToken CreateToken(LexerToken.LexerTokenType type, String text)
        {
            return new LexerToken()
            {
                LineNumer = this.LineNumber,
                Character = this.Character - text.Length,
                Text = text,
                Type = type
            };
        }

        private IEnumerable<LexerToken> ConsumeInput(StreamReader rdr)
        {
            while (!rdr.EndOfStream)
            {
                var curr = (char)rdr.Peek();
                while (!(rdr.EndOfStream))
                {
                    foreach (var v in ConsumeWhitespace(rdr))
                        yield return v;

                    //yield return ConsumeWhitespace(rdr);
                    //if (curr.isNewLineChar())
                    foreach (var v in ConsumeInputSection(rdr))
                        yield return v;

                    curr = (char)rdr.Peek();
                }
            }
            yield break;
        }

        private IEnumerable<LexerToken> ConsumeInputSection(StreamReader rdr)
        {
            char curr = (char)rdr.Peek();
            foreach (var v in ConsumeWhitespace(rdr))
                yield return v;
            if (curr == '/')
            {
                MoveNext(rdr);
                curr = (char)rdr.Peek();
                if (curr == '*' || curr == '/')
                {
                    yield return ConsumeComment(rdr);
                }
            }
            if (curr == '#')
            {
                //PreProcessor Directive
            }
            yield return ConsumeToken(rdr, curr);
        }

        #region Whitespace

        private IEnumerable<LexerToken> ConsumeWhitespace(StreamReader rdr)
        {
            StringBuilder whitespace = new StringBuilder();
            while (((char)rdr.Peek()).isWhitespaceChar() || ((char)rdr.Peek()).isNewLineChar())
            {
                if (((char)rdr.Peek()).isNewLineChar())
                {
                    if (whitespace.Length > 0)
                        yield return CreateToken(LexerTokenType.Whitespace, whitespace.ToString());
                    yield return CreateToken(LexerTokenType.Whitespace, ((char)rdr.Peek()).ToString());
                    whitespace.Clear();
                    MoveNext(rdr);
                }
                else if ((char)rdr.Peek() == '\t')
                {
                    whitespace.Append("    ");
                    MoveNext(rdr);
                }
                else
                {
                    whitespace.Append(MoveNext(rdr));
                }
                if (rdr.EndOfStream)
                    break;
            }
            if (whitespace.Length > 0)
                yield return CreateToken(LexerTokenType.Whitespace, whitespace.ToString());
        }

        private LexerToken ConsumeComment(StreamReader rdr)
        {
            switch ((char)rdr.Peek())
            {
                case '/':
                    MoveNext(rdr);
                    return ConsumeSingleLineComment(rdr);

                case '*':
                default:
                    MoveNext(rdr);
                    return ConsumeMultiLineComment(rdr);
            }
        }

        private LexerToken ConsumeSingleLineComment(StreamReader rdr)
        {
            StringBuilder comment = new StringBuilder();
            while (!((char)rdr.Peek()).isNewLineChar())
            {
                comment.Append(MoveNext(rdr));
                if (rdr.EndOfStream)
                    break;
            }
            return CreateToken(LexerTokenType.Comment, comment.ToString());
        }

        private LexerToken ConsumeMultiLineComment(StreamReader rdr)
        {
            StringBuilder comment = new StringBuilder();
            bool done = false;
            while (!done)
            {
                var curr = (char)rdr.Peek();
                if (curr == '*')
                {
                    var pC = MoveNext(rdr);
                    curr = (char)rdr.Peek();
                    if (curr == '/')
                    {
                        MoveNext(rdr);
                        break;
                    }
                    else
                        comment.Append(pC);
                }
                comment.Append(curr);
                curr = MoveNext(rdr);
                if (rdr.EndOfStream)
                    done = true;
            }

            return CreateToken(LexerTokenType.Comment, comment.ToString());
        }

        #endregion Whitespace

        #region Preprocess

        private LexerToken ConsumeDirective(StreamReader rdr, char curr)
        {
            var DirectiveToken = ConsumeSingleLineComment(rdr);
            DirectiveToken.Type = LexerTokenType.Directive;
            return DirectiveToken;
        }

        #endregion Preprocess

        #region Tokens

        private LexerToken ConsumeToken(StreamReader rdr, char curr)
        {
            if (curr.isIdentifierStartChar())
                return ConsumeIdentifierOrKeyword(rdr);
            if (OperatorMatch($"{curr}"))
                if (!((curr == '-') && !((char)rdr.Peek()).isDecimalDigit()))
                    return ConsumeOperator(rdr);
            if (curr == '\'')
                return ConsumeCharacterLiteral(rdr);
            if (curr == '"')
                return ConsumeStringLiteral(rdr);
            if (curr.isDecimalDigit() || curr == '-')
                return ConsumeNumericLiteral(rdr);
            return ConsumeUnknownLiteral(rdr);
            //throw new Exception("Unexpected.");
        }

        private LexerToken ConsumeNumericLiteral(StreamReader rdr)
        {
            StringBuilder text = new StringBuilder();
            LexerTokenType t = LexerTokenType.Integer_Literal;
            var curr = MoveNext(rdr);
            if (curr == '-')
            {
                text.Append(curr);
                curr = MoveNext(rdr);
            }
            if (curr == ('0'))
            {
                switch ((char)rdr.Peek())
                {
                    case 'x':
                        return ConsumeHexNumericLiteral(rdr, text.ToString());

                    case 'b':
                        return ConsumeBinaryLiteral(rdr, text.ToString());
                }
            }
            while (curr.isDecimalDigit() || curr == '.')
            {
                text.Append(curr);
                var tst = (char)rdr.Peek();
                if (!(tst.isDecimalDigit() || tst == '.'))
                    break;
                curr = MoveNext(rdr);
                if (curr == '.')
                {
                    if (t == LexerTokenType.Real_Literal)
                        throw new Exception("Invalid Numeric format.");
                    else
                        t = LexerTokenType.Real_Literal;
                }
            }
            return CreateToken(t, text.ToString());
        }

        private LexerToken ConsumeBinaryLiteral(StreamReader rdr, string prefix)
        {
            MoveNext(rdr);
            StringBuilder text = new StringBuilder();
            var curr = MoveNext(rdr);
            while (curr.isBinaryDigit())
            {
                text.Append(curr);
                if (!(((char)rdr.Peek()).isBinaryDigit()))
                    break;
                curr = MoveNext(rdr);
            }
            return CreateToken(LexerTokenType.Integer_Literal, $"{prefix}{Convert.ToInt64(text.ToString(), 2)}");
        }

        private LexerToken ConsumeHexNumericLiteral(StreamReader rdr, String prefix)
        {
            MoveNext(rdr);
            StringBuilder text = new StringBuilder();
            var curr = MoveNext(rdr);
            while (curr.isHexDigit())
            {
                text.Append(curr);
                if (!(((char)rdr.Peek()).isHexDigit()))
                    break;
                curr = MoveNext(rdr);
            }
            return CreateToken(LexerTokenType.Integer_Literal, $"{prefix}{Convert.ToInt64(text.ToString(), 16)}");
        }

        private String ConsumeEscapeSequence(StreamReader rdr)
        {
            var escSeq = MoveNext(rdr);
            var ret = String.Empty;
            switch (escSeq)
            {
                case 'n': ret = "\n"; break;
                case 'r': ret = "\r"; break;
                case '\\': ret = "\\"; break;
                case '"': ret = "\""; break;
                case '\'': ret = "'"; break;
                default: throw new Exception("Unrecognized Escape Sequence");
            }
            MoveNext(rdr);
            return ret;
        }

        private LexerToken ConsumeStringLiteral(StreamReader rdr)
        {
            StringBuilder result = new StringBuilder();
            var curr = MoveNext(rdr);
            curr = MoveNext(rdr);
            while (curr != '"')
            {
                if (curr == '\\')
                {
                    result.Append(ConsumeEscapeSequence(rdr));
                }
                else
                    result.Append(curr);
                curr = MoveNext(rdr);
            }
            var text = result.ToString();
            LexerTokenType tokenType = LexerTokenType.Identifier;
            if (Keywords.Contains(text))
                tokenType = LexerTokenType.Keyword;
            return CreateToken(tokenType, text);
        }

        private LexerToken ConsumeCharacterLiteral(StreamReader rdr)
        {
            var nxt = MoveNext(rdr);
            nxt = MoveNext(rdr);
            String txt = String.Empty;
            if (nxt == '\\')
                txt = ConsumeEscapeSequence(rdr);
            else
                txt = nxt.ToString();
            MoveNext(rdr);
            return CreateToken(LexerTokenType.Character_Literal, txt);
        }

        private LexerToken ConsumeUnknownLiteral(StreamReader rdr)
        {
            var nxt = MoveNext(rdr);
            String txt = nxt.ToString();
            return CreateToken(LexerTokenType.Unknown, txt);
        }

        private LexerToken ConsumeOperator(StreamReader rdr)
        {
            StringBuilder text = new StringBuilder();

            while (OperatorMatch($"{text}{(char)rdr.Peek()}"))
            {
                text.Append(MoveNext(rdr));
            }
            return CreateToken(LexerTokenType.Operator, $"{text}");
        }

        public bool IsKeyword(String text)
        {
            if (!KeywordsCaseInsensitive)
                return Keywords.Contains(text);
            foreach (String s in Keywords)
            {
                if (String.Compare(s, text, true) == 0)
                    return true;
            }
            return false;
        }

        private LexerToken ConsumeIdentifierOrKeyword(StreamReader rdr)
        {
            StringBuilder result = new StringBuilder();
            var curr = MoveNext(rdr);
            while (curr.isIdentifierChar())
            {
                result.Append(curr);
                if (!(((char)rdr.Peek()).isIdentifierChar()))
                    break;
                curr = MoveNext(rdr);
            }
            var text = result.ToString();
            LexerTokenType tokenType = LexerTokenType.Identifier;

            if (IsKeyword(text))
                tokenType = LexerTokenType.Keyword;
            return CreateToken(tokenType, text);
        }

        public void SetKeywords(IEnumerable<string> list)
        {
            Keywords.Clear();
            foreach (var keyword in list)
                Keywords.Add(keyword);
        }

        #endregion Tokens
    }
}