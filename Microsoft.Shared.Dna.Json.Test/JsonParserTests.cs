//-----------------------------------------------------------------------------
// <copyright file="JsonParserTests.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
//     Licensed under the MIT license. See license file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------------

namespace Microsoft.Shared.Dna.Json.Test
{
    using System;
    using System.Globalization;
    using System.Text;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests for the <see cref="JsonParser"/> class.
    /// </summary>
    [TestClass]
    public class JsonParserTests
    {
        /// <summary>
        /// Constructor allows null payload.
        /// </summary>
        [TestMethod]
        public void JsonParser_Constructor_Allows_Null_Payload()
        {
            string payload = null;
            JsonParser target = new JsonParser(payload);
            Assert.IsFalse(target.Next());
            Assert.AreEqual(JsonTokenType.Invalid, target.TokenType);
        }

        /// <summary>
        /// Constructor rejects non-positive depth.
        /// </summary>
        [TestMethod]
        public void JsonParser_Constructor_Rejects_Non_Positive_Depth()
        {
            try
            {
                JsonParser target = new JsonParser("null", 4, 0);
                Assert.IsNull(target);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Assert.AreEqual(typeof(ArgumentOutOfRangeException), ex.GetType());
            }
        }

        /// <summary>
        /// Next parses empty array.
        /// </summary>
        [TestMethod]
        public void JsonParser_Next_Parses_Empty_Array()
        {
            string payload = "[]";
            JsonParser target = new JsonParser(payload);
            Assert.IsTrue(target.Next());
            AssertToken.Matches(JsonTokenType.BeginArray, payload, 0, 1, target);
            Assert.IsTrue(target.Next());
            AssertToken.Matches(JsonTokenType.EndArray, payload, 0, 2, target);
            AssertToken.IsComplete(payload, target);
        }

        /// <summary>
        /// Next parses array nested in array.
        /// </summary>
        [TestMethod]
        public void JsonParser_Next_Parses_Array_Nested_In_Array()
        {
            string payload = "[[1,2]]";
            JsonParser target = new JsonParser(payload);
            Assert.IsTrue(target.Next());
            AssertToken.Matches(JsonTokenType.BeginArray, payload, 0, 1, target);
            Assert.IsTrue(target.Next());
            AssertToken.Matches(JsonTokenType.BeginArray, payload, 1, 1, target);
            Assert.IsTrue(target.Next());
            AssertToken.IsValue(1L, payload, 2, 1, target);
            Assert.IsTrue(target.Next());
            AssertToken.IsValue(2L, payload, 4, 1, target);
            Assert.IsTrue(target.Next());
            AssertToken.Matches(JsonTokenType.EndArray, payload, 1, 5, target);
            Assert.IsTrue(target.Next());
            AssertToken.Matches(JsonTokenType.EndArray, payload, 0, 7, target);
            AssertToken.IsComplete(payload, target);
        }

        /// <summary>
        /// Next parses array nested in property.
        /// </summary>
        [TestMethod]
        public void JsonParser_Next_Parses_Array_Nested_In_Property()
        {
            string payload = "{\"array\":[1,2]}";
            JsonParser target = new JsonParser(payload);
            Assert.IsTrue(target.Next());
            AssertToken.Matches(JsonTokenType.BeginObject, payload, 0, 1, target);
            Assert.IsTrue(target.Next());
            AssertToken.IsProperty("array", payload, 1, 8, target);
            Assert.IsTrue(target.Next());
            AssertToken.Matches(JsonTokenType.BeginArray, payload, 9, 1, target);
            Assert.IsTrue(target.Next());
            AssertToken.IsValue(1L, payload, 10, 1, target);
            Assert.IsTrue(target.Next());
            AssertToken.IsValue(2L, payload, 12, 1, target);
            Assert.IsTrue(target.Next());
            AssertToken.Matches(JsonTokenType.EndArray, payload, 9, 5, target);
            Assert.IsTrue(target.Next());
            AssertToken.Matches(JsonTokenType.EndProperty, payload, 1, 13, target);
            Assert.IsTrue(target.Next());
            AssertToken.Matches(JsonTokenType.EndObject, payload, 0, 15, target);
            AssertToken.IsComplete(payload, target);
        }

