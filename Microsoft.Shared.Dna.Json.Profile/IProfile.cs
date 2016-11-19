//-----------------------------------------------------------------------------
// <copyright file="IProfile.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
//     Licensed under the MIT license. See license file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------------

namespace Microsoft.Shared.Dna.Json.Profile
{
    using System.Diagnostics;

    /// <summary>
    /// Profiler interface.
    /// </summary>
    public interface IProfile
    {
        /// <summary>
        /// Execute a single test iteration.
        /// </summary>
        /// <param name="watch">The stopwatch timing the iteration.</param>
        /// <returns>A value indicating whether or not the test is valid.</returns>
        bool Execute(Stopwatch watch);
    }
}
