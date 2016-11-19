//-----------------------------------------------------------------------------
// <copyright file="StringSegment.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
//     Licensed under the MIT license. See license file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------------

namespace Microsoft.Shared.Dna.Text
{
    using System;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Delimits a section of a string.
    /// </summary>
    /// <remarks>
    /// It's surprising that this isn't in the .NET framework itself given the existence
    /// of <see cref="System.ArraySegment&lt;T&gt;"/>.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Microsoft.Performance",
        "CA1812:AvoidUninstantiatedInternalClasses",
        Justification = "You don't have to use everything in an in-line code share.")]
    internal struct StringSegment
    {
        /// <summary>
        /// The original string containing the substring.
        /// </summary>
        public readonly string String;

        /// <summary>
        /// The position of the first character in the substring.
        /// </summary>
        public readonly int Offset;

        /// <summary>
        /// The number of characters in the substring.
        /// </summary>
        public readonly int Count;

        /// <summary>
        /// Initializes a new instance of the <see cref="StringSegment"/> struct.
        /// </summary>
        /// <param name="str">The string to delimit</param>
        /// <remarks>
        /// This creates a substring containing the entire string.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        public StringSegment(string str)
        {
#if CONTRACTS_FULL // Work around the implicit rewrite requirements of Contract.Requires<T>
            Contract.Requires<ArgumentNullException>(str != null);
#endif
            this.String = str;
            this.Offset = 0;
            this.Count = str.Length;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StringSegment"/> struct.
        /// </summary>
        /// <param name="str">The original string containing the substring.</param>
        /// <param name="offset">
        /// The position of the first character in the substring.
        /// </param>
        /// <remarks>
        /// The count is set to the remainder of the string.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        public StringSegment(string str, int offset)
        {
#if CONTRACTS_FULL // Work around the implicit rewrite requirements of Contract.Requires<T>
            Contract.Requires<ArgumentNullException>(str != null);
            Contract.Requires<ArgumentOutOfRangeException>(offset >= 0 && (offset == 0 || offset < str.Length));
#endif
            this.String = str;
            this.Offset = offset;
            this.Count = str.Length - offset;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StringSegment"/> struct.
        /// </summary>
        /// <param name="str">The original string containing the substring.</param>
        /// <param name="offset">
        /// The position of the first character in the substring.
        /// </param>
        /// <param name="count">The number of characters in the substring.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        public StringSegment(string str, int offset, int count)
        {
#if CONTRACTS_FULL // Work around the implicit rewrite requirements of Contract.Requires<T>
            Contract.Requires<ArgumentNullException>(str != null);
            Contract.Requires<ArgumentOutOfRangeException>(offset >= 0 && (offset == 0 || offset < str.Length));
            Contract.Requires<ArgumentOutOfRangeException>(count >= 0 && count <= str.Length - offset);
#endif
            this.String = str;
            this.Offset = offset;
            this.Count = count;
        }

        /// <summary>
        /// Creates a substring from the original string.
        /// </summary>
        /// <returns>
        /// A substring starting at the offset position and having a length equal to the
        /// count.
        /// </returns>
        public override string ToString()
        {
            return this.String.Substring(this.Offset, this.Count);
        }
    }
}
