//-----------------------------------------------------------------------------
// <copyright file="JsonConstants.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
//     Licensed under the MIT license. See license file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------------

namespace Microsoft.Shared.Dna.Json
{
    /// <summary>
    /// Constant JSON values used by both <see cref="JsonBuilder"/> and
    /// <see cref="JsonParser"/>.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Microsoft.Performance",
        "CA1812:AvoidUninstantiatedInternalClasses",
        Justification = "You don't have to use everything in an in-line code share.")]
    internal static class JsonConstants
    {
        /// <summary>
        /// The array header character.
        /// </summary>
        public const char ArrayHeader = '[';

        /// <summary>
        /// The array footer character.
        /// </summary>
        public const char ArrayFooter = ']';

        /// <summary>
        /// Maximum character length of a boolean value.
        /// </summary>
        public const int BooleanValueLength = 5;

        /// <summary>
        /// The escape character.
        /// </summary>
        public const char CharacterEscape = '\\';

        /// <summary>
        /// The decimal point character.
        /// </summary>
        public const char DecimalPoint = '.';

        /// <summary>
        /// The base of a decimal number.
        /// </summary>
        public const byte DecimalRadix = 10;

        /// <summary>
        /// The default capacity for all internal character buffers.
        /// </summary>
        /// <remarks>
        /// Keep this under the large object limit.
        /// </remarks>
        public const int DefaultCapacity = 40000;

        /// <summary>
        /// The default depth for all internal context stacks.
        /// </summary>
        public const int DefaultDepth = 20;

        /// <summary>
        /// The element separator character.
        /// </summary>
        public const char ElementSeparator = ',';

        /// <summary>
        /// The lower-case exponent character.
        /// </summary>
        public const char ExponentLowercase = 'e';

        /// <summary>
        /// The upper-case exponent character.
        /// </summary>
        public const char ExponentUppercase = 'E';

        /// <summary>
        /// The first character of the false literal.
        /// </summary>
        public const char FalseLeadCharacter = 'f';

        /// <summary>
        /// The false literal string.
        /// </summary>
        public const string FalseValue = "false";

        /// <summary>
        /// The character length of the false literal string.
        /// </summary>
        public const int FalseValueLength = 5;

        /// <summary>
        /// The lower-case hexadecimal indicator.
        /// </summary>
        public const char HexLowercase = 'x';

        /// <summary>
        /// The base of a hexadecimal number.
        /// </summary>
        public const byte HexRadix = 16;

        /// <summary>
        /// The upper-case hexadecimal indicator.
        /// </summary>
        public const char HexUppercase = 'X';

        /// <summary>
        /// The name-value separator character.
        /// </summary>
        public const char NameValueSeparator = ':';

        /// <summary>
        /// The negative sign character.
        /// </summary>
        public const char NegativeSign = '-';

        /// <summary>
        /// The first character of the null literal.
        /// </summary>
        public const char NullLeadCharacter = 'n';

        /// <summary>
        /// The null literal string.
        /// </summary>
        public const string NullValue = "null";

        /// <summary>
        /// The character length of the null literal string.
        /// </summary>
        public const int NullValueLength = 4;

        /// <summary>
        /// The object header character.
        /// </summary>
        public const char ObjectHeader = '{';

        /// <summary>
        /// The object footer character.
        /// </summary>
        public const char ObjectFooter = '}';

        /// <summary>
        /// The negative sign character.
        /// </summary>
        public const char PositiveSign = '+';

        /// <summary>
        /// The string enclosure character.
        /// </summary>
        public const char StringEnclosure = '"';

        /// <summary>
        /// The first character of the true literal.
        /// </summary>
        public const char TrueLeadCharacter = 't';

        /// <summary>
        /// The true literal string.
        /// </summary>
        public const string TrueValue = "true";

        /// <summary>
        /// The character length of the true literal string.
        /// </summary>
        public const int TrueValueLength = 4;

        /// <summary>
        /// The truncated flag as an object.
        /// </summary>
        public const string TruncatedObject = "{\"(truncated)\":true}";

        /// <summary>
        /// The character length of the truncated flag as an object.
        /// </summary>
        public const int TruncatedObjectLength = 20;

        /// <summary>
        /// The truncated flag as a property.
        /// </summary>
        public const string TruncatedProperty = "\"(truncated)\":true";

        /// <summary>
        /// The character length of the truncated flag as a property.
        /// </summary>
        public const int TruncatedPropertyLength = 18;

        /// <summary>
        /// The zero character.
        /// </summary>
        public const char Zero = '0';

        /// <summary>
        /// The decimal digit lookup table.
        /// </summary>
        public static readonly sbyte[] DecimalDigits = { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

        /// <summary>
        /// The element length of the decimal digit lookup table.
        /// </summary>
        public static readonly int DecimalDigitsLength = JsonConstants.DecimalDigits.Length;

        /// <summary>
        /// The character escape sequences lookup table.
        /// </summary>
        public static readonly string[] EscapeSequences = { "\\u0000", "\\u0001", "\\u0002", "\\u0003", "\\u0004", "\\u0005", "\\u0006", "\\u0007", "\\b", "\\t", "\\n", "\\u000B", "\\f", "\\r", "\\u000E", "\\u000F", "\\u0010", "\\u0011", "\\u0012", "\\u0013", "\\u0014", "\\u0015", "\\u0016", "\\u0017", "\\u0018", "\\u0019", "\\u001A", "\\u001B", "\\u001C", "\\u001D", "\\u001E", "\\u001F", null, null, "\\\"", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "\\\\", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "\\u007F", "\\u0080", "\\u0081", "\\u0082", "\\u0083", "\\u0084", "\\u0085", "\\u0086", "\\u0087", "\\u0088", "\\u0089", "\\u008A", "\\u008B", "\\u008C", "\\u008D", "\\u008E", "\\u008F", "\\u0090", "\\u0091", "\\u0092", "\\u0093", "\\u0094", "\\u0095", "\\u0096", "\\u0097", "\\u0098", "\\u0099", "\\u009A", "\\u009B", "\\u009C", "\\u009D", "\\u009E", "\\u009F" };

        /// <summary>
        /// The element length of the character escape sequences lookup table.
        /// </summary>
        public static readonly int EscapeSequencesLength = JsonConstants.EscapeSequences.Length;

        /// <summary>
        /// The hexadecimal digit lookup table.
        /// </summary>
        public static readonly sbyte[] HexDigits = { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, -1, -1, -1, -1, -1, -1, -1, 10, 11, 12, 13, 14, 15, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 10, 11, 12, 13, 14, 15 };

        /// <summary>
        /// The element length of the hexadecimal digit lookup table.
        /// </summary>
        public static readonly int HexDigitsLength = JsonConstants.HexDigits.Length;

        /// <summary>
        /// The character un-escape sequences lookup table.
        /// </summary>
        public static readonly char[] UnescapeSequences = { '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\"', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '/', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\\', '\0', '\0', '\0', '\0', '\0', '\b', '\0', '\0', '\0', '\f', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\n', '\0', '\0', '\0', '\r', '\0', '\t' };

        /// <summary>
        /// The element length of the character un-escape sequences lookup table.
        /// </summary>
        public static readonly int UnescapeSequencesLength = JsonConstants.UnescapeSequences.Length;
    }
}
