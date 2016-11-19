//-----------------------------------------------------------------------------
// <copyright file="Program.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
//     Licensed under the MIT license. See license file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------------

namespace Microsoft.Shared.Dna.Json.Profile
{
    using System;
    using System.Globalization;

    /// <summary>
    /// Entry point class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Entry point method.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        public static void Main(string[] args)
        {
            try
            {
                string group = args[0];
                string profile = args[1];
                int iterations = int.Parse(args[2], CultureInfo.InvariantCulture);
                string typeName = string.Concat("Microsoft.Shared.Dna.Json.Profile." + group + "+" + profile);
                Type type = null;
                try
                {
                    type = typeof(Program).Assembly.GetType(typeName);
                }
                catch 
                {
                }
                
                if (type == null)
                {
                    Console.WriteLine("Couldn't find profile \"{0}\" in group \"{1}\".", profile, group);
                    return;
                }

                double meanExecutionMilliseconds = 0D;
                bool valid = Profiler.Execute(type, iterations, out meanExecutionMilliseconds);
                Console.WriteLine("{0},{1},{2},{3}", group, profile, meanExecutionMilliseconds, valid);
            }
            catch
            {
                Console.WriteLine("Usage: Microsoft.Shared.Dna.Json.Profile.exe (group) (profile) (iterations)");
                Console.WriteLine("  Groups:");
                Console.WriteLine("    JsonDna");
                Console.WriteLine("  Reading Profiles:");
                Console.WriteLine("    ReadLogical");
                Console.WriteLine("    ReadIntegral");
                Console.WriteLine("    ReadFloat");
                Console.WriteLine("    ReadText");
                Console.WriteLine("    ReadArray");
                Console.WriteLine("    ReadObject");
                Console.WriteLine("    ReadComplex");
                Console.WriteLine("  Writing Profiles:");
                Console.WriteLine("    WriteLogical");
                Console.WriteLine("    WriteIntegral");
                Console.WriteLine("    WriteFloat");
                Console.WriteLine("    WriteText");
                Console.WriteLine("    WriteArray");
                Console.WriteLine("    WriteObject");
                Console.WriteLine("    WriteComplex");
            }
        }
    }
}
