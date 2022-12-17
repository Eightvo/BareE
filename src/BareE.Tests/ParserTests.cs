using Microsoft.VisualStudio.TestTools.UnitTesting;

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
    }
}