        /// <summary>
        /// Next parses empty object.
        /// </summary>
        [TestMethod]
        public void JsonParser_Next_Parses_Empty_Object()
        {
            string payload = "{}";
            JsonParser target = new JsonParser(payload);
            Assert.IsTrue(target.Next());
            AssertToken.Matches(JsonTokenType.BeginObject, payload, 0, 1, target);
            Assert.IsTrue(target.Next());
            AssertToken.Matches(JsonTokenType.EndObject, payload, 0, 2, target);
            AssertToken.IsComplete(payload, target);
        }

        /// <summary>
        /// Next parses object nested in array.
        /// </summary>
        [TestMethod]
        public void JsonParser_Next_Parses_Object_Nested_In_Array()
        {
            string payload = "[{\"value\":1},{\"value\":2}]";
            JsonParser target = new JsonParser(payload);
            Assert.IsTrue(target.Next());
            AssertToken.Matches(JsonTokenType.BeginArray, payload, 0, 1, target);
            Assert.IsTrue(target.Next());
            AssertToken.Matches(JsonTokenType.BeginObject, payload, 1, 1, target);
            Assert.IsTrue(target.Next());
            AssertToken.IsProperty("value", payload, 2, 8, target);
            Assert.IsTrue(target.Next());
            AssertToken.IsValue(1L, payload, 10, 1, target);
            Assert.IsTrue(target.Next());
            AssertToken.Matches(JsonTokenType.EndProperty, payload, 2, 9, target);
            Assert.IsTrue(target.Next());
            AssertToken.Matches(JsonTokenType.EndObject, payload, 1, 11, target);
            Assert.IsTrue(target.Next());
            AssertToken.Matches(JsonTokenType.BeginObject, payload, 13, 1, target);
            Assert.IsTrue(target.Next());
            AssertToken.IsProperty("value", payload, 14, 8, target);
            Assert.IsTrue(target.Next());
            AssertToken.IsValue(2L, payload, 22, 1, target);
            Assert.IsTrue(target.Next());
            AssertToken.Matches(JsonTokenType.EndProperty, payload, 14, 9, target);
            Assert.IsTrue(target.Next());
            AssertToken.Matches(JsonTokenType.EndObject, payload, 13, 11, target);
            Assert.IsTrue(target.Next());
            AssertToken.Matches(JsonTokenType.EndArray, payload, 0, 25, target);
            AssertToken.IsComplete(payload, target);
        }

        /// <summary>
        /// Next parses object nested in property.
        /// </summary>
        [TestMethod]
        public void JsonParser_Next_Parses_Object_Nested_In_Property()
        {
            string payload = "{\"first\":{\"value\":1},\"second\":{\"value\":2}}";
            JsonParser target = new JsonParser(payload);
            Assert.IsTrue(target.Next());
            AssertToken.Matches(JsonTokenType.BeginObject, payload, 0, 1, target);
            Assert.IsTrue(target.Next());
            AssertToken.IsProperty("first", payload, 1, 8, target);
            Assert.IsTrue(target.Next());
            AssertToken.Matches(JsonTokenType.BeginObject, payload, 9, 1, target);
            Assert.IsTrue(target.Next());
            AssertToken.IsProperty("value", payload, 10, 8, target);
            Assert.IsTrue(target.Next());
            AssertToken.IsValue(1L, payload, 18, 1, target);
            Assert.IsTrue(target.Next());
            AssertToken.Matches(JsonTokenType.EndProperty, payload, 10, 9, target);
            Assert.IsTrue(target.Next());
            AssertToken.Matches(JsonTokenType.EndObject, payload, 9, 11, target);
            Assert.IsTrue(target.Next());
            AssertToken.Matches(JsonTokenType.EndProperty, payload, 1, 19, target);
            Assert.IsTrue(target.Next());
            AssertToken.IsProperty("second", payload, 21, 9, target);
            Assert.IsTrue(target.Next());
            AssertToken.Matches(JsonTokenType.BeginObject, payload, 30, 1, target);
            Assert.IsTrue(target.Next());
            AssertToken.IsProperty("value", payload, 31, 8, target);
            Assert.IsTrue(target.Next());
            AssertToken.IsValue(2L, payload, 39, 1, target);
            Assert.IsTrue(target.Next());
            AssertToken.Matches(JsonTokenType.EndProperty, payload, 31, 9, target);
            Assert.IsTrue(target.Next());
            AssertToken.Matches(JsonTokenType.EndObject, payload, 30, 11, target);
            Assert.IsTrue(target.Next());
            AssertToken.Matches(JsonTokenType.EndProperty, payload, 21, 20, target);
            Assert.IsTrue(target.Next());
            AssertToken.Matches(JsonTokenType.EndObject, payload, 0, 42, target);
            AssertToken.IsComplete(payload, target);
        }

