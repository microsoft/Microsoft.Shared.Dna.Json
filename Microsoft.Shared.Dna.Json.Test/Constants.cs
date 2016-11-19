//-----------------------------------------------------------------------------
// <copyright file="Constants.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
//     Licensed under the MIT license. See license file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------------

namespace Microsoft.Shared.Dna.Json.Test
{
    using System;
    using System.Text;

    /// <summary>
    /// Test constants.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// A string containing every unicode character.
        /// </summary>
        public static readonly string UnicodeRainbowDecoded = Constants.CreateUnicodeRainbowDecoded();

        /// <summary>
        /// A string containing every unicode character encoded as a JSON escape sequence.
        /// </summary>
        public static readonly string UnicodeRainbowEncoded = Constants.CreateUnicodeRainbowEncoded();

        /// <summary>
        /// Create the unicode rainbow.
        /// </summary>
        /// <returns>A string containing every unicode character.</returns>
        private static string CreateUnicodeRainbowDecoded()
        {
            char[] result = new char[char.MaxValue + 1];
            for (char c = char.MinValue; c < char.MaxValue; c++)
            {
                result[c] = c;
            }

            result[char.MaxValue] = char.MaxValue;
            return new string(result);
        }

        /// <summary>
        /// Create a JSON-encoded version of the unicode rainbow.
        /// </summary>
        /// <returns>A string containing every unicode character encoded as a JSON escape sequence.</returns>
        private static string CreateUnicodeRainbowEncoded()
        {
            StringBuilder result = new StringBuilder(70000);
            result.Append("\"");
            for (char c = char.MinValue; c < char.MaxValue; c++)
            {
                switch (c)
                {
                    case '"':
                        result.Append("\\\"");
                        break;
                    case '\\':
                        result.Append("\\\\");
                        break;
                    case '\b':
                        result.Append("\\b");
                        break;
                    case '\f':
                        result.Append("\\f");
                        break;
                    case '\n':
                        result.Append("\\n");
                        break;
                    case '\r':
                        result.Append("\\r");
                        break;
                    case '\t':
                        result.Append("\\t");
                        break;
                    default:
                        if (char.IsControl(c))
                        {
                            result.Append("\\u");
                            byte[] bytes = BitConverter.GetBytes(c);
                            if (BitConverter.IsLittleEndian)
                            {
                                Array.Reverse(bytes);
                            }

                            for (int i = 0; i < bytes.Length; i++)
                            {
                                result.Append(BitConverter.ToString(bytes, i, 1));
                            }
                        }
                        else
                        {
                            result.Append(c);
                        }

                        break;
                }
            }

            result.Append(char.MaxValue);
            result.Append("\"");
            return result.ToString();
        }
    }
}
