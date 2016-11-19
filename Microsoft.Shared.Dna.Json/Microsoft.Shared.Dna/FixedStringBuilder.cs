//-----------------------------------------------------------------------------
// <copyright file="FixedStringBuilder.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
//     Licensed under the MIT license. See license file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------------

namespace Microsoft.Shared.Dna.Text
{
    using System;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Fixed-length string builder.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Microsoft.Performance",
        "CA1812:AvoidUninstantiatedInternalClasses",
        Justification = "You don't have to use everything in an in-line code share.")]
    internal sealed class FixedStringBuilder
    {
        /// <summary>
        /// Character buffer array.
        /// </summary>
        private char[] buffer = null;

        /// <summary>
        /// Cached buffer capacity. Always equal to this.buffer.Length.
        /// </summary>
        private int bufferCapacity = 0;

        /// <summary>
        /// Current length of the characters in the buffer.
        /// </summary>
        private int length = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="FixedStringBuilder"/> class.
        /// </summary>
        /// <param name="capacity">
        /// The maximum number of characters that the builder can create a string for.
        /// </param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        public FixedStringBuilder(int capacity)
        {
#if CONTRACTS_FULL // Work around the implicit rewrite requirements of Contract.Requires<T>
            Contract.Requires<ArgumentOutOfRangeException>(capacity >= 0);
#endif
            this.buffer = new char[capacity];
            this.bufferCapacity = capacity;
        }

        /// <summary>
        /// Gets the last character written to the builder, or the null character if nothing
        /// has been added.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        public char Last
        {
            get
            {
                if (this.length == 0)
                {
                    return char.MinValue;
                }

                return this.buffer[this.length - 1];
            }
        }

        /// <summary>
        /// Gets the current number of characters that have been written to the builder.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        public int Length
        {
            get { return this.length; }
        }

        /// <summary>
        /// Clears all the characters that have been written to the builder.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        public void Clear()
        {
            this.length = 0;
        }

        /// <summary>
        /// Converts all the characters in the builder to a single string.
        /// </summary>
        /// <returns>
        /// A string containing all the characters currently in the builder.
        /// </returns>
        public override string ToString()
        {
            return new string(this.buffer, 0, this.length);
        }

        /// <summary>
        /// Tries to append a single character to the builder.
        /// </summary>
        /// <param name="value">The character to append.</param>
        /// <param name="reserve">
        /// The amount of capacity that must remain in the builder after the character has
        /// been added.
        /// </param>
        /// <returns>A value indicating whether or not the character was added.</returns>
        /// <remarks>
        /// You'll see a lot of repeated code in the TryAppend methods. This is deliberate.
        /// Since these methods are are usually on the hot path, you get a significant
        /// performance boost by manually in-lining the code.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        public bool TryAppend(char value, int reserve)
        {
#if CONTRACTS_FULL // Work around the implicit rewrite requirements of Contract.Requires<T>
            Contract.Requires<ArgumentOutOfRangeException>(reserve >= 0);
#endif
            if (this.length + reserve < this.bufferCapacity)
            {
                this.buffer[this.length++] = value;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Tries to append a single character to the builder.
        /// </summary>
        /// <param name="value">The character to append.</param>
        /// <param name="reserve">
        /// The amount of capacity that must remain in the builder after the character has
        /// been added.
        /// </param>
        /// <param name="rollback">
        /// The length to set the builder to if the append failed.
        /// </param>
        /// <returns>A value indicating whether or not the character was added.</returns>
        /// <remarks>
        /// You'll see a lot of repeated code in the TryAppend methods. This is deliberate.
        /// Since these methods are are usually on the hot path, you get a significant
        /// performance boost by manually in-lining the code.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        public bool TryAppend(char value, int reserve, int rollback)
        {
#if CONTRACTS_FULL // Work around the implicit rewrite requirements of Contract.Requires<T>
            Contract.Requires<ArgumentOutOfRangeException>(reserve >= 0);
            Contract.Requires<ArgumentOutOfRangeException>(rollback >= 0);
#endif
            if (this.length + reserve < this.bufferCapacity)
            {
                this.buffer[this.length++] = value;
                return true;
            }

            this.length = rollback;
            return false;
        }

        /// <summary>
        /// Tries to append a single character to the builder.
        /// </summary>
        /// <param name="value">The character to append.</param>
        /// <param name="reserve">
        /// The amount of capacity that must remain in the builder after the character has
        /// been added.
        /// </param>
        /// <param name="rollback">
        /// The length of the builder before the character was added.
        /// </param>
        /// <returns>A value indicating whether or not the character was added.</returns>
        /// <remarks>
        /// You'll see a lot of repeated code in the TryAppend methods. This is deliberate.
        /// Since these methods are are usually on the hot path, you get a significant
        /// performance boost by manually in-lining the code.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        public bool TryAppend(char value, int reserve, out int rollback)
        {
#if CONTRACTS_FULL // Work around the implicit rewrite requirements of Contract.Requires<T>
            Contract.Requires<ArgumentOutOfRangeException>(reserve >= 0);
#endif
            rollback = this.length;
            if (this.length + reserve < this.bufferCapacity)
            {
                this.buffer[this.length++] = value;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Tries to append a string to the builder.
        /// </summary>
        /// <param name="value">The string to append.</param>
        /// <param name="reserve">
        /// The amount of capacity that must remain in the builder after the string has been
        /// added.
        /// </param>
        /// <returns>A value indicating whether or not the string was added.</returns>
        /// <remarks>
        /// You'll see a lot of repeated code in the TryAppend methods. This is deliberate.
        /// Since these methods are are usually on the hot path, you get a significant
        /// performance boost by manually in-lining the code.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        public bool TryAppend(string value, int reserve)
        {
#if CONTRACTS_FULL // Work around the implicit rewrite requirements of Contract.Requires<T>
            Contract.Requires<ArgumentOutOfRangeException>(reserve >= 0);
#endif
            return this.TryAppendUnsafe(value, reserve);
        }

        /// <summary>
        /// Tries to append a string to the builder.
        /// </summary>
        /// <param name="value">The string to append.</param>
        /// <param name="reserve">
        /// The amount of capacity that must remain in the builder after the string has been
        /// added.
        /// </param>
        /// <param name="rollback">
        /// The length to set the builder to if the append failed.
        /// </param>
        /// <returns>A value indicating whether or not the string was added.</returns>
        /// <remarks>
        /// You'll see a lot of repeated code in the TryAppend methods. This is deliberate.
        /// Since these methods are are usually on the hot path, you get a significant
        /// performance boost by manually in-lining the code.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        public bool TryAppend(string value, int reserve, int rollback)
        {
#if CONTRACTS_FULL // Work around the implicit rewrite requirements of Contract.Requires<T>
            Contract.Requires<ArgumentOutOfRangeException>(reserve >= 0);
            Contract.Requires<ArgumentOutOfRangeException>(rollback >= 0);
#endif
            return this.TryAppendUnsafe(value, reserve, rollback);
        }

        /// <summary>
        /// Tries to expand the capacity of the string builder.
        /// </summary>
        /// <param name="capacity">
        /// The new maximum number of characters that the builder can create a string for.
        /// </param>
        /// <returns>
        /// A value indicating whether the builder was expanded or the current capacity was
        /// sufficient.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        public bool TryExpand(int capacity)
        {
#if CONTRACTS_FULL // Work around the implicit rewrite requirements of Contract.Requires<T>
            Contract.Requires<ArgumentOutOfRangeException>(capacity >= 0);
#endif
            if (capacity > this.bufferCapacity)
            {
                this.Resize(capacity);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Tries to change the capacity of the string builder.
        /// </summary>
        /// <param name="capacity">
        /// The new maximum number of characters that the builder can create a string for.
        /// </param>
        /// <param name="reserve">
        /// The amount of capacity that must remain in the builder after the builder has
        /// been resized.
        /// </param>
        /// <returns>
        /// A value indicating whether the builder was resized or the current content and
        /// reserve exceeded the new capacity.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        public bool TryResize(int capacity, int reserve)
        {
#if CONTRACTS_FULL // Work around the implicit rewrite requirements of Contract.Requires<T>
            Contract.Requires<ArgumentOutOfRangeException>(capacity >= reserve);
            Contract.Requires<ArgumentOutOfRangeException>(reserve >= 0);
#endif
            if (capacity - reserve >= this.length)
            {
                this.Resize(capacity);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Changes the capacity of the string builder.
        /// </summary>
        /// <param name="capacity">
        /// The new maximum number of characters that the builder can create a string for.
        /// </param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        private void Resize(int capacity)
        {
            char[] resized = new char[capacity];
            for (int i = 0; i < this.length; i++)
            {
                resized[i] = this.buffer[i];
            }

            this.buffer = resized;
            this.bufferCapacity = capacity;
        }

        /// <summary>
        /// Tries to append a string to the builder.
        /// </summary>
        /// <param name="value">The string to append.</param>
        /// <param name="reserve">
        /// The amount of capacity that must remain in the builder after the string has been
        /// added.
        /// </param>
        /// <returns>A value indicating whether or not the string was added.</returns>
        /// <remarks>
        /// This method uses character pointer arithmetic to iterate over the string because
        /// it is significantly faster than using the string indexer.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        private unsafe bool TryAppendUnsafe(string value, int reserve)
        {
            if (value == null)
            {
                return true;
            }

            int valueLength = value.Length;
            if (this.length + valueLength + reserve <= this.bufferCapacity)
            {
                fixed (char* valuePointer = value)
                {
                    for (int i = 0; i < valueLength; i++)
                    {
                        this.buffer[this.length++] = *(valuePointer + i);
                    }
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Tries to append a string to the builder.
        /// </summary>
        /// <param name="value">The string to append.</param>
        /// <param name="reserve">
        /// The amount of capacity that must remain in the builder after the string has been
        /// added.
        /// </param>
        /// <param name="rollback">
        /// The length to set the builder to if the append failed.
        /// </param>
        /// <returns>A value indicating whether or not the string was added.</returns>
        /// <remarks>
        /// This method uses character pointer arithmetic to iterate over the string because
        /// it is significantly faster than using the string indexer.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        private unsafe bool TryAppendUnsafe(string value, int reserve, int rollback)
        {
            if (value == null)
            {
                return true;
            }

            int valueLength = value.Length;
            if (this.length + valueLength + reserve <= this.bufferCapacity)
            {
                fixed (char* valuePointer = value)
                {
                    for (int i = 0; i < valueLength; i++)
                    {
                        this.buffer[this.length++] = *(valuePointer + i);
                    }
                }

                return true;
            }

            this.length = rollback;
            return false;
        }
    }
}