        /// <summary>
        /// Next halts on empty element.
        /// </summary>
        [TestMethod]
        public void JsonParser_Next_Halts_On_Empty_Element()
        {
            string payload = "{\"array\":[0z0]}";
            JsonParser target = new JsonParser(payload);
            Assert.IsTrue(target.Next());
            AssertToken.Matches(JsonTokenType.BeginObject, payload, 0, 1, target);
            Assert.IsTrue(target.Next());
            AssertToken.IsProperty("array", payload, 1, 8, target);
            Assert.IsTrue(target.Next());
            AssertToken.Matches(JsonTokenType.BeginArray, payload, 9, 1, target);
            Assert.IsTrue(target.Next());
            AssertToken.IsValue(0L, payload, 10, 1, target);
            Assert.IsFalse(target.Next());
            AssertToken.Matches(JsonTokenType.Invalid, payload, 11, 0, target);
        }

        /// <summary>
        /// Skip moves over containers.
        /// </summary>
        [TestMethod]
        public void JsonParser_Skip_Moves_Over_Containers()
        {
            string payload = "{\"first\":{\"value\":1},\"second\":[1,2],\"third\":123.45}";
            JsonParser target = new JsonParser(payload);
            Assert.IsTrue(target.Next());
            AssertToken.Matches(JsonTokenType.BeginObject, payload, 0, 1, target);
            Assert.IsTrue(target.Next());
            AssertToken.IsProperty("first", payload, 1, 8, target);
            Assert.IsTrue(target.Next());
            AssertToken.Matches(JsonTokenType.BeginObject, payload, 9, 1, target);
            Assert.IsTrue(target.Skip());
            AssertToken.Matches(JsonTokenType.EndObject, payload, 9, 11, target);
            Assert.IsTrue(target.Next());
            AssertToken.Matches(JsonTokenType.EndProperty, payload, 1, 19, target);
            Assert.IsTrue(target.Next());
            AssertToken.IsProperty("second", payload, 21, 9, target);
            Assert.IsTrue(target.Next());
            AssertToken.Matches(JsonTokenType.BeginArray, payload, 30, 1, target);
            Assert.IsTrue(target.Next());
            AssertToken.IsValue(1, payload, 31, 1, target);
            Assert.IsTrue(target.Skip());
            AssertToken.Matches(JsonTokenType.EndArray, payload, 30, 5, target);
            Assert.IsTrue(target.Next());
            AssertToken.Matches(JsonTokenType.EndProperty, payload, 21, 14, target);
            Assert.IsTrue(target.Next());
            AssertToken.IsProperty("third", payload, 36, 8, target);
            Assert.IsTrue(target.Skip());
            AssertToken.Matches(JsonTokenType.EndProperty, payload, 36, 14, target);
            Assert.IsTrue(target.Next());
            AssertToken.Matches(JsonTokenType.EndObject, payload, 0, 51, target);
            AssertToken.IsComplete(payload, target);
        }

