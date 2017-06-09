//-----------------------------------------------------------------------------
// <copyright file="JsonParser.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
//     Licensed under the MIT license. See license file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------------

namespace Microsoft.Shared.Dna.Json
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using Microsoft.Shared.Dna.Text;

    /// <summary>
    /// Parses JSON strings in a fast, forward-only manner.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Microsoft.Performance",
        "CA1812:AvoidUninstantiatedInternalClasses",
        Justification = "You don't have to use everything in an in-line code share.")]
    internal sealed class JsonParser
    {
        /// <summary>
        /// Whether or not to close the token in the next iteration.
        /// </summary>
        private bool close = false;

        /// <summary>
        /// Whether or not the token needs string escape sequence decoding.
        /// </summary>
        private bool decode = false;

        /// <summary>
        /// The buffer used for decoding strings.
        /// </summary>
        private FixedStringBuilder decodeBuffer = null;

        /// <summary>
        /// The payload being parsed.
        /// </summary>
        private string payload = null;

        /// <summary>
        /// The character length of the payload being parsed.
        /// </summary>
        private int payloadLength = 0;

        /// <summary>
        /// The current index inside the payload of the parser.
        /// </summary>
        private int position = 0;

        /// <summary>
        /// JSON token scope.
        /// </summary>
        private Stack<Container> scope = null;

        /// <summary>
        /// The character count of the token from the offset.
        /// </summary>
        private int segmentCount = 0;

        /// <summary>
        /// The offset of the token segment.
        /// </summary>
        private int segmentOffset = 0;

        /// <summary>
        /// Type of the current token.
        /// </summary>
        private JsonTokenType tokenType = JsonTokenType.None;

        /// <summary>
        /// Truthfulness of the token. Currently used only to cache Boolean result.
        /// </summary>
        private bool truth = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonParser"/> class.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        public JsonParser()
            : this(string.Empty, JsonConstants.DefaultCapacity, JsonConstants.DefaultDepth)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonParser"/> class.
        /// </summary>
        /// <param name="json">The JSON payload to parse.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        public JsonParser(string json)
            : this(json, JsonConstants.DefaultCapacity, JsonConstants.DefaultDepth)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonParser"/> class.
        /// </summary>
        /// <param name="json">The JSON payload to parse.</param>
        /// <param name="capacity">
        /// The initial capacity for the string decoding buffer. It will always at least
        /// cover the size of the payload.
        /// </param>
        /// <param name="depth">
        /// The initial depth of the token scope stack. The stack will grow depending on how
        /// deeply nested the object is.
        /// </param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        public JsonParser(string json, int capacity, int depth)
        {
#if CONTRACTS_FULL // Work around the implicit rewrite requirements of Contract.Requires<T>
            Contract.Requires<ArgumentOutOfRangeException>(depth > 0);
#endif
            this.decodeBuffer = new FixedStringBuilder(Math.Max(capacity, json == null ? 0 : json.Length));
            this.scope = new Stack<Container>(depth);
            this.Reset(json);
        }

        /// <summary>
        /// Gets the delimited string segment of the current token.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        public StringSegment TokenSegment
        {
            get
            {
                return new StringSegment(this.payload, this.segmentOffset, this.segmentCount);
            }
        }

        /// <summary>
        /// Gets the type of the current token.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        public JsonTokenType TokenType
        {
            get
            {
                return this.tokenType;
            }
        }

        /// <summary>
        /// Gets the current token scope.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        private Container Current
        {
            get
            {
                return this.scope.Peek();
            }
        }

        /// <summary>
        /// Attempts to bypass an invalid token so that parsing may continue.
        /// </summary>
        public void Bypass()
        {
            if (this.tokenType == JsonTokenType.Invalid)
            {
                unsafe
                {
                    fixed (char* payloadPointer = this.payload)
                    {
                        this.PrepareForClose(payloadPointer, false);
                    }
                }

                if (!this.close)
                {
                    this.position++;
                }

                this.tokenType = JsonTokenType.Bypass;
            }
        }

        /// <summary>
        /// Advances the parser to the next token.
        /// </summary>
        /// <returns>
        /// A value indicating whether or not parsing may continue.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        public bool Next()
        {
            if (this.TokenType.IsEndOfPayload())
            {
                return false;
            }

            this.decode = false;
            this.NextUnsafe();
            return !this.TokenType.IsEndOfPayload();
        }

        /// <summary>
        /// Advances the parser straight to the end of the container that it is currently
        /// inside of.
        /// </summary>
        /// <returns>
        /// A value indicating whether or not parsing may continue.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        public bool Skip()
        {
            int depth = this.scope.Count;
            while (depth <= this.scope.Count)
            {
                if (!this.Next())
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Resets the parser with a new string. The instance can be reused after this method returns.
        /// </summary>
        /// <param name="json">The new JSON payload to parse.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        public void Reset(string json)
        {
            this.close = false;
            this.segmentCount = 0;
            this.decode = false;
            this.decodeBuffer.Clear();
            this.segmentOffset = 0;
            this.payload = json ?? string.Empty;
            this.payloadLength = this.payload.Length;
            this.decodeBuffer.TryExpand(this.payloadLength);
            this.position = 0;
            this.scope.Clear();
            this.scope.Push(Container.Root);
            this.tokenType = JsonTokenType.None;
        }

        /// <summary>
        /// Tries to parse the current token.
        /// </summary>
        /// <param name="value">The managed value of the token.</param>
        /// <returns>
        /// A value indicating whether or not the token could be parsed. Failure to parse
        /// indicates either malformed JSON or that the token cannot be converted to the
        /// value type.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        public bool TryParseToken(out bool value)
        {
            value = default(bool);
            if ((this.TokenType & JsonTokenType.Boolean) == JsonTokenType.None)
            {
                return false;
            }

            value = this.truth;
            return true;
        }

        /// <summary>
        /// Tries to parse the current token.
        /// </summary>
        /// <param name="value">The managed value of the token.</param>
        /// <returns>
        /// A value indicating whether or not the token could be parsed. Failure to parse
        /// indicates either malformed JSON or that the token cannot be converted to the
        /// value type.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        public bool TryParseToken(out bool? value)
        {
            value = null;
            if (this.TokenType.IsNull())
            {
                return true;
            }

            bool actual = default(bool);
            if (this.TryParseToken(out actual))
            {
                value = actual;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Tries to parse the current token.
        /// </summary>
        /// <param name="value">The managed value of the token.</param>
        /// <returns>
        /// A value indicating whether or not the token could be parsed. Failure to parse
        /// indicates either malformed JSON or that the token cannot be converted to the
        /// value type.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        public bool TryParseToken(out long value)
        {
            value = default(long);
            if (this.TokenType != JsonTokenType.Integer)
            {
                return false;
            }

            return this.TryParseTokenUnsafe(out value);
        }

        /// <summary>
        /// Tries to parse the current token.
        /// </summary>
        /// <param name="value">The managed value of the token.</param>
        /// <returns>
        /// A value indicating whether or not the token could be parsed. Failure to parse
        /// indicates either malformed JSON or that the token cannot be converted to the
        /// value type.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        public bool TryParseToken(out long? value)
        {
            value = null;
            if (this.TokenType.IsNull())
            {
                return true;
            }

            long actual = default(long);
            if (this.TryParseToken(out actual))
            {
                value = actual;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Tries to parse the current token.
        /// </summary>
        /// <param name="value">The managed value of the token.</param>
        /// <returns>
        /// A value indicating whether or not the token could be parsed. Failure to parse
        /// indicates either malformed JSON or that the token cannot be converted to the
        /// value type.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        public bool TryParseToken(out ulong value)
        {
            value = default(ulong);
            if (this.TokenType != JsonTokenType.Integer)
            {
                return false;
            }

            return this.TryParseTokenUnsafe(out value);
        }

        /// <summary>
        /// Tries to parse the current token.
        /// </summary>
        /// <param name="value">The managed value of the token.</param>
        /// <returns>
        /// A value indicating whether or not the token could be parsed. Failure to parse
        /// indicates either malformed JSON or that the token cannot be converted to the
        /// value type.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        public bool TryParseToken(out ulong? value)
        {
            value = null;
            if (this.TokenType.IsNull())
            {
                return true;
            }

            ulong actual = default(ulong);
            if (this.TryParseToken(out actual))
            {
                value = actual;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Tries to parse the current token.
        /// </summary>
        /// <param name="value">The managed value of the token.</param>
        /// <returns>
        /// A value indicating whether or not the token could be parsed. Failure to parse
        /// indicates either malformed JSON or that the token cannot be converted to the
        /// value type.
        /// </returns>
        /// <remarks>
        /// Punting on this one by allowing it to allocate an ephemeral string to parse.
        /// Dealing with floating point rounding errors is just too fragile.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        public bool TryParseToken(out double value)
        {
            value = default(double);
            if (this.TokenType != JsonTokenType.Float && this.TokenType != JsonTokenType.Integer)
            {
                return false;
            }

            return double.TryParse(this.TokenSegment.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }

        /// <summary>
        /// Tries to parse the current token.
        /// </summary>
        /// <param name="value">The managed value of the token.</param>
        /// <returns>
        /// A value indicating whether or not the token could be parsed. Failure to parse
        /// indicates either malformed JSON or that the token cannot be converted to the
        /// value type.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        public bool TryParseToken(out double? value)
        {
            value = null;
            if (this.TokenType.IsNull())
            {
                return true;
            }

            double actual = default(double);
            if (this.TryParseToken(out actual))
            {
                value = actual;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Tries to parse the current token.
        /// </summary>
        /// <param name="value">The managed value of the token.</param>
        /// <returns>
        /// A value indicating whether or not the token could be parsed. Failure to parse
        /// indicates either malformed JSON or that the token cannot be converted to the
        /// value type.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        public bool TryParseToken(out string value)
        {
            value = null;
            if (this.TokenType.IsNull())
            {
                return true;
            }

            int trim = 0;
            if (this.TokenType == JsonTokenType.String)
            {
                trim = 2;
            }
            else if (this.TokenType == JsonTokenType.BeginProperty)
            {
                int cursor = this.TokenSegment.Offset + this.TokenSegment.Count - 2;
                trim = cursor - this.EatWhitespaceReverse(cursor) + 3;
            }

            if (trim > 0)
            {
                StringSegment subsegment = new StringSegment(this.TokenSegment.String, this.TokenSegment.Offset + 1, this.TokenSegment.Count - trim);
                /* Skip the enclosing quotation marks. */
                if (!this.decode)
                {
                    /* If there are no character escapes, then skip the more complicated decoding logic. */
                    value = subsegment.ToString();
                    return true;
                }

                return this.TryParseTokenUnsafe(subsegment, out value);
            }
            else
            {
                value = this.TokenSegment.ToString();
            }

            return true;
        }

        /// <summary>
        /// Determines if a character is insignificant whitespace.
        /// </summary>
        /// <param name="c">The character to evaluate.</param>
        /// <returns>A value indicating whether or not the character is insignificant whitespace.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        private static bool IsJsonWhitespace(char c)
        {
            return c == JsonConstants.Space || c == JsonConstants.HorizontalTab || c == JsonConstants.CarriageReturn || c == JsonConstants.LineFeed;
        }

        /// <summary>
        /// Tries to convert a character to its decimal equivalent.
        /// </summary>
        /// <param name="character">The character to convert.</param>
        /// <param name="digit">The decimal value.</param>
        /// <returns>
        /// A value indicating whether or not the character is a decimal digit.
        /// </returns>
        /// <remarks>
        /// Don't use the value of digit if this returns false.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        private static bool TryConvertDecimal(char character, out byte digit)
        {
            digit = default(byte);
            int asIndex = character;
            if (asIndex >= JsonConstants.DecimalDigitsLength)
            {
                return false;
            }

            sbyte quarantine = JsonConstants.DecimalDigits[asIndex];
            if (quarantine < 0)
            {
                return false;
            }

            digit = (byte)quarantine;
            return true;
        }

        /// <summary>
        /// Tries to convert a character to its hexadecimal equivalent.
        /// </summary>
        /// <param name="character">The character to convert.</param>
        /// <param name="digit">The hexadecimal value.</param>
        /// <returns>
        /// A value indicating whether or not the character is a hexadecimal digit.
        /// </returns>
        /// <remarks>
        /// Don't use the value of digit if this returns false.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        private static bool TryConvertHex(char character, out byte digit)
        {
            digit = default(byte);
            int asIndex = character;
            if (asIndex >= JsonConstants.HexDigitsLength)
            {
                return false;
            }

            sbyte quarantine = JsonConstants.HexDigits[asIndex];
            if (quarantine < 0)
            {
                return false;
            }

            digit = (byte)quarantine;
            return true;
        }

        /// <summary>
        /// Tries to parse a hexadecimal character escape sequence.
        /// </summary>
        /// <param name="payloadPointer">A pointer to the payload.</param>
        /// <param name="first">
        /// The character offset of the beginning of the sequence.
        /// </param>
        /// <param name="last">The character offset of the end of the sequence.</param>
        /// <param name="value">The character code of the escape sequence.</param>
        /// <returns>
        /// A value indicating whether or not the sequence represents a valid unicode
        /// character code.
        /// </returns>
        /// <remarks>
        /// This method uses character pointer arithmetic to iterate over the string because
        /// it is significantly faster than using the string indexer.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        private static unsafe bool TryParseHex(char* payloadPointer, int first, int last, out ushort value)
        {
            value = default(ushort);
            ushort accumulator = 0;
            byte digit = default(byte);
            for (int i = first; i < last; i++)
            {
                if (JsonParser.TryConvertHex(*(payloadPointer + i), out digit))
                {
                    try
                    {
                        accumulator = checked((ushort)((accumulator * JsonConstants.HexRadix) + digit));
                    }
                    catch (OverflowException)
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            value = accumulator;
            return true;
        }

        /// <summary>
        /// Creates the completion token for the parser.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        private void CreateCompleteToken()
        {
            this.tokenType = JsonTokenType.Complete;
            this.segmentOffset = 0;
            this.position = this.payloadLength;
            this.segmentCount = this.payloadLength;
            this.close = false;
        }

        /// <summary>
        /// Creates an invalid token for the parser.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        private void CreateInvalidToken()
        {
            this.segmentOffset = this.position;
            this.segmentCount = 0;
            this.tokenType = JsonTokenType.Invalid;
        }

        /// <summary>
        /// Creates a token for the parser.
        /// </summary>
        /// <param name="count">
        /// The number of characters in the token from the current position.
        /// </param>
        /// <param name="type">The type of token being represented.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        private void CreateToken(int count, JsonTokenType type)
        {
            if (this.position + count > this.payloadLength)
            {
                this.CreateInvalidToken();
            }
            else
            {
                this.segmentOffset = this.position;
                this.segmentCount = count;
                this.tokenType = type;
                this.position += count;
            }
        }

        /// <summary>
        /// Skips past an element separator in an array or object.
        /// </summary>
        /// <param name="payloadPointer">A pointer to the payload.</param>
        /// <param name="any">A value indicating whether or not any elements have been encountered inside the container.</param>
        /// <remarks>
        /// This method uses character pointer arithmetic to iterate over the string because
        /// it is significantly faster than using the string indexer.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        private unsafe void EatElementSeparator(char* payloadPointer, bool any)
        {
            if (*(payloadPointer + this.position) == JsonConstants.ElementSeparator)
            {
                this.position++;
            }
            else if (any)
            {
                this.CreateInvalidToken();
            }
        }

        /// <summary>
        /// Skips past any insignificant whitespace in the payload.
        /// </summary>
        /// <param name="payloadPointer">A pointer to the payload.</param>
        /// <remarks>
        /// This method uses character pointer arithmetic to iterate over the string because
        /// it is significantly faster than using the string indexer.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        private unsafe void EatWhitespace(char* payloadPointer)
        {
            this.EatWhitespace(payloadPointer, ref this.position);
        }

        /// <summary>
        /// Skips past any insignificant whitespace in the payload.
        /// </summary>
        /// <param name="payloadPointer">A pointer to the payload.</param>
        /// <param name="cursor">
        /// The position in the payload before and after the skip.
        /// </param>
        /// <remarks>
        /// This method uses character pointer arithmetic to iterate over the string because
        /// it is significantly faster than using the string indexer.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        private unsafe void EatWhitespace(char* payloadPointer, ref int cursor)
        {
            while (cursor < this.payloadLength && JsonParser.IsJsonWhitespace(*(payloadPointer + cursor)))
            {
                cursor++;
            }
        }

        /// <summary>
        /// Skips past any insignificant whitespace in the payload in reverse.
        /// </summary>
        /// <param name="cursor">
        /// The position in the payload before the skip.
        /// </param>
        /// <returns>
        /// The position in the payload after the skip.
        /// </returns>
        /// <remarks>
        /// This method uses character pointer arithmetic to iterate over the string because
        /// it is significantly faster than using the string indexer.
        /// </remarks>
        private unsafe int EatWhitespaceReverse(int cursor)
        {
            int result = cursor;
            fixed (char* payloadPointer = this.payload)
            {
                while (result >= 0 && JsonParser.IsJsonWhitespace(*(payloadPointer + result)))
                {
                    result--;
                }
            }

            return result;
        }

        /// <summary>
        /// Finds the end of a series of decimal digits.
        /// </summary>
        /// <param name="payloadPointer">A pointer to the payload.</param>
        /// <param name="cursor">The starting point of the decimal digits.</param>
        /// <returns>
        /// The first non-decimal character seen or the original cursor if it is past the
        /// end of the payload.
        /// </returns>
        /// <remarks>
        /// This method uses character pointer arithmetic to iterate over the string because
        /// it is significantly faster than using the string indexer.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        private unsafe int EndOfDigits(char* payloadPointer, int cursor)
        {
            int result = cursor;
            while (result < this.payloadLength)
            {
                char c = *(payloadPointer + result);
                if (c >= JsonConstants.Zero && c <= JsonConstants.Nine)
                {
                    result++;
                }
                else
                {
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// Finds the end of a series of hexadecimal digits.
        /// </summary>
        /// <param name="payloadPointer">A pointer to the payload.</param>
        /// <param name="cursor">The starting point of the hexadecimal digits.</param>
        /// <returns>
        /// The first non-hexadecimal character seen or the original cursor if it is past
        /// the end of the payload.
        /// </returns>
        /// <remarks>
        /// This method uses character pointer arithmetic to iterate over the string because
        /// it is significantly faster than using the string indexer.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        private unsafe int EndOfHexDigits(char* payloadPointer, int cursor)
        {
            int result = cursor;
            while (result < this.payloadLength)
            {
                char c = *(payloadPointer + result);
                if ((c >= JsonConstants.Zero && c <= JsonConstants.Nine) || (c >= JsonConstants.HexTenUppercase && c <= JsonConstants.HexFifteenUppercase) || (c >= JsonConstants.HexTenLowercase && c <= JsonConstants.HexFifteenLowercase))
                {
                    result++;
                }
                else
                {
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// Finds the end of a string token.
        /// </summary>
        /// <param name="payloadPointer">A pointer to the payload.</param>
        /// <returns>The position of the end of the string.</returns>
        /// <remarks>
        /// This method uses character pointer arithmetic to iterate over the string because
        /// it is significantly faster than using the string indexer.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        private unsafe int EndOfString(char* payloadPointer)
        {
            int result = this.position + 1;
            bool escaped = false;
            while (result < this.payloadLength)
            {
                char c = *(payloadPointer + result);
                result++;
                if (escaped)
                {
                    escaped = false;
                }
                else
                {
                    if (c == JsonConstants.StringEnclosure)
                    {
                        return result;
                    }

                    if (c == JsonConstants.CharacterEscape)
                    {
                        escaped = true;
                        this.decode = true;
                    }
                }
            }

            return -1;
        }

        /// <summary>
        /// Advances the parser to the next token.
        /// </summary>
        /// <remarks>
        /// This method uses character pointer arithmetic to iterate over the string because
        /// it is significantly faster than using the string indexer.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        private unsafe void NextUnsafe()
        {
            fixed (char* payloadPointer = this.payload)
            {
                this.EatWhitespace(payloadPointer);
                Container current = this.Current;
                switch (current.TokenType)
                {
                    case JsonTokenType.BeginArray:
                        if (this.close)
                        {
                            this.ReadEndArray(payloadPointer);
                            return;
                        }

                        break;
                    case JsonTokenType.BeginObject:
                        if (this.close)
                        {
                            this.ReadEndObject(payloadPointer);
                        }
                        else
                        {
                            this.ReadBeginProperty(payloadPointer);
                        }

                        return;
                    case JsonTokenType.BeginProperty:
                        if (this.close)
                        {
                            this.ReadEndProperty(payloadPointer);
                            return;
                        }

                        break;
                }

                this.ReadToken(payloadPointer);
            }
        }

        /// <summary>
        /// Prepares the parser to close out an open token in the next iteration, the
        /// content being parsed in the current iteration.
        /// </summary>
        /// <param name="payloadPointer">A pointer to the payload.</param>
        /// <param name="any">A value indicating whether or not any elements have been encountered inside the container.</param>
        /// <remarks>
        /// This method uses character pointer arithmetic to iterate over the string because
        /// it is significantly faster than using the string indexer.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        private unsafe void PrepareForClose(char* payloadPointer, bool any)
        {
            Container current = this.Current;
            this.EatWhitespace(payloadPointer);
            if (this.position >= this.payloadLength)
            {
                if (this.Current.TokenType == JsonTokenType.None)
                {
                    this.close = true;
                }
                else
                {
                    this.CreateInvalidToken();
                }

                return;
            }

            char c = *(payloadPointer + this.position);
            switch (current.TokenType)
            {
                case JsonTokenType.BeginArray:
                    if (c == JsonConstants.ArrayFooter)
                    {
                        this.close = true;
                    }
                    else
                    {
                        this.EatElementSeparator(payloadPointer, any);
                    }

                    break;
                case JsonTokenType.BeginObject:
                    if (c == JsonConstants.ObjectFooter)
                    {
                        this.close = true;
                    }
                    else
                    {
                        this.EatElementSeparator(payloadPointer, any);
                    }

                    break;
                case JsonTokenType.BeginProperty:
                    this.close = true;
                    break;
            }
        }

        /// <summary>
        /// Reads the beginning of an array.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        private void ReadBeginArray()
        {
            this.scope.Push(new Container(JsonTokenType.BeginArray, this.position));
            this.CreateToken(1, JsonTokenType.BeginArray);
        }

        /// <summary>
        /// Reads the beginning of an object.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        private void ReadBeginObject()
        {
            this.scope.Push(new Container(JsonTokenType.BeginObject, this.position));
            this.CreateToken(1, JsonTokenType.BeginObject);
        }

        /// <summary>
        /// Reads the beginning of a property.
        /// </summary>
        /// <param name="payloadPointer">A pointer to the payload.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        private unsafe void ReadBeginProperty(char* payloadPointer)
        {
            int cursor = this.EndOfString(payloadPointer);
            if (cursor > 0)
            {
                this.EatWhitespace(payloadPointer, ref cursor);
                char c = *(payloadPointer + cursor);
                if (c == JsonConstants.NameValueSeparator)
                {
                    cursor++;
                    this.scope.Push(new Container(JsonTokenType.BeginProperty, this.position));
                    this.CreateToken(cursor - this.position, JsonTokenType.BeginProperty);
                    return;
                }
            }

            this.CreateInvalidToken();
        }

        /// <summary>
        /// Reads the end of an array.
        /// </summary>
        /// <param name="payloadPointer">A pointer to the payload.</param>
        /// <remarks>
        /// This method uses character pointer arithmetic to iterate over the string because
        /// it is significantly faster than using the string indexer.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        private unsafe void ReadEndArray(char* payloadPointer)
        {
            Container current = this.scope.Pop();
            int count = this.position - current.Offset + 1;
            this.position = current.Offset;
            this.CreateToken(count, JsonTokenType.EndArray);
            this.close = false;
            this.PrepareForClose(payloadPointer, true);
        }

        /// <summary>
        /// Reads the end of an object.
        /// </summary>
        /// <param name="payloadPointer">A pointer to the payload.</param>
        /// <remarks>
        /// This method uses character pointer arithmetic to iterate over the string because
        /// it is significantly faster than using the string indexer.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        private unsafe void ReadEndObject(char* payloadPointer)
        {
            Container current = this.scope.Pop();
            int count = this.position - current.Offset + 1;
            this.position = current.Offset;
            this.CreateToken(count, JsonTokenType.EndObject);
            this.close = false;
            this.PrepareForClose(payloadPointer, true);
        }

        /// <summary>
        /// Reads the end of a property.
        /// </summary>
        /// <param name="payloadPointer">A pointer to the payload.</param>
        /// <remarks>
        /// This method uses character pointer arithmetic to iterate over the string because
        /// it is significantly faster than using the string indexer.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        private unsafe void ReadEndProperty(char* payloadPointer)
        {
            Container current = this.scope.Pop();
            int count = this.TokenSegment.Offset + this.TokenSegment.Count - current.Offset;
            this.position = current.Offset;
            this.CreateToken(count, JsonTokenType.EndProperty);
            this.close = false;
            this.PrepareForClose(payloadPointer, true);
        }

        /// <summary>
        /// Reads the exponent part of a floating point number.
        /// </summary>
        /// <param name="payloadPointer">A pointer to the payload.</param>
        /// <param name="cursor">The starting point of the exponent.</param>
        /// <remarks>
        /// This method uses character pointer arithmetic to iterate over the string because
        /// it is significantly faster than using the string indexer.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        private unsafe void ReadExponent(char* payloadPointer, int cursor)
        {
            char c = *(payloadPointer + cursor);
            if (c == JsonConstants.PositiveSign || c == JsonConstants.NegativeSign)
            {
                cursor++;
            }

            int start = cursor;
            cursor = this.EndOfDigits(payloadPointer, cursor);
            if (start == cursor)
            {
                this.CreateInvalidToken();
                return;
            }

            this.CreateToken(cursor - this.position, JsonTokenType.Float);
        }

        /// <summary>
        /// Reads a false literal.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        private void ReadFalse()
        {
            if (string.CompareOrdinal(this.payload, this.position, JsonConstants.FalseValue, 0, JsonConstants.FalseValueLength) == 0)
            {
                this.CreateToken(JsonConstants.FalseValueLength, JsonTokenType.Boolean);
                this.truth = false;
            }
            else
            {
                this.CreateInvalidToken();
            }
        }

        /// <summary>
        /// Reads the fractional part of a floating point number.
        /// </summary>
        /// <param name="payloadPointer">A pointer to the payload.</param>
        /// <param name="cursor">The starting point of the fraction.</param>
        /// <remarks>
        /// This method uses character pointer arithmetic to iterate over the string because
        /// it is significantly faster than using the string indexer.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        private unsafe void ReadFraction(char* payloadPointer, int cursor)
        {
            int start = cursor;
            cursor = this.EndOfDigits(payloadPointer, cursor);
            if (start == cursor)
            {
                this.CreateInvalidToken();
                return;
            }

            char c = *(payloadPointer + cursor);
            if (c == JsonConstants.ExponentLowercase || c == JsonConstants.ExponentUppercase)
            {
                this.ReadExponent(payloadPointer, cursor + 1);
                return;
            }

            this.CreateToken(cursor - this.position, JsonTokenType.Float);
        }

        /// <summary>
        /// Reads a hexadecimal number.
        /// </summary>
        /// <param name="payloadPointer">A pointer to the payload.</param>
        /// <param name="cursor">The starting point of the hexadecimal digits.</param>
        private unsafe void ReadHexNumber(char* payloadPointer, int cursor)
        {
            int start = cursor;
            cursor = this.EndOfHexDigits(payloadPointer, cursor);
            if (start == cursor)
            {
                this.CreateInvalidToken();
            }

            this.CreateToken(cursor - this.position, JsonTokenType.Integer);
        }

        /// <summary>
        /// Reads a null literal.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        private void ReadNull()
        {
            if (string.CompareOrdinal(this.payload, this.position, JsonConstants.NullValue, 0, JsonConstants.NullValueLength) == 0)
            {
                this.CreateToken(JsonConstants.NullValueLength, JsonTokenType.Null);
            }
            else
            {
                this.CreateInvalidToken();
            }
        }

        /// <summary>
        /// Reads a number.
        /// </summary>
        /// <param name="payloadPointer">A pointer to the payload.</param>
        /// <remarks>
        /// This method uses character pointer arithmetic to iterate over the string because
        /// it is significantly faster than using the string indexer.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        private unsafe void ReadNumber(char* payloadPointer)
        {
            int cursor = this.position;
            char c = *(payloadPointer + cursor);
            if (c == JsonConstants.NegativeSign)
            {
                cursor++;
            }
            else if (c == JsonConstants.Zero)
            {
                int hexOffset = cursor + 1;
                if (hexOffset < this.payloadLength)
                {
                    c = *(payloadPointer + hexOffset);
                    if (c == JsonConstants.HexLowercase || c == JsonConstants.HexUppercase)
                    {
                        this.ReadHexNumber(payloadPointer, hexOffset + 1);
                        return;
                    }
                }
            }

            int start = cursor;
            cursor = this.EndOfDigits(payloadPointer, cursor);
            if (start == cursor)
            {
                this.CreateInvalidToken();
                return;
            }

            c = *(payloadPointer + cursor);
            if (c == JsonConstants.DecimalPoint)
            {
                this.ReadFraction(payloadPointer, cursor + 1);
                return;
            }

            if (c == JsonConstants.ExponentLowercase || c == JsonConstants.ExponentUppercase)
            {
                this.ReadExponent(payloadPointer, cursor + 1);
                return;
            }

            this.CreateToken(cursor - this.position, JsonTokenType.Integer);
        }

        /// <summary>
        /// Reads a string.
        /// </summary>
        /// <param name="payloadPointer">A pointer to the payload.</param>
        /// <remarks>
        /// This method uses character pointer arithmetic to iterate over the string because
        /// it is significantly faster than using the string indexer.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        private unsafe void ReadString(char* payloadPointer)
        {
            int cursor = this.EndOfString(payloadPointer);
            if (cursor > 0)
            {
                this.CreateToken(cursor - this.position, JsonTokenType.String);
            }
            else
            {
                this.CreateInvalidToken();
            }
        }

        /// <summary>
        /// Reads any token.
        /// </summary>
        /// <param name="payloadPointer">A pointer to the payload.</param>
        /// <remarks>
        /// This method uses character pointer arithmetic to iterate over the string because
        /// it is significantly faster than using the string indexer.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        private unsafe void ReadToken(char* payloadPointer)
        {
            if (this.close)
            {
                this.CreateCompleteToken();
                return;
            }

            char c = *(payloadPointer + this.position);
            switch (c)
            {
                case JsonConstants.NullLeadCharacter:
                    this.ReadNull();
                    break;
                case JsonConstants.FalseLeadCharacter:
                    this.ReadFalse();
                    break;
                case JsonConstants.TrueLeadCharacter:
                    this.ReadTrue();
                    break;
                case JsonConstants.ArrayHeader:
                    this.ReadBeginArray();
                    break;
                case JsonConstants.ObjectHeader:
                    this.ReadBeginObject();
                    break;
                case JsonConstants.StringEnclosure:
                    this.ReadString(payloadPointer);
                    break;
                default:
                    this.ReadNumber(payloadPointer);
                    break;
            }

            if (this.TokenType != JsonTokenType.Invalid)
            {
                this.PrepareForClose(payloadPointer, false);
            }
        }

        /// <summary>
        /// Reads a true literal.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        private void ReadTrue()
        {
            if (string.CompareOrdinal(this.payload, this.position, JsonConstants.TrueValue, 0, JsonConstants.TrueValueLength) == 0)
            {
                this.CreateToken(JsonConstants.TrueValueLength, JsonTokenType.Boolean);
                this.truth = true;
            }
            else
            {
                this.CreateInvalidToken();
            }
        }

        /// <summary>
        /// Tries to parse the current token.
        /// </summary>
        /// <param name="value">The managed value of the token.</param>
        /// <returns>
        /// A value indicating whether or not the token could be parsed. Failure to parse
        /// indicates either malformed JSON or that the token cannot be converted to the
        /// value type.
        /// </returns>
        private unsafe bool TryParseTokenUnsafe(out long value)
        {
            value = default(long);
            long accumulator = 0L;
            int first = this.TokenSegment.Offset;
            int last = first + this.TokenSegment.Count;
            byte digit = default(byte);
            fixed (char* payloadPointer = this.TokenSegment.String)
            {
                char* cp = payloadPointer + first;
                if (*cp == JsonConstants.NegativeSign)
                {
                    for (int i = first + 1; i < last; i++)
                    {
                        cp = payloadPointer + i;
                        if (JsonParser.TryConvertDecimal(*cp, out digit))
                        {
                            try
                            {
                                accumulator = checked((accumulator * JsonConstants.DecimalRadix) - digit);
                            }
                            catch (OverflowException)
                            {
                                return false;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    for (int i = first; i < last; i++)
                    {
                        cp = payloadPointer + i;
                        if (JsonParser.TryConvertDecimal(*cp, out digit))
                        {
                            try
                            {
                                accumulator = checked((accumulator * JsonConstants.DecimalRadix) + digit);
                            }
                            catch (OverflowException)
                            {
                                return false;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }

            value = accumulator;
            return true;
        }

        /// <summary>
        /// Tries to parse the current token.
        /// </summary>
        /// <param name="value">The managed value of the token.</param>
        /// <returns>
        /// A value indicating whether or not the token could be parsed. Failure to parse
        /// indicates either malformed JSON or that the token cannot be converted to the
        /// value type.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        private unsafe bool TryParseTokenUnsafe(out ulong value)
        {
            value = default(ulong);
            ulong accumulator = 0UL;
            int first = this.TokenSegment.Offset;
            int last = first + this.TokenSegment.Count;
            byte digit = default(byte);
            bool isHex = false;
            fixed (char* payloadPointer = this.TokenSegment.String)
            {
                char* cp = payloadPointer + first;
                if (first + 2 < last && *cp == JsonConstants.Zero)
                {
                    first++;
                    cp = payloadPointer + first;
                    isHex = *cp == JsonConstants.HexLowercase || *cp == JsonConstants.HexUppercase;
                }

                if (isHex)
                {
                    first++;
                    for (int i = first; i < last; i++)
                    {
                        cp = payloadPointer + i;
                        if (JsonParser.TryConvertHex(*cp, out digit))
                        {
                            try
                            {
                                accumulator = checked((accumulator * JsonConstants.HexRadix) + digit);
                            }
                            catch (OverflowException)
                            {
                                return false;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    for (int i = first; i < last; i++)
                    {
                        cp = payloadPointer + i;
                        if (JsonParser.TryConvertDecimal(*cp, out digit))
                        {
                            try
                            {
                                accumulator = checked((accumulator * JsonConstants.DecimalRadix) + digit);
                            }
                            catch (OverflowException)
                            {
                                return false;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }

            value = accumulator;
            return true;
        }

        /// <summary>
        /// Tries to parse the current token.
        /// </summary>
        /// <param name="subsegment">
        /// The string body without the enclosing quotation marks.
        /// </param>
        /// <param name="value">The managed value of the token.</param>
        /// <returns>
        /// A value indicating whether or not the token could be parsed. Failure to parse
        /// indicates either malformed JSON or that the token cannot be converted to the
        /// value type.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        private unsafe bool TryParseTokenUnsafe(StringSegment subsegment, out string value)
        {
            value = null;
            int endOfSegment = subsegment.Offset + subsegment.Count;
            int cursor = subsegment.Offset;
            this.decodeBuffer.Clear();
            fixed (char* payloadPointer = subsegment.String)
            {
                while (cursor < endOfSegment)
                {
                    char c = *(payloadPointer + cursor);
                    if (c == JsonConstants.StringEnclosure)
                    {
                        break;
                    }

                    if (c != JsonConstants.CharacterEscape)
                    {
                        if (this.decodeBuffer.TryAppend(c, 0))
                        {
                            cursor++;
                            continue;
                        }

                        return false;
                    }

                    if (++cursor >= endOfSegment)
                    {
                        return false;
                    }

                    c = *(payloadPointer + cursor);
                    int asIndex = c;
                    if (asIndex > JsonConstants.UnescapeSequencesLength)
                    {
                        return false;
                    }

                    if (asIndex == JsonConstants.UnescapeSequencesLength)
                    {
                        int start = cursor + 1;
                        int jump = start + 4;
                        if (jump > endOfSegment)
                        {
                            return false;
                        }

                        ushort code = 0;
                        if (JsonParser.TryParseHex(payloadPointer, start, jump, out code))
                        {
                            if (this.decodeBuffer.TryAppend((char)code, 0))
                            {
                                cursor = jump;
                                continue;
                            }

                            return false;
                        }

                        return false;
                    }

                    char u = JsonConstants.UnescapeSequences[asIndex];
                    if (this.decodeBuffer.TryAppend(u, 0))
                    {
                        cursor++;
                        continue;
                    }

                    return false;
                }

                value = this.decodeBuffer.ToString();
            }

            return true;
        }

        /// <summary>
        /// Parsing container scope state.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1812:AvoidUninstantiatedInternalClasses",
            Justification = "You don't have to use everything in an in-line code share.")]
        private struct Container
        {
            /// <summary>
            /// The root container.
            /// </summary>
            public static readonly Container Root = new Container(JsonTokenType.None, 0);

            /// <summary>
            /// The offset of the beginning of the container in the payload.
            /// </summary>
            public readonly int Offset;

            /// <summary>
            /// The container token type.
            /// </summary>
            public readonly JsonTokenType TokenType;

            /// <summary>
            /// Initializes a new instance of the <see cref="Container"/> struct.
            /// </summary>
            /// <param name="type">The container token type.</param>
            /// <param name="offset">
            /// The offset of the beginning of the container in the payload.
            /// </param>
            [System.Diagnostics.CodeAnalysis.SuppressMessage(
                "Microsoft.Performance",
                "CA1811:AvoidUncalledPrivateCode",
                Justification = "You don't have to use everything in an in-line code share.")]
            public Container(JsonTokenType type, int offset)
            {
                this.TokenType = type;
                this.Offset = offset;
            }
        }
    }
}
