using System;
using System.Diagnostics;

namespace BareE
{
    [DebuggerDisplay("[{LineNumer}, {Character}] {Type}: {Text}")]
    public class LexerToken
    {
        public LexerTokenType Type;
        public int LineNumer;
        public int Character;
        public String Text;

        public enum LexerTokenType
        {
            Whitespace = 0,
            Identifier = 1,
            Keyword = 2,
            Integer_Literal = 4,
            Real_Literal = 8,
            Character_Literal = 16,
            String_Literal = 32,
            Interpolated_String_Literal = 64,
            Operator = 128,
            Comment = 256,
            Directive = 512,
            FreeForm = 1024,
            Unknown = 2048,
        }
    }
}