        /// <summary>
        /// Next parses null property.
        /// </summary>
        [TestMethod]
        public void JsonParser_Next_Parses_Null_Property()
        {
            string payload = "{\"isNull\":null}";
            JsonParser target = new JsonParser(payload);
            Assert.IsTrue(target.Next());
            AssertToken.Matches(JsonTokenType.BeginObject, payload, 0, 1, target);
            Assert.IsTrue(target.Next());
            AssertToken.IsProperty("isNull", payload, 1, 9, target);
            Assert.IsTrue(target.Next());
            AssertToken.Matches(JsonTokenType.Null, payload, 10, 4, target);
            Assert.IsTrue(target.Next());
            AssertToken.Matches(JsonTokenType.EndProperty, payload, 1, 13, target);
            Assert.IsTrue(target.Next());
            AssertToken.Matches(JsonTokenType.EndObject, payload, 0, 15, target);
            AssertToken.IsComplete(payload, target);
        }

        /// <summary>
        /// Reset expands buffer.
        /// </summary>
        [TestMethod]
        public void JsonParser_Reset_Expands_Buffer()
        {
            string payload = "{\"text\":\"\\n\"}";
            JsonParser target = new JsonParser(payload);
            Assert.IsTrue(target.Next());
            AssertToken.Matches(JsonTokenType.BeginObject, payload, 0, 1, target);
            Assert.IsTrue(target.Next());
            AssertToken.IsProperty("text", payload, 1, 7, target);
            Assert.IsTrue(target.Next());
            AssertToken.IsValue("\n", payload, 8, 4, target);
            Assert.IsTrue(target.Next());
            AssertToken.Matches(JsonTokenType.EndProperty, payload, 1, 11, target);
            Assert.IsTrue(target.Next());
            AssertToken.Matches(JsonTokenType.EndObject, payload, 0, 13, target);
            AssertToken.IsComplete(payload, target);
            StringBuilder builder = new StringBuilder();
            builder.Append("{\"text\":\"");
            for (int i = 0; i < 100000; i++)
            {
                builder.Append("\\n");
            }

            builder.Append("\"}");
            payload = builder.ToString();
            target.Reset(payload);
            Assert.IsTrue(target.Next());
            AssertToken.Matches(JsonTokenType.BeginObject, payload, 0, 1, target);
            Assert.IsTrue(target.Next());
            AssertToken.IsProperty("text", payload, 1, 7, target);
            Assert.IsTrue(target.Next());
            AssertToken.IsValue(new string('\n', 100000), payload, 8, 200002, target);
            Assert.IsTrue(target.Next());
            AssertToken.Matches(JsonTokenType.EndProperty, payload, 1, 200009, target);
            Assert.IsTrue(target.Next());
            AssertToken.Matches(JsonTokenType.EndObject, payload, 0, 200011, target);
            AssertToken.IsComplete(payload, target);
        }

        /// <summary>
        /// TryParseToken parses Boolean.
        /// </summary>
        [TestMethod]
        public void JsonParser_TryParseToken_Parses_Boolean()
        {
            string payload = "true";
            JsonParser target = new JsonParser(payload);
            Assert.IsTrue(target.Next());
            AssertToken.IsValue(true, payload, 0, payload.Length, target);
            AssertToken.IsComplete(payload, target);
            payload = "false";
            target.Reset(payload);
            Assert.IsTrue(target.Next());
            AssertToken.IsValue(false, payload, 0, payload.Length, target);
            AssertToken.IsComplete(payload, target);
        }

        /// <summary>
        /// TryParseToken parses Int64.
        /// </summary>
        [TestMethod]
        public void JsonParser_TryParseToken_Parses_Int64()
        {
            string payload = long.MaxValue.ToString(CultureInfo.InvariantCulture);
            JsonParser target = new JsonParser(payload);
            Assert.IsTrue(target.Next());
            AssertToken.IsValue(long.MaxValue, payload, 0, payload.Length, target);
            AssertToken.IsComplete(payload, target);
            payload = long.MinValue.ToString(CultureInfo.InvariantCulture);
            target.Reset(payload);
            Assert.IsTrue(target.Next());
            AssertToken.IsValue(long.MinValue, payload, 0, payload.Length, target);
            AssertToken.IsComplete(payload, target);
        }

