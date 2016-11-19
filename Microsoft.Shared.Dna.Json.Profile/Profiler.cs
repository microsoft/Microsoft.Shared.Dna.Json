//-----------------------------------------------------------------------------
// <copyright file="Profiler.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
//     Licensed under the MIT license. See license file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------------

namespace Microsoft.Shared.Dna.Json.Profile
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using System.Threading;

    /// <summary>
    /// Performance profiler.
    /// </summary>
    public static class Profiler
    {
        /// <summary>
        /// Execute a profile.
        /// </summary>
        /// <typeparam name="T">The profile type.</typeparam>
        /// <param name="iterations">The number of iterations.</param>
        /// <param name="meanElapsedMilliseconds">
        /// The average elapsed time per iteration.
        /// </param>
        /// <returns>A value indicating whether or not the execution is valid.</returns>
        public static bool Execute<T>(int iterations, out double meanElapsedMilliseconds) where T : IProfile, new()
        {
            return Profiler.ExecuteGeneric<T>(iterations, out meanElapsedMilliseconds);
        }

        /// <summary>
        /// Execute a profile.
        /// </summary>
        /// <param name="type">The profile type.</param>
        /// <param name="iterations">The number of iterations.</param>
        /// <param name="meanElapsedMilliseconds">
        /// The average elapsed time per iteration.
        /// </param>
        /// <returns>A value indicating whether or not the execution is valid.</returns>
        public static bool Execute(Type type, int iterations, out double meanElapsedMilliseconds)
        {
            object[] parameters = new object[] { iterations, 0D };
            bool result = (bool)typeof(Profiler)
                .GetMethod("ExecuteGeneric", BindingFlags.NonPublic | BindingFlags.Static)
                .MakeGenericMethod(type)
                .Invoke(null, parameters);
            meanElapsedMilliseconds = (double)parameters[1];
            return result;
        }

        /// <summary>
        /// Execute a profile.
        /// </summary>
        /// <typeparam name="T">The profile type.</typeparam>
        /// <param name="iterations">The number of iterations.</param>
        /// <param name="meanElapsedMilliseconds">
        /// The average elapsed time per iteration.
        /// </param>
        /// <returns>A value indicating whether or not the execution is valid.</returns>
        private static bool ExecuteGeneric<T>(int iterations, out double meanElapsedMilliseconds) where T : IProfile, new()
        {
            bool result = true;
            int dop = Environment.ProcessorCount;
            Thread[] executors = new Thread[dop];
            ExecuteState[] states = new ExecuteState[dop];
            int perExecutor = iterations / dop;
            int remainder = iterations % dop;
            for (int i = 0; i < dop; i++)
            {
                states[i] = new ExecuteState
                {
                    Iterations = perExecutor + (--remainder > 0 ? 1 : 0),
                    Count = 0L,
                    Total = 0L,
                    Valid = true
                };
                executors[i] = new Thread(Profiler.ExecutePartial<T>);
                executors[i].Start(states[i]);
            }

            long total = 0L;
            long count = 0L;
            for (int i = 0; i < dop; i++)
            {
                executors[i].Join();
                total += states[i].Total;
                count += states[i].Count;
                result = result && states[i].Valid;
            }

            meanElapsedMilliseconds = (double)total / TimeSpan.TicksPerMillisecond / count;
            return result;
        }

        /// <summary>
        /// Execute a fraction of the iterations for a profile.
        /// </summary>
        /// <typeparam name="T">The profile type.</typeparam>
        /// <param name="state">The execution state.</param>
        private static void ExecutePartial<T>(object state) where T : IProfile, new()
        {
            ExecuteState asExecute = state as ExecuteState;
            T profile = new T();
            Stopwatch watch = new Stopwatch();
            for (int warmup = asExecute.Iterations / 10; warmup > 0; warmup--)
            {
                profile.Execute(watch);
            }

            bool valid = false;
            for (int i = 0; i < asExecute.Iterations; i++)
            {
                try
                {
                    watch.Restart();
                    valid = profile.Execute(watch);
                    watch.Stop();
                }
                catch
                {
                    valid = false;
                }

                if (valid)
                {
                    asExecute.Total += watch.ElapsedTicks;
                    asExecute.Count++;
                }

                asExecute.Valid = asExecute.Valid && valid;
            }
        }

        /// <summary>
        /// Profiler execution state.
        /// </summary>
        private sealed class ExecuteState
        {
            /// <summary>
            /// Gets or sets the number of valid iterations.
            /// </summary>
            public long Count { get; set; }

            /// <summary>
            /// Gets or sets the number of iterations to attempt.
            /// </summary>
            public int Iterations { get; set; }

            /// <summary>
            /// Gets or sets the total execution time of all iterations in ticks.
            /// </summary>
            public long Total { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether or not all iterations were valid.
            /// </summary>
            public bool Valid { get; set; }
        }
    }
}
