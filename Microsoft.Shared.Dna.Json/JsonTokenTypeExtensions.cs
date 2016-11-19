//-----------------------------------------------------------------------------
// <copyright file="JsonTokenTypeExtensions.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
//     Licensed under the MIT license. See license file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------------

namespace Microsoft.Shared.Dna.Json
{
    using System;

    /// <summary>
    /// Represents the various JSON token types that the parser can encounter.
    /// </summary>
    [Flags]
    internal enum JsonTokenType : short
    {
        /// <summary>
        /// No token encountered yet.
        /// </summary>
        None = 0,

        /// <summary>
        /// The beginning of an array.
        /// </summary>
        BeginArray = 1,

        /// <summary>
        /// The end of an array.
        /// </summary>
        EndArray = 2,

        /// <summary>
        /// The beginning of an object.
        /// </summary>
        BeginObject = 4,

        /// <summary>
        /// The end of an object.
        /// </summary>
        EndObject = 8,

        /// <summary>
        /// The beginning of a property.
        /// </summary>
        BeginProperty = 16,

        /// <summary>
        /// The end of a property.
        /// </summary>
        EndProperty = 32,

        /// <summary>
        /// A null literal.
        /// </summary>
        Null = 64,

        /// <summary>
        /// A boolean literal - true or false.
        /// </summary>
        Boolean = 128,

        /// <summary>
        /// An integer value.
        /// </summary>
        Integer = 256,

        /// <summary>
        /// A floating-point value.
        /// </summary>
        Float = 512,

        /// <summary>
        /// A string value.
        /// </summary>
        String = 1024,

        /// <summary>
        /// Parsing reached the end of the string with no errors.
        /// </summary>
        Complete = 2048,

        /// <summary>
        /// Parsing encountered a malformed token in the string.
        /// </summary>
        Invalid = 4096,

        /// <summary>
        /// Any open container type - arrays, objects or properties.
        /// </summary>
        OpenContainer = BeginArray | BeginObject | BeginProperty,

        /// <summary>
        /// Any closed container type - arrays, objects or properties.
        /// </summary>
        ClosedContainer = EndArray | EndObject | EndProperty,
        
        /// <summary>
        /// Any container type - arrays, objects or properties.
        /// </summary>
        Container = BeginArray | EndArray | BeginObject | EndObject | BeginProperty | EndProperty,

        /// <summary>
        /// Any numeric type - integers or floating-point values.
        /// </summary>
        Number = Integer | Float,

        /// <summary>
        /// Any value type - nulls, booleans, numbers, or strings.
        /// </summary>
        Value = Null | Boolean | Integer | Float | String,

        /// <summary>
        /// Parsing is complete - either because the end of the string was reached or an
        /// invalid token was encountered.
        /// </summary>
        EndOfPayload = Complete | Invalid
    }

    /// <summary>
    /// Extension methods for <see cref="JsonTokenType"/>.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Microsoft.Performance",
        "CA1812:AvoidUninstantiatedInternalClasses",
        Justification = "You don't have to use everything in an in-line code share.")]
    internal static class JsonTokenTypeExtensions
    {
        /// <summary>
        /// Determines if a token type is a closed container.
        /// </summary>
        /// <param name="target">The token to check.</param>
        /// <returns>
        /// A value indicating whether or not the token is a closed container.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        public static bool IsClosedContainer(this JsonTokenType target)
        {
            return (target & JsonTokenType.ClosedContainer) != JsonTokenType.None;
        }

        /// <summary>
        /// Determines if a token type is a container or not.
        /// </summary>
        /// <param name="target">The token to check.</param>
        /// <returns>A value indicating whether or not the token is a container.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        public static bool IsContainer(this JsonTokenType target)
        {
            return (target & JsonTokenType.Container) != JsonTokenType.None;
        }

        /// <summary>
        /// Determines if a token is at the end of the payload.
        /// </summary>
        /// <param name="target">The token to check.</param>
        /// <returns>
        /// A value indicating whether or not the token is at the end of the payload.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        public static bool IsEndOfPayload(this JsonTokenType target)
        {
            return (target & JsonTokenType.EndOfPayload) != JsonTokenType.None;
        }

        /// <summary>
        /// Determines if a token type is a null literal.
        /// </summary>
        /// <param name="target">The token to check.</param>
        /// <returns>
        /// A value indicating whether or not the token is a null literal.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        public static bool IsNull(this JsonTokenType target)
        {
            return target == JsonTokenType.Null;
        }

        /// <summary>
        /// Determines if a token type is an open container.
        /// </summary>
        /// <param name="target">The token to check.</param>
        /// <returns>
        /// A value indicating whether or not the token is an open container.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        public static bool IsOpenContainer(this JsonTokenType target)
        {
            return (target & JsonTokenType.OpenContainer) != JsonTokenType.None;
        }

        /// <summary>
        /// Determines if a token type is a value or not.
        /// </summary>
        /// <param name="target">The token to check.</param>
        /// <returns>A value indicating whether or not the token is a value.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        public static bool IsValue(this JsonTokenType target)
        {
            return (target & JsonTokenType.Value) != JsonTokenType.None;
        }
    }
}