        /// <summary>
        /// TryParseToken parses UInt64.
        /// </summary>
        [TestMethod]
        public void JsonParser_TryParseToken_Parses_UInt64()
        {
            string payload = ulong.MaxValue.ToString(CultureInfo.InvariantCulture);
            JsonParser target = new JsonParser(payload);
            Assert.IsTrue(target.Next());
            AssertToken.IsValue(ulong.MaxValue, payload, 0, payload.Length, target);
            AssertToken.IsComplete(payload, target);
        }

        /// <summary>
        /// TryParseToken parses UInt64.
        /// </summary>
        [TestMethod]
        public void JsonParser_TryParseToken_Parses_Hexadecimal_UInt64()
        {
            string payload = "0x0123456789ABCDEF";
            JsonParser target = new JsonParser(payload);
            Assert.IsTrue(target.Next());
            AssertToken.IsValue(81985529216486895UL, payload, 0, payload.Length, target);
            AssertToken.IsComplete(payload, target);
        }

        /// <summary>
        /// TryParseToken parses Double.
        /// </summary>
        [TestMethod]
        public void JsonParser_TryParseToken_Parses_Double()
        {
            string payload = double.MinValue.ToString("R", CultureInfo.InvariantCulture);
            JsonParser target = new JsonParser(payload);
            Assert.IsTrue(target.Next());
            AssertToken.IsValue(double.MinValue, payload, 0, payload.Length, target);
            AssertToken.IsComplete(payload, target);
            payload = double.Epsilon.ToString("R", CultureInfo.InvariantCulture);
            target.Reset(payload);
            Assert.IsTrue(target.Next());
            AssertToken.IsValue(double.Epsilon, payload, 0, payload.Length, target);
            AssertToken.IsComplete(payload, target);
            payload = double.MaxValue.ToString("R", CultureInfo.InvariantCulture);
            target.Reset(payload);
            Assert.IsTrue(target.Next());
            AssertToken.IsValue(double.MaxValue, payload, 0, payload.Length, target);
            AssertToken.IsComplete(payload, target);
        }

        /// <summary>
        /// TryParseToken parses String.
        /// </summary>
        [TestMethod]
        public void JsonParser_TryParseToken_Parses_String()
        {
            string payload = Constants.UnicodeRainbowEncoded;
            JsonParser target = new JsonParser(payload);
            Assert.IsTrue(target.Next());
            AssertToken.IsValue(Constants.UnicodeRainbowDecoded, payload, 0, payload.Length, target);
            AssertToken.IsComplete(payload, target);
        }

        /// <summary>
        /// TryParseToken ignores array white space.
        /// </summary>
        [TestMethod]
        public void JsonParser_TryParseToken_Ignores_Array_White_Space()
        {
            string payload = " [ 1 , 2 ] ";
            JsonParser target = new JsonParser(payload);
            Assert.IsTrue(target.Next());
            AssertToken.Matches(JsonTokenType.BeginArray, payload, 1, 1, target);
            Assert.IsTrue(target.Next());
            AssertToken.IsValue(1, payload, 3, 1, target);
            Assert.IsTrue(target.Next());
            AssertToken.IsValue(2, payload, 7, 1, target);
            Assert.IsTrue(target.Next());
            AssertToken.Matches(JsonTokenType.EndArray, payload, 1, 9, target);
            AssertToken.IsComplete(payload, target);
        }

        /// <summary>
        /// TryParseToken ignores object white space.
        /// </summary>
        [TestMethod]
        public void JsonParser_TryParseToken_Ignores_Object_White_Space()
        {
            string payload = " { \"key\" : \"value\" } ";
            JsonParser target = new JsonParser(payload);
            Assert.IsTrue(target.Next());
            AssertToken.Matches(JsonTokenType.BeginObject, payload, 1, 1, target);
            Assert.IsTrue(target.Next());
            AssertToken.IsProperty("key", payload, 3, 7, target);
            Assert.IsTrue(target.Next());
            AssertToken.IsValue("value", payload, 11, 7, target);
            Assert.IsTrue(target.Next());
            AssertToken.Matches(JsonTokenType.EndProperty, payload, 3, 15, target);
            Assert.IsTrue(target.Next());
            AssertToken.Matches(JsonTokenType.EndObject, payload, 1, 19, target);
            AssertToken.IsComplete(payload, target);
        }
    }
}
