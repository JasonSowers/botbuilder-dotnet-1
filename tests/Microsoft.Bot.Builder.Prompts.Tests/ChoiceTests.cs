// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Bot.Builder.Prompts.Choices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Prompts.Tests
{
    [TestClass]
    [TestCategory("Prompts")]
    [TestCategory("Choice Tests")]
    public class ChoiceTests
    {
        [TestMethod]
        public void TestBreakOnEmojis()
        {
            var tokens = Tokenizer.DefaultTokenizer("food 💥👍😀");
            Assert.AreEqual(tokens.Count, 4);
            AssertToken(tokens[0], 0, 3, "food");
            AssertToken(tokens[1], 5, 6, "💥");
            AssertToken(tokens[2], 7, 8, "👍");
            AssertToken(tokens[3], 9, 10, "😀");
        }

        private static void AssertToken(Token token, int start, int end, string text, string normalized = null)
        {
            Assert.IsTrue(token.Start == start, "Invalid token.start of '${token.start}' for '${text}' token.");
            Assert.IsTrue(token.End == end, "Invalid token.end of '${token.end}' for '${text}' token.");
            Assert.IsTrue(token.Text == text, "Invalid token.text of '${token.text}' for '${text}' token.");
            Assert.IsTrue(token.Normalized == (normalized ?? text), "Invalid token.normalized of '${token.normalized}' for '${text}' token.");
        }
    }
}
