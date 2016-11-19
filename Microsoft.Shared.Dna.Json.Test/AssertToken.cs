//-----------------------------------------------------------------------------
// <copyright file="AssertToken.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
//     Licensed under the MIT license. See license file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------------

namespace Microsoft.Shared.Dna.Json.Test
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Parser token validation.
    /// </summary>
    internal static class AssertToken
    {
        /// <summary>
        /// Validates that the parser is complete.
        /// </summary>
        /// <param name="expectedPayload">The expected payload.</param>
        /// <param name="actualParser">The actual parser.</param>
        public static void IsComplete(string expectedPayload, JsonParser actualParser)
        {
            Assert.IsFalse(actualParser.Next());
            AssertToken.Matches(JsonTokenType.Complete, expectedPayload, 0, expectedPayload.Length, actualParser);
            Assert.IsFalse(actualParser.Next());
            AssertToken.Matches(JsonTokenType.Complete, expectedPayload, 0, expectedPayload.Length, actualParser);
        }

        /// <summary>
        /// Validates that the parser is on a property.
        /// </summary>
        /// <param name="expectedProperty">The expected property.</param>
        /// <param name="expectedPayload">The expected payload.</param>
        /// <param name="expectedOffset">The expected offset.</param>
        /// <param name="expectedCount">The expected count.</param>
        /// <param name="actualParser">The actual parser.</param>
        public static void IsProperty(string expectedProperty, string expectedPayload, int expectedOffset, int expectedCount, JsonParser actualParser)
        {
            AssertToken.Matches(JsonTokenType.BeginProperty, expectedPayload, expectedOffset, expectedCount, actualParser);
            string actualProperty = null;
            Assert.IsTrue(actualParser.TryParseToken(out actualProperty));
            Assert.AreEqual(expectedProperty, actualProperty);
        }

        /// <summary>
        /// Validates that the parser is on a value.
        /// </summary>
        /// <param name="expectedValue">The expected value.</param>
        /// <param name="expectedPayload">The expected payload.</param>
        /// <param name="expectedOffset">The expected offset.</param>
        /// <param name="expectedCount">The expected count.</param>
        /// <param name="actualParser">The actual parser.</param>
        public static void IsValue(bool expectedValue, string expectedPayload, int expectedOffset, int expectedCount, JsonParser actualParser)
        {
            AssertToken.Matches(JsonTokenType.Boolean, expectedPayload, expectedOffset, expectedCount, actualParser);
            bool actualValue = false;
            Assert.IsTrue(actualParser.TryParseToken(out actualValue));
            Assert.AreEqual(expectedValue, actualValue);
        }

        /// <summary>
        /// Validates that the parser is on a value.
        /// </summary>
        /// <param name="expectedValue">The expected value.</param>
        /// <param name="expectedPayload">The expected payload.</param>
        /// <param name="expectedOffset">The expected offset.</param>
        /// <param name="expectedCount">The expected count.</param>
        /// <param name="actualParser">The actual parser.</param>
        public static void IsValue(long expectedValue, string expectedPayload, int expectedOffset, int expectedCount, JsonParser actualParser)
        {
            AssertToken.Matches(JsonTokenType.Integer, expectedPayload, expectedOffset, expectedCount, actualParser);
            long actualValue = 0L;
            Assert.IsTrue(actualParser.TryParseToken(out actualValue));
            Assert.AreEqual(expectedValue, actualValue);
        }

        /// <summary>
        /// Validates that the parser is on a value.
        /// </summary>
        /// <param name="expectedValue">The expected value.</param>
        /// <param name="expectedPayload">The expected payload.</param>
        /// <param name="expectedOffset">The expected offset.</param>
        /// <param name="expectedCount">The expected count.</param>
        /// <param name="actualParser">The actual parser.</param>
        public static void IsValue(ulong expectedValue, string expectedPayload, int expectedOffset, int expectedCount, JsonParser actualParser)
        {
            AssertToken.Matches(JsonTokenType.Integer, expectedPayload, expectedOffset, expectedCount, actualParser);
            ulong actualValue = 0UL;
            Assert.IsTrue(actualParser.TryParseToken(out actualValue));
            Assert.AreEqual(expectedValue, actualValue);
        }

        /// <summary>
        /// Validates that the parser is on a value.
        /// </summary>
        /// <param name="expectedValue">The expected value.</param>
        /// <param name="expectedPayload">The expected payload.</param>
        /// <param name="expectedOffset">The expected offset.</param>
        /// <param name="expectedCount">The expected count.</param>
        /// <param name="actualParser">The actual parser.</param>
        public static void IsValue(double expectedValue, string expectedPayload, int expectedOffset, int expectedCount, JsonParser actualParser)
        {
            AssertToken.Matches(JsonTokenType.Float, expectedPayload, expectedOffset, expectedCount, actualParser);
            double actualValue = 0D;
            Assert.IsTrue(actualParser.TryParseToken(out actualValue));
            Assert.AreEqual(expectedValue, actualValue);
        }

        /// <summary>
        /// Validates that the parser is on a value.
        /// </summary>
        /// <param name="expectedValue">The expected value.</param>
        /// <param name="expectedPayload">The expected payload.</param>
        /// <param name="expectedOffset">The expected offset.</param>
        /// <param name="expectedCount">The expected count.</param>
        /// <param name="actualParser">The actual parser.</param>
        public static void IsValue(string expectedValue, string expectedPayload, int expectedOffset, int expectedCount, JsonParser actualParser)
        {
            AssertToken.Matches(JsonTokenType.String, expectedPayload, expectedOffset, expectedCount, actualParser);
            string actualValue = null;
            Assert.IsTrue(actualParser.TryParseToken(out actualValue));
            Assert.AreEqual(expectedValue.Length, actualValue.Length);
            for (int i = 0; i < expectedValue.Length; i++)
            {
                Assert.AreEqual(expectedValue[i], actualValue[i], "index:{0}", i);
            }
        }

        /// <summary>
        /// Validates that the parser is on a given token.
        /// </summary>
        /// <param name="expectedType">The expected token type.</param>
        /// <param name="expectedPayload">The expected payload.</param>
        /// <param name="expectedOffset">The expected offset.</param>
        /// <param name="expectedCount">The expected count.</param>
        /// <param name="actualParser">The actual parser.</param>
        public static void Matches(JsonTokenType expectedType, string expectedPayload, int expectedOffset, int expectedCount, JsonParser actualParser)
        {
            Assert.AreEqual(expectedType, actualParser.TokenType);
            Assert.AreSame(expectedPayload, actualParser.TokenSegment.String);
            Assert.AreEqual(expectedOffset, actualParser.TokenSegment.Offset);
            Assert.AreEqual(expectedCount, actualParser.TokenSegment.Count);
        }
    }
}
