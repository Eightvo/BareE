using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json.Serialization;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BareE.Tests
{
    [TestClass]
    public class ParserTests
    {
        [DataTestMethod]
        [DataRow(" ")]
        [DataRow("   ")]
        [DataRow("  ")]
        public void GetWhitespace_FromAnyWhitespaceSrc(String src)
        {
            var tokens = BareE.Lexer.DefaultLexer.Tokenize(src).ToList();
            Assert.IsNotNull(tokens);
            Assert.AreEqual(1, tokens.Count());
            Assert.IsTrue(tokens.ElementAt(0).Type == LexerToken.LexerTokenType.Whitespace);
        }


        [DataTestMethod]
        [DataRow(-10, 0, 100, 90)]
        [DataRow(  0, 0, 100,  0)]
        [DataRow( 10, 0, 100, 10)]
        [DataRow(110, 0, 100, 10)]
        public void TestWrap(int value, int min, int max, int expected)
        {
            Assert.AreEqual(MathHelper.Wrap(value, min, max), expected);
        }

    }



}
