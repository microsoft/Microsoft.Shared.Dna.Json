//-----------------------------------------------------------------------------
// <copyright file="JsonBuilder.cs" company="Microsoft">
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
    /// Builds fixed-capacity JSON strings, truncating the payload when necessary.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Microsoft.Performance",
        "CA1812:AvoidUninstantiatedInternalClasses",
        Justification = "You don't have to use everything in an in-line code share.")]
    internal sealed class JsonBuilder
    {
        /// <summary>
        /// JSON string buffer.
        /// </summary>
        private FixedStringBuilder builder = null;

        /// <summary>
        /// JSON token scope.
        /// </summary>
        private Stack<JsonTokenType> scope = null;

        /// <summary>
        /// Whether or not the output has been truncated.
        /// </summary>
        private bool truncated = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonBuilder"/> class.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        public JsonBuilder()
            : this(JsonConstants.DefaultCapacity, JsonConstants.DefaultDepth)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonBuilder"/> class.
        /// </summary>
        /// <param name="capacity">
        /// The maximum number of characters the JSON string can be.
        /// </param>
        /// <param name="depth">
        /// The initial depth of the token scope stack. The stack will grow depending on how
        /// deeply nested the object is.
        /// </param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        public JsonBuilder(int capacity, int depth)
        {
#if CONTRACTS_FULL // Work around the implicit rewrite requirements of Contract.Requires<T>
            Contract.Requires<ArgumentOutOfRangeException>(depth > 0);
#endif
            if (capacity < JsonConstants.TruncatedObjectLength)
            {
                capacity = JsonConstants.TruncatedObjectLength;
            }

            this.builder = new FixedStringBuilder(capacity);
            this.scope = new Stack<JsonTokenType>(depth);
            this.scope.Push(JsonTokenType.None);
        }

        /// <summary>
        /// Gets the current token scope.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        private JsonTokenType Current
        {
            get
            {
                return this.scope.Peek();
            }
        }

        /// <summary>
        /// Gets the number of reserve characters required to write the truncate flag and
        /// close any open tokens.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        private int Reserve
        {
            get
            {
                return this.scope.Count + JsonConstants.TruncatedObjectLength;
            }
        }

        /// <summary>
        /// Clears the content of the builder. The instance can be reused to build a new
        /// string after this method returns.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        public void Clear()
        {
            this.builder.Clear();
            this.scope.Clear();
            this.scope.Push(JsonTokenType.None);
            this.truncated = false;
        }

        /// <summary>
        /// Closes the current token, writing null to an unassigned property and doing
        /// nothing if there are no tokens to close.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        public void CloseToken()
        {
            if (this.Current == JsonTokenType.None)
            {
                return;
            }

            JsonTokenType container = this.scope.Pop();
            switch (container)
            {
                case JsonTokenType.BeginArray:
                    this.builder.TryAppend(JsonConstants.ArrayFooter, 0);
                    break;
                case JsonTokenType.BeginObject:
                    this.builder.TryAppend(JsonConstants.ObjectFooter, 0);
                    break;
                case JsonTokenType.BeginProperty:
                    if (this.builder.Last == JsonConstants.NameValueSeparator)
                    {
                        this.builder.TryAppend(JsonConstants.NullValue, 0);
                    }

                    break;
            }
        }

        /// <summary>
        /// Closes any open tokens and builds the final JSON string.
        /// </summary>
        /// <returns>The completed JSON string.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        public string Finish()
        {
            while (this.Current != JsonTokenType.None)
            {
                this.CloseToken();
            }

            return this.builder.ToString();
        }

        /// <summary>
        /// Opens an array token.
        /// </summary>
        /// <returns>
        /// A value indicating whether the array was opened or the payload truncated.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        public bool OpenArray()
        {
            int reserve = this.Reserve;
            int rollback = 0;
            if (this.PrepareContainer(reserve, out rollback))
            {
                if (this.builder.TryAppend(JsonConstants.ArrayHeader, reserve, rollback))
                {
                    this.scope.Push(JsonTokenType.BeginArray);
                    return true;
                }
            }

            this.Truncate();
            return false;
        }

        /// <summary>
        /// Opens an object token.
        /// </summary>
        /// <returns>
        /// A value indicating whether the object was opened or the payload truncated.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        public bool OpenObject()
        {
            int reserve = this.Reserve;
            int rollback = 0;
            if (this.PrepareContainer(reserve, out rollback))
            {
                if (this.builder.TryAppend(JsonConstants.ObjectHeader, reserve, rollback))
                {
                    this.scope.Push(JsonTokenType.BeginObject);
                    return true;
                }
            }

            this.Truncate();
            return false;
        }

        /// <summary>
        /// Opens a property token.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <returns>
        /// A value indicating whether the property was opened or the payload truncated.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        public bool OpenProperty(string name)
        {
#if CONTRACTS_FULL // Work around the implicit rewrite requirements of Contract.Requires<T>
            Contract.Requires<ArgumentNullException>(name != null);
#endif
            int reserve = this.Reserve;
            int rollback = 0;
            if (this.PrepareProperty(reserve, out rollback))
            {
                if (this.TryEncode(name, reserve, rollback))
                {
                    if (this.builder.TryAppend(JsonConstants.NameValueSeparator, reserve, rollback))
                    {
                        this.scope.Push(JsonTokenType.BeginProperty);
                        return true;
                    }
                }
            }

            this.Truncate();
            return false;
        }

        /// <summary>
        /// Tries to change the capacity of the JSON builder.
        /// </summary>
        /// <param name="capacity">
        /// The new maximum number of characters that the builder can create a string for.
        /// </param>
        /// <returns>
        /// A value indicating whether the builder was resized or the current content and
        /// reserve exceeded the new capacity.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        public bool TryResize(int capacity)
        {
            if (capacity < this.Reserve)
            {
                return false;
            }

            return this.builder.TryResize(capacity, this.Reserve);
        }

        /// <summary>
        /// Writes a value to the payload.
        /// </summary>
        /// <param name="value">The value to write.</param>
        /// <returns>
        /// A value indicating whether the value was written or the payload truncated.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        public bool WriteValue(bool value)
        {
            int reserve = this.Reserve;
            int rollback = 0;
            if (this.PrepareValue(reserve, out rollback))
            {
                if (this.TryEncode(value, reserve, rollback))
                {
                    return true;
                }
            }

            this.Truncate();
            return false;
        }

        /// <summary>
        /// Writes a value to the payload.
        /// </summary>
        /// <param name="value">The value to write.</param>
        /// <returns>
        /// A value indicating whether the value was written or the payload truncated.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        public bool WriteValue(byte value)
        {
            int reserve = this.Reserve;
            int rollback = 0;
            if (this.PrepareValue(reserve, out rollback))
            {
                if (this.TryEncode(value, reserve, rollback))
                {
                    return true;
                }
            }

            this.Truncate();
            return false;
        }

        /// <summary>
        /// Writes a value to the payload.
        /// </summary>
        /// <param name="value">The value to write.</param>
        /// <returns>
        /// A value indicating whether the value was written or the payload truncated.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        public bool WriteValue(sbyte value)
        {
            int reserve = this.Reserve;
            int rollback = 0;
            if (this.PrepareValue(reserve, out rollback))
            {
                if (this.TryEncode(value, reserve, rollback))
                {
                    return true;
                }
            }

            this.Truncate();
            return false;
        }

        /// <summary>
        /// Writes a value to the payload.
        /// </summary>
        /// <param name="value">The value to write.</param>
        /// <returns>
        /// A value indicating whether the value was written or the payload truncated.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        public bool WriteValue(short value)
        {
            int reserve = this.Reserve;
            int rollback = 0;
            if (this.PrepareValue(reserve, out rollback))
            {
                if (this.TryEncode(value, reserve, rollback))
                {
                    return true;
                }
            }

            this.Truncate();
            return false;
        }

        /// <summary>
        /// Writes a value to the payload.
        /// </summary>
        /// <param name="value">The value to write.</param>
        /// <returns>
        /// A value indicating whether the value was written or the payload truncated.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        public bool WriteValue(ushort value)
        {
            int reserve = this.Reserve;
            int rollback = 0;
            if (this.PrepareValue(reserve, out rollback))
            {
                if (this.TryEncode(value, reserve, rollback))
                {
                    return true;
                }
            }

            this.Truncate();
            return false;
        }

        /// <summary>
        /// Writes a value to the payload.
        /// </summary>
        /// <param name="value">The value to write.</param>
        /// <returns>
        /// A value indicating whether the value was written or the payload truncated.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        public bool WriteValue(int value)
        {
            int reserve = this.Reserve;
            int rollback = 0;
            if (this.PrepareValue(reserve, out rollback))
            {
                if (this.TryEncode(value, reserve, rollback))
                {
                    return true;
                }
            }

            this.Truncate();
            return false;
        }

        /// <summary>
        /// Writes a value to the payload.
        /// </summary>
        /// <param name="value">The value to write.</param>
        /// <returns>
        /// A value indicating whether the value was written or the payload truncated.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        public bool WriteValue(uint value)
        {
            int reserve = this.Reserve;
            int rollback = 0;
            if (this.PrepareValue(reserve, out rollback))
            {
                if (this.TryEncode(value, reserve, rollback))
                {
                    return true;
                }
            }

            this.Truncate();
            return false;
        }

        /// <summary>
        /// Writes a value to the payload.
        /// </summary>
        /// <param name="value">The value to write.</param>
        /// <returns>
        /// A value indicating whether the value was written or the payload truncated.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        public bool WriteValue(long value)
        {
            int reserve = this.Reserve;
            int rollback = 0;
            if (this.PrepareValue(reserve, out rollback))
            {
                if (this.TryEncode(value, reserve, rollback))
                {
                    return true;
                }
            }

            this.Truncate();
            return false;
        }

        /// <summary>
        /// Writes a value to the payload.
        /// </summary>
        /// <param name="value">The value to write.</param>
        /// <returns>
        /// A value indicating whether the value was written or the payload truncated.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        public bool WriteValue(ulong value)
        {
            int reserve = this.Reserve;
            int rollback = 0;
            if (this.PrepareValue(reserve, out rollback))
            {
                if (this.TryEncode(value, reserve, rollback))
                {
                    return true;
                }
            }

            this.Truncate();
            return false;
        }

        /// <summary>
        /// Writes a value to the payload.
        /// </summary>
        /// <param name="value">The value to write.</param>
        /// <returns>
        /// A value indicating whether the value was written or the payload truncated.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        public bool WriteValue(float value)
        {
            int reserve = this.Reserve;
            int rollback = 0;
            if (this.PrepareValue(reserve, out rollback))
            {
                if (this.TryEncode(value, reserve, rollback))
                {
                    return true;
                }
            }

            this.Truncate();
            return false;
        }

        /// <summary>
        /// Writes a value to the payload.
        /// </summary>
        /// <param name="value">The value to write.</param>
        /// <returns>
        /// A value indicating whether the value was written or the payload truncated.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        public bool WriteValue(double value)
        {
            int reserve = this.Reserve;
            int rollback = 0;
            if (this.PrepareValue(reserve, out rollback))
            {
                if (this.TryEncode(value, reserve, rollback))
                {
                    return true;
                }
            }

            this.Truncate();
            return false;
        }

        /// <summary>
        /// Writes a value to the payload.
        /// </summary>
        /// <param name="value">The value to write.</param>
        /// <returns>
        /// A value indicating whether the value was written or the payload truncated.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        public bool WriteValue(decimal value)
        {
            int reserve = this.Reserve;
            int rollback = 0;
            if (this.PrepareValue(reserve, out rollback))
            {
                if (this.TryEncode(value, reserve, rollback))
                {
                    return true;
                }
            }

            this.Truncate();
            return false;
        }

        /// <summary>
        /// Writes a value to the payload.
        /// </summary>
        /// <param name="value">The value to write.</param>
        /// <returns>
        /// A value indicating whether the value was written or the payload truncated.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        public bool WriteValue(string value)
        {
            int reserve = this.Reserve;
            int rollback = 0;
            if (this.PrepareValue(reserve, out rollback))
            {
                if (this.TryEncode(value, reserve, rollback))
                {
                    return true;
                }
            }

            this.Truncate();
            return false;
        }

        /// <summary>
        /// Prepares the current token for a container write.
        /// </summary>
        /// <param name="reserve">
        /// The amount of capacity that must remain in the builder after the token has been
        /// prepared.
        /// </param>
        /// <param name="rollback">
        /// The length of the builder before the token was prepared.
        /// </param>
        /// <returns>
        /// A value indicating whether the token was prepared or the payload should be
        /// truncated.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        private bool PrepareContainer(int reserve, out int rollback)
        {
            rollback = this.builder.Length;
            if (this.truncated)
            {
                return false;
            }

            switch (this.Current)
            {
                case JsonTokenType.None:
                    return this.builder.Length == 0;
                case JsonTokenType.BeginArray:
                    if (this.builder.Last != JsonConstants.ArrayHeader)
                    {
                        return this.builder.TryAppend(JsonConstants.ElementSeparator, reserve, out rollback);
                    }

                    return true;
                case JsonTokenType.BeginObject:
                    return false;
                case JsonTokenType.BeginProperty:
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Prepares the current token for a property write.
        /// </summary>
        /// <param name="reserve">
        /// The amount of capacity that must remain in the builder after the token has been
        /// prepared.
        /// </param>
        /// <param name="rollback">
        /// The length of the builder before the token was prepared.
        /// </param>
        /// <returns>
        /// A value indicating whether the token was prepared or the payload should be
        /// truncated.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        private bool PrepareProperty(int reserve, out int rollback)
        {
            rollback = this.builder.Length;
            if (this.truncated)
            {
                return false;
            }

            if (this.Current == JsonTokenType.BeginObject)
            {
                if (this.builder.Last != JsonConstants.ObjectHeader)
                {
                    return this.builder.TryAppend(JsonConstants.ElementSeparator, reserve, out rollback);
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Prepares the current token for a value write.
        /// </summary>
        /// <param name="reserve">
        /// The amount of capacity that must remain in the builder after the token has been
        /// prepared.
        /// </param>
        /// <param name="rollback">
        /// The length of the builder before the token was prepared.
        /// </param>
        /// <returns>
        /// A value indicating whether the token was prepared or the payload should be
        /// truncated.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        private bool PrepareValue(int reserve, out int rollback)
        {
            rollback = this.builder.Length;
            if (this.truncated)
            {
                return false;
            }

            switch (this.Current)
            {
                case JsonTokenType.None:
                    return this.builder.Length == 0;
                case JsonTokenType.BeginArray:
                    if (this.builder.Last != JsonConstants.ArrayHeader)
                    {
                        return this.builder.TryAppend(JsonConstants.ElementSeparator, reserve, out rollback);
                    }

                    return true;
                case JsonTokenType.BeginObject:
                    return false;
                case JsonTokenType.BeginProperty:
                    return this.builder.Last == JsonConstants.NameValueSeparator;
            }

            return false;
        }

        /// <summary>
        /// Writes the truncate flag to the current token and blocks any further updates
        /// apart from closing and finishing.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        private void Truncate()
        {
            if (this.truncated)
            {
                return;
            }

            switch (this.Current)
            {
                case JsonTokenType.None:
                    this.builder.TryAppend(JsonConstants.TruncatedObject, 0);
                    break;
                case JsonTokenType.BeginArray:
                    if (this.builder.Last != JsonConstants.ArrayHeader)
                    {
                        this.builder.TryAppend(JsonConstants.ElementSeparator, 0);
                    }

                    this.builder.TryAppend(JsonConstants.TruncatedObject, 0);
                    break;
                case JsonTokenType.BeginObject:
                    if (this.builder.Last != JsonConstants.ObjectHeader)
                    {
                        this.builder.TryAppend(JsonConstants.ElementSeparator, 0);
                    }

                    this.builder.TryAppend(JsonConstants.TruncatedProperty, 0);
                    break;
                case JsonTokenType.BeginProperty:
                    if (this.builder.Last == JsonConstants.NameValueSeparator)
                    {
                        this.builder.TryAppend(JsonConstants.TruncatedObject, 0);
                    }
                    else
                    {
                        this.builder.TryAppend(JsonConstants.ElementSeparator, 0);
                        this.builder.TryAppend(JsonConstants.TruncatedProperty, 0);
                    }

                    break;
            }

            this.truncated = true;
        }

        /// <summary>
        /// Tries to encode and write the value into the payload.
        /// </summary>
        /// <param name="value">The value to encode.</param>
        /// <param name="reserve">
        /// The amount of capacity that must remain in the builder after the value has been
        /// added.
        /// </param>
        /// <param name="rollback">
        /// The length to set the builder to if the encoding failed.
        /// </param>
        /// <returns>
        /// A value indicating whether the value was added or the payload should be
        /// truncated.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        private bool TryEncode(bool value, int reserve, int rollback)
        {
            if (value)
            {
                return this.builder.TryAppend(JsonConstants.TrueValue, reserve, rollback);
            }

            return this.builder.TryAppend(JsonConstants.FalseValue, reserve, rollback);
        }

        /// <summary>
        /// Tries to encode and write the value into the payload.
        /// </summary>
        /// <param name="value">The value to encode.</param>
        /// <param name="reserve">
        /// The amount of capacity that must remain in the builder after the value has been
        /// added.
        /// </param>
        /// <param name="rollback">
        /// The length to set the builder to if the encoding failed.
        /// </param>
        /// <returns>
        /// A value indicating whether the value was added or the payload should be
        /// truncated.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        private bool TryEncode(byte value, int reserve, int rollback)
        {
            return this.builder.TryAppend(value.ToString(CultureInfo.InvariantCulture), reserve, rollback);
        }

        /// <summary>
        /// Tries to encode and write the value into the payload.
        /// </summary>
        /// <param name="value">The value to encode.</param>
        /// <param name="reserve">
        /// The amount of capacity that must remain in the builder after the value has been
        /// added.
        /// </param>
        /// <param name="rollback">
        /// The length to set the builder to if the encoding failed.
        /// </param>
        /// <returns>
        /// A value indicating whether the value was added or the payload should be
        /// truncated.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        private bool TryEncode(sbyte value, int reserve, int rollback)
        {
            return this.builder.TryAppend(value.ToString(CultureInfo.InvariantCulture), reserve, rollback);
        }

        /// <summary>
        /// Tries to encode and write the value into the payload.
        /// </summary>
        /// <param name="value">The value to encode.</param>
        /// <param name="reserve">
        /// The amount of capacity that must remain in the builder after the value has been
        /// added.
        /// </param>
        /// <param name="rollback">
        /// The length to set the builder to if the encoding failed.
        /// </param>
        /// <returns>
        /// A value indicating whether the value was added or the payload should be
        /// truncated.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        private bool TryEncode(short value, int reserve, int rollback)
        {
            return this.builder.TryAppend(value.ToString(CultureInfo.InvariantCulture), reserve, rollback);
        }

        /// <summary>
        /// Tries to encode and write the value into the payload.
        /// </summary>
        /// <param name="value">The value to encode.</param>
        /// <param name="reserve">
        /// The amount of capacity that must remain in the builder after the value has been
        /// added.
        /// </param>
        /// <param name="rollback">
        /// The length to set the builder to if the encoding failed.
        /// </param>
        /// <returns>
        /// A value indicating whether the value was added or the payload should be
        /// truncated.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        private bool TryEncode(ushort value, int reserve, int rollback)
        {
            return this.builder.TryAppend(value.ToString(CultureInfo.InvariantCulture), reserve, rollback);
        }

        /// <summary>
        /// Tries to encode and write the value into the payload.
        /// </summary>
        /// <param name="value">The value to encode.</param>
        /// <param name="reserve">
        /// The amount of capacity that must remain in the builder after the value has been
        /// added.
        /// </param>
        /// <param name="rollback">
        /// The length to set the builder to if the encoding failed.
        /// </param>
        /// <returns>
        /// A value indicating whether the value was added or the payload should be
        /// truncated.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        private bool TryEncode(int value, int reserve, int rollback)
        {
            return this.builder.TryAppend(value.ToString(CultureInfo.InvariantCulture), reserve, rollback);
        }

        /// <summary>
        /// Tries to encode and write the value into the payload.
        /// </summary>
        /// <param name="value">The value to encode.</param>
        /// <param name="reserve">
        /// The amount of capacity that must remain in the builder after the value has been
        /// added.
        /// </param>
        /// <param name="rollback">
        /// The length to set the builder to if the encoding failed.
        /// </param>
        /// <returns>
        /// A value indicating whether the value was added or the payload should be
        /// truncated.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        private bool TryEncode(uint value, int reserve, int rollback)
        {
            return this.builder.TryAppend(value.ToString(CultureInfo.InvariantCulture), reserve, rollback);
        }

        /// <summary>
        /// Tries to encode and write the value into the payload.
        /// </summary>
        /// <param name="value">The value to encode.</param>
        /// <param name="reserve">
        /// The amount of capacity that must remain in the builder after the value has been
        /// added.
        /// </param>
        /// <param name="rollback">
        /// The length to set the builder to if the encoding failed.
        /// </param>
        /// <returns>
        /// A value indicating whether the value was added or the payload should be
        /// truncated.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        private bool TryEncode(long value, int reserve, int rollback)
        {
            return this.builder.TryAppend(value.ToString(CultureInfo.InvariantCulture), reserve, rollback);
        }

        /// <summary>
        /// Tries to encode and write the value into the payload.
        /// </summary>
        /// <param name="value">The value to encode.</param>
        /// <param name="reserve">
        /// The amount of capacity that must remain in the builder after the value has been
        /// added.
        /// </param>
        /// <param name="rollback">
        /// The length to set the builder to if the encoding failed.
        /// </param>
        /// <returns>
        /// A value indicating whether the value was added or the payload should be
        /// truncated.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        private bool TryEncode(ulong value, int reserve, int rollback)
        {
            return this.builder.TryAppend(value.ToString(CultureInfo.InvariantCulture), reserve, rollback);
        }

        /// <summary>
        /// Tries to encode and write the value into the payload.
        /// </summary>
        /// <param name="value">The value to encode.</param>
        /// <param name="reserve">
        /// The amount of capacity that must remain in the builder after the value has been
        /// added.
        /// </param>
        /// <param name="rollback">
        /// The length to set the builder to if the encoding failed.
        /// </param>
        /// <returns>
        /// A value indicating whether the value was added or the payload should be
        /// truncated.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        private bool TryEncode(float value, int reserve, int rollback)
        {
            return this.builder.TryAppend(value.ToString("R", CultureInfo.InvariantCulture), reserve, rollback);
        }

        /// <summary>
        /// Tries to encode and write the value into the payload.
        /// </summary>
        /// <param name="value">The value to encode.</param>
        /// <param name="reserve">
        /// The amount of capacity that must remain in the builder after the value has been
        /// added.
        /// </param>
        /// <param name="rollback">
        /// The length to set the builder to if the encoding failed.
        /// </param>
        /// <returns>
        /// A value indicating whether the value was added or the payload should be
        /// truncated.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        private bool TryEncode(double value, int reserve, int rollback)
        {
            return this.builder.TryAppend(value.ToString("R", CultureInfo.InvariantCulture), reserve, rollback);
        }

        /// <summary>
        /// Tries to encode and write the value into the payload.
        /// </summary>
        /// <param name="value">The value to encode.</param>
        /// <param name="reserve">
        /// The amount of capacity that must remain in the builder after the value has been
        /// added.
        /// </param>
        /// <param name="rollback">
        /// The length to set the builder to if the encoding failed.
        /// </param>
        /// <returns>
        /// A value indicating whether the value was added or the payload should be
        /// truncated.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        private bool TryEncode(decimal value, int reserve, int rollback)
        {
            return this.builder.TryAppend(value.ToString(CultureInfo.InvariantCulture), reserve, rollback);
        }

        /// <summary>
        /// Tries to encode and write the value into the payload.
        /// </summary>
        /// <param name="value">The value to encode.</param>
        /// <param name="reserve">
        /// The amount of capacity that must remain in the builder after the value has been
        /// added.
        /// </param>
        /// <param name="rollback">
        /// The length to set the builder to if the encoding failed.
        /// </param>
        /// <returns>
        /// A value indicating whether the value was added or the payload should be
        /// truncated.
        /// </returns>
        /// <remarks>
        /// This method uses character pointer arithmetic to iterate over the string because
        /// it is significantly faster than using the string indexer.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        private unsafe bool TryEncode(string value, int reserve, int rollback)
        {
            if (value == null)
            {
                return this.builder.TryAppend(JsonConstants.NullValue, reserve, rollback);
            }

            if (!this.builder.TryAppend(JsonConstants.StringEnclosure, reserve, rollback))
            {
                return false;
            }

            fixed (char* valuePointer = value)
            {
                int valueLength = value.Length;
                for (int i = 0; i < valueLength; i++)
                {
                    char* c = valuePointer + i;
                    string escaped = null;
                    int asIndex = *c;
                    if (asIndex < JsonConstants.EscapeSequencesLength)
                    {
                        escaped = JsonConstants.EscapeSequences[asIndex];
                    }

                    if (escaped == null)
                    {
                        if (this.builder.TryAppend(*c, reserve, rollback))
                        {
                            continue;
                        }
                    }
                    else if (this.builder.TryAppend(escaped, reserve, rollback))
                    {
                        continue;
                    }

                    return false;
                }
            }

            return this.builder.TryAppend(JsonConstants.StringEnclosure, reserve, rollback);
        }
    }
}
