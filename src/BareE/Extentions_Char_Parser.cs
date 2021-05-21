using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace BareE
{
    public static partial class Extentions
    {
        public static List<String> Operators = new List<string>()
        {
            "@","$","{","}","[","]","(",")",",",";","+","-","*",
            "/","%","&","|","^","!","~","=","<",">","?","++","--",
            "&&","||","==","!=","<=",">=",
        };

        public static bool isNewLineChar(this char c)
        {
            switch (c)
            {
                case '\u000D': //Carriage Return
                case '\u000A': //Line Feed
                case '\u0085': //Next Line
                case '\u2028': //Line Separator
                case '\u2029': //Paragraph Separator
                    return true;

                default:
                    return false;
            }
        }

        public static bool isWhitespaceChar(this char c)
        {
            switch (c)
            {
                case '\u0020': //Space (SP)
                case '\u00A0': //No-break space
                case '\u1680': //Ogham Space Mark
                case '\u2000': //En Quad
                case '\u2001': //Em Quad
                case '\u2002': //En Space
                case '\u2003': //Em Space
                case '\u2004': //Three-per-em Space
                case '\u2005': //Four-per-em Space
                case '\u2006': //Figure Space
                case '\u2008': //Punctuation Space
                case '\u2009': //Thin space
                case '\u200A': //Hair Space
                case '\u202F': //Narrow No-break Space
                case '\u205F': //Medium Mathematical Space
                case '\u3000': //<>IS

                case '\u0009': //Horizontal tab
                case '\u000B': //Vertical tab
                case '\u000C': //Form Feed Character
                    return true;

                default:
                    return false;
            }
        }

        public static bool isIdentifierStartChar(this char c)
        {
            if (isLetter(c))
                return true;
            if (c == '_') return true;
            return false;
        }

        public static bool isIdentifierChar(this char c)
        {
            if (isLetter(c)) return true;
            if (isDecimalDigit(c)) return true;
            if (isConnector(c)) return true;
            if (isCombiningCharacter(c)) return true;
            if (isFormattingCharacter(c)) return true;
            //if (c == '.') return true;
            //if (c == '[' || c == ']') return true;
            return false;
        }

        public static bool isLetter(this char c)
        {
            UnicodeCategory uCat = CharUnicodeInfo.GetUnicodeCategory(c);
            switch (uCat)
            {
                case UnicodeCategory.UppercaseLetter:
                case UnicodeCategory.LowercaseLetter:
                case UnicodeCategory.TitlecaseLetter:
                case UnicodeCategory.ModifierLetter:
                case UnicodeCategory.OtherLetter:
                case UnicodeCategory.LetterNumber:
                    return true;
            }
            return false;
        }

        public static bool isBinaryDigit(this char c)
        {
            switch (c)
            {
                case '0':
                case '1':
                    return true;
            }
            return false;
        }

        public static bool isDecimalDigit(this char c)
        {
            switch (c)
            {
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    return true;
            }
            return false;
        }

        public static bool isHexDigit(this char c)
        {
            switch (c)
            {
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                case 'a':
                case 'A':
                case 'b':
                case 'B':
                case 'c':
                case 'C':
                case 'd':
                case 'D':
                case 'e':
                case 'E':
                case 'f':
                case 'F':
                    return true;
            }
            return false;
        }

        public static bool isConnector(this Char c)
        {
            switch (c)
            {
                case '\u005F': //Low Line
                case '\u203F': //UnderTie
                case '\u2040': //CharacterTie
                case '\u2054': //Inverted UnderTie
                case '\uFE33': //PRESENTATION FORM FOR VERTICAL LOW LINE
                case '\uFE34': //PRESENTATION FORM FOR VERTICAL WAVY LOW LINE
                case '\uFE4D': //DASHED LOW LINE
                case '\uFE4E': //CENTRELINE LOW LINE
                case '\uFE4F': //WAVY LOW LINE
                case '\uFF3F': //FULLWIDTH LOW LINE
                    return true;
            }
            return false;
        }

        public static bool isCombiningCharacter(this char c)
        {
            UnicodeCategory uCat = CharUnicodeInfo.GetUnicodeCategory(c);
            if (uCat == UnicodeCategory.NonSpacingMark) return true;
            if (uCat == UnicodeCategory.SpacingCombiningMark) return true;
            return false;
        }

        public static bool isFormattingCharacter(this Char c)
        {
            UnicodeCategory uCat = CharUnicodeInfo.GetUnicodeCategory(c);
            if (uCat == UnicodeCategory.Format) return true;
            return false;
        }

        public static bool isPartialOperatorOrPunctuator(this String s)
        {
            return Operators.FirstOrDefault(x => x.StartsWith(s)) != null;
        }

        public static bool isOperatorOrPunctuator(this String s)
        {
            return Operators.Contains(s);
        }
    }
}