//-----------------------------------------------------------------------------
// <copyright file="JsonBuilderTests.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
//     Licensed under the MIT license. See license file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------------

namespace Microsoft.Shared.Dna.Json.Test
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests for the <see cref="JsonBuilder"/> class.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class JsonBuilderTests
    {
        /// <summary>
        /// Constructor assigns minimum capacity.
        /// </summary>
        [TestMethod]
        public void JsonBuilder_Constructor_Assigns_Minimum_Capacity()
        {
            JsonBuilder target = new JsonBuilder(0, 1);
            Assert.IsFalse(target.WriteValue(null));
            Assert.AreEqual("{\"(truncated)\":true}", target.Finish());
        }

        /// <summary>
        /// Constructor rejects non-positive depth.
        /// </summary>
        [TestMethod]
        public void JsonBuilder_Constructor_Rejects_Non_Positive_Depth()
        {
            try
            {
                JsonBuilder target = new JsonBuilder(0, 0);
                Assert.IsNull(target);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Assert.AreEqual(typeof(ArgumentOutOfRangeException), ex.GetType());
            }
        }

        /// <summary>
        /// Clear causes Finish to return empty string.
        /// </summary>
        [TestMethod]
        public void JsonBuilder_Clear_Causes_Finish_To_Return_Empty_String()
        {
            JsonBuilder target = new JsonBuilder();
            Assert.IsTrue(target.WriteValue("Should not be returned"));
            target.Clear();
            Assert.AreEqual(string.Empty, target.Finish());
        }

        /// <summary>
        /// OpenArray creates empty array.
        /// </summary>
        [TestMethod]
        public void JsonBuilder_OpenArray_Creates_Empty_Array()
        {
            JsonBuilder target = new JsonBuilder();
            Assert.IsTrue(target.OpenArray());
            Assert.AreEqual("[]", target.Finish());
        }

        /// <summary>
        /// OpenArray can be nested in OpenArray.
        /// </summary>
        [TestMethod]
        public void JsonBuilder_OpenArray_Can_Be_Nested_In_OpenArray()
        {
            JsonBuilder target = new JsonBuilder();
            Assert.IsTrue(target.OpenArray());
            Assert.IsTrue(target.OpenArray());
            Assert.IsTrue(target.WriteValue(1));
            target.CloseToken();
            Assert.IsTrue(target.OpenArray());
            Assert.IsTrue(target.WriteValue(2));
            Assert.AreEqual("[[1],[2]]", target.Finish());
        }

        /// <summary>
        /// OpenArray can be nested in OpenProperty.
        /// </summary>
        [TestMethod]
        public void JsonBuilder_OpenArray_Can_Be_Nested_In_OpenProperty()
        {
            JsonBuilder target = new JsonBuilder();
            Assert.IsTrue(target.OpenObject());
            Assert.IsTrue(target.OpenProperty("first"));
            Assert.IsTrue(target.OpenArray());
            Assert.IsTrue(target.WriteValue(1));
            target.CloseToken();
            target.CloseToken();
            Assert.IsTrue(target.OpenProperty("second"));
            Assert.IsTrue(target.OpenArray());
            Assert.IsTrue(target.WriteValue(2));
            Assert.AreEqual("{\"first\":[1],\"second\":[2]}", target.Finish());
        }

        /// <summary>
        /// OpenObject creates empty object.
        /// </summary>
        [TestMethod]
        public void JsonBuilder_OpenObject_Creates_Empty_Object()
        {
            JsonBuilder target = new JsonBuilder();
            Assert.IsTrue(target.OpenObject());
            Assert.AreEqual("{}", target.Finish());
        }

        /// <summary>
        /// OpenObject can be nested in OpenArray.
        /// </summary>
        [TestMethod]
        public void JsonBuilder_OpenObject_Can_Be_Nested_In_OpenArray()
        {
            JsonBuilder target = new JsonBuilder();
            Assert.IsTrue(target.OpenArray());
            Assert.IsTrue(target.OpenObject());
            Assert.IsTrue(target.OpenProperty("value"));
            Assert.IsTrue(target.WriteValue(1));
            target.CloseToken();
            target.CloseToken();
            Assert.IsTrue(target.OpenObject());
            Assert.IsTrue(target.OpenProperty("value"));
            Assert.IsTrue(target.WriteValue(2));
            Assert.AreEqual("[{\"value\":1},{\"value\":2}]", target.Finish());
        }

        /// <summary>
        /// OpenObject can be nested in OpenProperty.
        /// </summary>
        [TestMethod]
        public void JsonBuilder_OpenObject_Can_Be_Nested_In_OpenProperty()
        {
            JsonBuilder target = new JsonBuilder();
            Assert.IsTrue(target.OpenObject());
            Assert.IsTrue(target.OpenProperty("first"));
            Assert.IsTrue(target.OpenObject());
            Assert.IsTrue(target.OpenProperty("value"));
            Assert.IsTrue(target.WriteValue(1));
            target.CloseToken();
            target.CloseToken();
            target.CloseToken();
            Assert.IsTrue(target.OpenProperty("second"));
            Assert.IsTrue(target.OpenObject());
            Assert.IsTrue(target.OpenProperty("value"));
            Assert.IsTrue(target.WriteValue(2));
            Assert.AreEqual("{\"first\":{\"value\":1},\"second\":{\"value\":2}}", target.Finish());
        }

        /// <summary>
        /// OpenProperty creates null property.
        /// </summary>
        [TestMethod]
        public void JsonBuilder_OpenProperty_Creates_Null_Property()
        {
            JsonBuilder target = new JsonBuilder();
            Assert.IsTrue(target.OpenObject());
            Assert.IsTrue(target.OpenProperty("isNull"));
            Assert.AreEqual("{\"isNull\":null}", target.Finish());
        }

        /// <summary>
        /// OpenProperty creates null property.
        /// </summary>
        [TestMethod]
        public void JsonBuilder_OpenProperty_Rejects_Null_Property_Name()
        {
            JsonBuilder target = new JsonBuilder();
            Assert.IsTrue(target.OpenObject());
            try
            {
                target.OpenProperty(null);
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual(typeof(ArgumentNullException), ex.GetType());
            }
        }

        /// <summary>
        /// TryResize increases capacity and preserves current state.
        /// </summary>
        [TestMethod]
        public void JsonBuilder_TryResize_Increases_Capacity_And_Preserves_Current_State()
        {
            JsonBuilder target = new JsonBuilder(50, 2);
            Assert.IsTrue(target.OpenArray());
            for (int i = 0; i < 10; i++)
            {
                Assert.IsTrue(target.WriteValue(i), "index:{0}", i);
            }

            Assert.IsTrue(target.TryResize(100));
            for (int i = 10; i < 20; i++)
            {
                Assert.IsTrue(target.WriteValue(i), "index:{0}", i);
            }

            Assert.AreEqual("[0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19]", target.Finish());
        }

        /// <summary>
        /// TryResize decreases capacity if state can be preserved.
        /// </summary>
        [TestMethod]
        public void JsonBuilder_TryResize_Decreases_Capacity_If_State_Can_Be_Preserved()
        {
            JsonBuilder target = new JsonBuilder(100, 2);
            Assert.IsTrue(target.OpenArray());
            for (int i = 0; i < 10; i++)
            {
                Assert.IsTrue(target.WriteValue(i), "index:{0}", i);
            }

            Assert.IsTrue(target.TryResize(50));
            Assert.AreEqual("[0,1,2,3,4,5,6,7,8,9]", target.Finish());
        }

        /// <summary>
        /// TryResize does nothing if capacity is below minimum.
        /// </summary>
        [TestMethod]
        public void JsonBuilder_TryResize_Does_Nothing_If_Capacity_Is_Below_Minimum()
        {
            JsonBuilder target = new JsonBuilder(100, 2);
            Assert.IsFalse(target.TryResize(0));
            Assert.IsTrue(target.OpenArray());
            for (int i = 0; i < 20; i++)
            {
                Assert.IsTrue(target.WriteValue(i), "index:{0}", i);
            }

            Assert.AreEqual("[0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19]", target.Finish());
        }

        /// <summary>
        /// TryResize does nothing if state cannot be preserved.
        /// </summary>
        [TestMethod]
        public void JsonBuilder_TryResize_Does_Nothing_If_State_Cannot_Be_Preserved()
        {
            JsonBuilder target = new JsonBuilder(100, 2);
            Assert.IsTrue(target.OpenArray());
            for (int i = 0; i < 20; i++)
            {
                Assert.IsTrue(target.WriteValue(i), "index:{0}", i);
            }

            Assert.IsFalse(target.TryResize(50));
            Assert.AreEqual("[0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19]", target.Finish());
        }

        /// <summary>
        /// WriteValue writes Boolean.
        /// </summary>
        [TestMethod]
        public void JsonBuilder_WriteValue_Writes_Boolean()
        {
            JsonBuilder target = new JsonBuilder();
            Assert.IsTrue(target.WriteValue(true));
            Assert.AreEqual("true", target.Finish());
            target.Clear();
            Assert.IsTrue(target.WriteValue(false));
            Assert.AreEqual("false", target.Finish());
        }

        /// <summary>
        /// WriteValue writes Byte.
        /// </summary>
        [TestMethod]
        public void JsonBuilder_WriteValue_Writes_Byte()
        {
            JsonBuilder target = new JsonBuilder();
            Assert.IsTrue(target.WriteValue(byte.MinValue));
            Assert.AreEqual("0", target.Finish());
            target.Clear();
            Assert.IsTrue(target.WriteValue(byte.MaxValue));
            Assert.AreEqual("255", target.Finish());
        }

        /// <summary>
        /// WriteValue writes SByte.
        /// </summary>
        [TestMethod]
        public void JsonBuilder_WriteValue_Writes_SByte()
        {
            JsonBuilder target = new JsonBuilder();
            Assert.IsTrue(target.WriteValue(sbyte.MinValue));
            Assert.AreEqual("-128", target.Finish());
            target.Clear();
            Assert.IsTrue(target.WriteValue(sbyte.MaxValue));
            Assert.AreEqual("127", target.Finish());
        }

        /// <summary>
        /// WriteValue writes Int16.
        /// </summary>
        [TestMethod]
        public void JsonBuilder_WriteValue_Writes_Int16()
        {
            JsonBuilder target = new JsonBuilder();
            Assert.IsTrue(target.WriteValue(short.MinValue));
            Assert.AreEqual("-32768", target.Finish());
            target.Clear();
            Assert.IsTrue(target.WriteValue(short.MaxValue));
            Assert.AreEqual("32767", target.Finish());
        }

        /// <summary>
        /// WriteValue writes UInt16.
        /// </summary>
        [TestMethod]
        public void JsonBuilder_WriteValue_Writes_UInt16()
        {
            JsonBuilder target = new JsonBuilder();
            Assert.IsTrue(target.WriteValue(ushort.MinValue));
            Assert.AreEqual("0", target.Finish());
            target.Clear();
            Assert.IsTrue(target.WriteValue(ushort.MaxValue));
            Assert.AreEqual("65535", target.Finish());
        }

        /// <summary>
        /// WriteValue writes Int32.
        /// </summary>
        [TestMethod]
        public void JsonBuilder_WriteValue_Writes_Int32()
        {
            JsonBuilder target = new JsonBuilder();
            Assert.IsTrue(target.WriteValue(int.MinValue));
            Assert.AreEqual("-2147483648", target.Finish());
            target.Clear();
            Assert.IsTrue(target.WriteValue(int.MaxValue));
            Assert.AreEqual("2147483647", target.Finish());
        }

        /// <summary>
        /// WriteValue writes UInt32.
        /// </summary>
        [TestMethod]
        public void JsonBuilder_WriteValue_Writes_UInt32()
        {
            JsonBuilder target = new JsonBuilder();
            Assert.IsTrue(target.WriteValue(uint.MinValue));
            Assert.AreEqual("0", target.Finish());
            target.Clear();
            Assert.IsTrue(target.WriteValue(uint.MaxValue));
            Assert.AreEqual("4294967295", target.Finish());
        }

        /// <summary>
        /// WriteValue writes Int64.
        /// </summary>
        [TestMethod]
        public void JsonBuilder_WriteValue_Writes_Int64()
        {
            JsonBuilder target = new JsonBuilder();
            Assert.IsTrue(target.WriteValue(long.MinValue));
            Assert.AreEqual("-9223372036854775808", target.Finish());
            target.Clear();
            Assert.IsTrue(target.WriteValue(long.MaxValue));
            Assert.AreEqual("9223372036854775807", target.Finish());
        }

        /// <summary>
        /// WriteValue writes UInt64.
        /// </summary>
        [TestMethod]
        public void JsonBuilder_WriteValue_Writes_UInt64()
        {
            JsonBuilder target = new JsonBuilder();
            Assert.IsTrue(target.WriteValue(ulong.MinValue));
            Assert.AreEqual("0", target.Finish());
            target.Clear();
            Assert.IsTrue(target.WriteValue(ulong.MaxValue));
            Assert.AreEqual("18446744073709551615", target.Finish());
        }

        /// <summary>
        /// WriteValue writes Single.
        /// </summary>
        [TestMethod]
        public void JsonBuilder_WriteValue_Writes_Single()
        {
            JsonBuilder target = new JsonBuilder();
            Assert.IsTrue(target.WriteValue(float.MinValue));
            Assert.AreEqual("-3.40282347E+38", target.Finish());
            target.Clear();
            Assert.IsTrue(target.WriteValue(float.Epsilon));
            Assert.AreEqual("1.401298E-45", target.Finish());
            target.Clear();
            Assert.IsTrue(target.WriteValue(float.MaxValue));
            Assert.AreEqual("3.40282347E+38", target.Finish());
        }

        /// <summary>
        /// WriteValue writes Double.
        /// </summary>
        [TestMethod]
        public void JsonBuilder_WriteValue_Writes_Double()
        {
            JsonBuilder target = new JsonBuilder();
            Assert.IsTrue(target.WriteValue(double.MinValue));
            Assert.AreEqual("-1.7976931348623157E+308", target.Finish());
            target.Clear();
            Assert.IsTrue(target.WriteValue(double.Epsilon));
            Assert.AreEqual("4.94065645841247E-324", target.Finish());
            target.Clear();
            Assert.IsTrue(target.WriteValue(double.MaxValue));
            Assert.AreEqual("1.7976931348623157E+308", target.Finish());
        }

        /// <summary>
        /// WriteValue writes decimal.
        /// </summary>
        [TestMethod]
        public void JsonBuilder_WriteValue_Writes_Decimal()
        {
            JsonBuilder target = new JsonBuilder();
            Assert.IsTrue(target.WriteValue(decimal.MinValue));
            Assert.AreEqual("-79228162514264337593543950335", target.Finish());
            target.Clear();
            Assert.IsTrue(target.WriteValue(decimal.MaxValue));
            Assert.AreEqual("79228162514264337593543950335", target.Finish());
        }

        /// <summary>
        /// WriteValue writes String.
        /// </summary>
        [TestMethod]
        public void JsonBuilder_WriteValue_Writes_String()
        {
            JsonBuilder target = new JsonBuilder(70000, 1);
            Assert.IsTrue(target.WriteValue(null));
            Assert.AreEqual("null", target.Finish());
            target.Clear();
            Assert.IsTrue(target.WriteValue(string.Empty));
            Assert.AreEqual("\"\"", target.Finish());
            target.Clear();
            Assert.IsTrue(target.WriteValue(Constants.UnicodeRainbowDecoded));
            string encoded = target.Finish();
            for (int i = 0; i < Constants.UnicodeRainbowEncoded.Length; i++)
            {
                Assert.AreEqual(Constants.UnicodeRainbowEncoded[i], encoded[i], "index:{0}", i);
            }
        }

        /// <summary>
        /// Writing truncates array when capacity is exceeded.
        /// </summary>
        [TestMethod]
        public void JsonBuilder_Writing_Truncates_Array_When_Capacity_Is_Exceeded()
        {
            JsonBuilder target = new JsonBuilder(50, 2);
            Assert.IsTrue(target.OpenArray());
            bool succeeded = true;
            for (int i = 0; succeeded; i++)
            {
                succeeded = target.WriteValue(i);
            }

            Assert.AreEqual("[0,1,2,3,4,5,6,7,8,9,10,11,{\"(truncated)\":true}]", target.Finish());
        }

        /// <summary>
        /// Writing truncates object when capacity is exceeded.
        /// </summary>
        [TestMethod]
        public void JsonBuilder_Writing_Truncates_Object_When_Capacity_Is_Exceeded()
        {
            JsonBuilder target = new JsonBuilder(50, 4);
            Assert.IsTrue(target.OpenObject());
            bool succeeded = true;
            for (int i = 0; succeeded; i++)
            {
                if (target.OpenProperty(i.ToString(CultureInfo.InvariantCulture)))
                {
                    succeeded = target.WriteValue(i);
                    target.CloseToken();
                    continue;
                }

                break;
            }

            Assert.AreEqual("{\"0\":0,\"1\":1,\"2\":2,\"3\":3,\"(truncated)\":true}", target.Finish());
        }

        /// <summary>
        /// Writing truncates value when capacity is exceeded.
        /// </summary>
        [TestMethod]
        public void JsonBuilder_Writing_Truncates_Value_When_Capacity_Is_Exceeded()
        {
            JsonBuilder target = new JsonBuilder(20, 1);
            target.WriteValue("ABCDEFGHIJKLMNOPQRSTUVWXYZ");
            Assert.AreEqual("{\"(truncated)\":true}", target.Finish());
        }
    }
}
