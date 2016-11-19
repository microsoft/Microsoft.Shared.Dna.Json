//-----------------------------------------------------------------------------
// <copyright file="JsonDna.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
//     Licensed under the MIT license. See license file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------------

namespace Microsoft.Shared.Dna.Json.Profile
{
    using System;
    using System.Diagnostics;

    /// <summary>
    /// OSG Data and Analytics JSON profilers.
    /// </summary>
    public static class JsonDna
    {
        /// <summary>
        /// Reusable reader.
        /// </summary>
        [ThreadStatic]
        private static JsonParser reader = null;

        /// <summary>
        /// Reusable writer.
        /// </summary>
        [ThreadStatic]
        private static JsonBuilder writer = null;

        /// <summary>
        /// Prepare the reader for use in a single iteration.
        /// </summary>
        /// <param name="payload">The JSON payload to use.</param>
        /// <returns>A prepared reader.</returns>
        private static JsonParser PrepareReader(string payload)
        {
            JsonParser result = JsonDna.reader;
            if (result == null)
            {
                result = new JsonParser();
                JsonDna.reader = result;
            }

            result.Reset(payload);
            return result;
        }

        /// <summary>
        /// Prepare the writer for use in a single iteration.
        /// </summary>
        /// <returns>A prepared writer.</returns>
        private static JsonBuilder PerpareWriter()
        {
            JsonBuilder result = JsonDna.writer;
            if (result == null)
            {
                result = new JsonBuilder();
                JsonDna.writer = result;
            }

            result.Clear();
            return result;
        }

        /// <summary>
        /// Confirm the reader successfully parses the payload.
        /// </summary>
        /// <param name="reader">The reader to check.</param>
        /// <param name="watch">The stopwatch timing the iteration.</param>
        /// <returns>A value indicating whether or not the iteration is valid.</returns>
        private static bool ConfirmReader(JsonParser reader, Stopwatch watch)
        {
            bool valid = true;
            bool parsed = false;
            bool outcome = false;
            bool asLogical = false;
            long asIntegral = 0L;
            double asFloating = 0D;
            string asText = null;
            while (valid && reader.Next())
            {
                parsed = true;
                switch (reader.TokenType)
                {
                    case JsonTokenType.Boolean:
                        outcome = reader.TryParseToken(out asLogical);
                        watch.Stop();
                        valid = valid && outcome && asLogical == Constants.LogicalValue;
                        watch.Start();
                        break;
                    case JsonTokenType.Integer:
                        outcome = reader.TryParseToken(out asIntegral);
                        watch.Stop();
                        valid = valid && outcome && asIntegral == Constants.IntegralValue;
                        watch.Start();
                        break;
                    case JsonTokenType.Float:
                        outcome = reader.TryParseToken(out asFloating);
                        watch.Stop();
                        valid = valid && outcome && asFloating == Constants.FloatValue;
                        watch.Start();
                        break;
                    case JsonTokenType.BeginProperty:
                        outcome = reader.TryParseToken(out asText);
                        watch.Stop();
                        valid = valid && outcome && Constants.AnyProperty.ContainsKey(asText);
                        watch.Start();
                        break;
                    case JsonTokenType.String:
                        outcome = reader.TryParseToken(out asText);
                        watch.Stop();
                        valid = valid && outcome && string.CompareOrdinal(asText, Constants.TextValue) == 0;
                        watch.Start();
                        break;
                }
            }

            return parsed && valid;
        }

        /// <summary>
        /// Confirm the writer successfully builds the payload.
        /// </summary>
        /// <param name="writer">The writer to check.</param>
        /// <param name="expected">The expected payload.</param>
        /// <param name="watch">The stopwatch timing the iteration.</param>
        /// <returns>A value indicating whether or not the iteration is valid.</returns>
        private static bool ConfirmWriter(JsonBuilder writer, string expected, Stopwatch watch)
        {
            bool result = false;
            string actual = writer.Finish();
            watch.Stop();
            result = string.CompareOrdinal(expected, actual) == 0;
            watch.Start();
            return result;
        }

        /// <summary>
        /// Recursively build a complex object.
        /// </summary>
        /// <param name="writer">The writer to use.</param>
        /// <param name="width">The length of the recursive array.</param>
        /// <param name="depth">The depth of the recursive object.</param>
        private static void RecursiveObject(JsonBuilder writer, int width, int depth)
        {
            writer.OpenObject();
            writer.OpenProperty(Constants.LogicalProperty);
            writer.WriteValue(Constants.LogicalValue);
            writer.CloseToken();
            writer.OpenProperty(Constants.IntegralProperty);
            writer.WriteValue(Constants.IntegralValue);
            writer.CloseToken();
            writer.OpenProperty(Constants.FloatProperty);
            writer.WriteValue(Constants.FloatValue);
            writer.CloseToken();
            writer.OpenProperty(Constants.TextProperty);
            writer.WriteValue(Constants.TextValue);
            writer.CloseToken();
            if (depth > 0)
            {
                writer.OpenProperty(Constants.ArrayProperty);
                writer.OpenArray();
                for (int i = 0; i < width; i++)
                {
                    int shallow = depth - 1;
                    JsonDna.RecursiveObject(writer, width, shallow);
                }

                writer.CloseToken();
                writer.CloseToken();
            }

            writer.CloseToken();
        }

        /// <summary>
        /// Read logical value.
        /// </summary>
        public sealed class ReadLogical : IProfile
        {
            /// <summary>
            /// Execute a single test iteration.
            /// </summary>
            /// <param name="watch">The stopwatch timing the iteration.</param>
            /// <returns>A value indicating whether or not the test is valid.</returns>
            public bool Execute(Stopwatch watch)
            {
                JsonParser reader = JsonDna.PrepareReader(Constants.LogicalJson);
                return JsonDna.ConfirmReader(reader, watch);
            }
        }

        /// <summary>
        /// Read integral value.
        /// </summary>
        public sealed class ReadIntegral : IProfile
        {
            /// <summary>
            /// Execute a single test iteration.
            /// </summary>
            /// <param name="watch">The stopwatch timing the iteration.</param>
            /// <returns>A value indicating whether or not the test is valid.</returns>
            public bool Execute(Stopwatch watch)
            {
                JsonParser reader = JsonDna.PrepareReader(Constants.IntegralJson);
                return JsonDna.ConfirmReader(reader, watch);
            }
        }

        /// <summary>
        /// Read floating point value.
        /// </summary>
        public sealed class ReadFloat : IProfile
        {
            /// <summary>
            /// Execute a single test iteration.
            /// </summary>
            /// <param name="watch">The stopwatch timing the iteration.</param>
            /// <returns>A value indicating whether or not the test is valid.</returns>
            public bool Execute(Stopwatch watch)
            {
                JsonParser reader = JsonDna.PrepareReader(Constants.FloatJson);
                return JsonDna.ConfirmReader(reader, watch);
            }
        }

        /// <summary>
        /// Read text value.
        /// </summary>
        public sealed class ReadText : IProfile
        {
            /// <summary>
            /// Execute a single test iteration.
            /// </summary>
            /// <param name="watch">The stopwatch timing the iteration.</param>
            /// <returns>A value indicating whether or not the test is valid.</returns>
            public bool Execute(Stopwatch watch)
            {
                JsonParser reader = JsonDna.PrepareReader(Constants.TextJson);
                return JsonDna.ConfirmReader(reader, watch);
            }
        }

        /// <summary>
        /// Read array.
        /// </summary>
        public sealed class ReadArray : IProfile
        {
            /// <summary>
            /// Execute a single test iteration.
            /// </summary>
            /// <param name="watch">The stopwatch timing the iteration.</param>
            /// <returns>A value indicating whether or not the test is valid.</returns>
            public bool Execute(Stopwatch watch)
            {
                JsonParser reader = JsonDna.PrepareReader(Constants.ArrayJson);
                return JsonDna.ConfirmReader(reader, watch);
            }
        }

        /// <summary>
        /// Read object.
        /// </summary>
        public sealed class ReadObject : IProfile
        {
            /// <summary>
            /// Execute a single test iteration.
            /// </summary>
            /// <param name="watch">The stopwatch timing the iteration.</param>
            /// <returns>A value indicating whether or not the test is valid.</returns>
            public bool Execute(Stopwatch watch)
            {
                JsonParser reader = JsonDna.PrepareReader(Constants.ObjectJson);
                return JsonDna.ConfirmReader(reader, watch);
            }
        }

        /// <summary>
        /// Read complex object.
        /// </summary>
        public sealed class ReadComplex : IProfile
        {
            /// <summary>
            /// Execute a single test iteration.
            /// </summary>
            /// <param name="watch">The stopwatch timing the iteration.</param>
            /// <returns>A value indicating whether or not the test is valid.</returns>
            public bool Execute(Stopwatch watch)
            {
                JsonParser reader = JsonDna.PrepareReader(Constants.ComplexJson);
                return JsonDna.ConfirmReader(reader, watch);
            }
        }

        /// <summary>
        /// Write logical value.
        /// </summary>
        public sealed class WriteLogical : IProfile
        {
            /// <summary>
            /// Execute a single test iteration.
            /// </summary>
            /// <param name="watch">The stopwatch timing the iteration.</param>
            /// <returns>A value indicating whether or not the test is valid.</returns>
            public bool Execute(Stopwatch watch)
            {
                JsonBuilder writer = JsonDna.PerpareWriter();
                writer.WriteValue(Constants.LogicalValue);
                return JsonDna.ConfirmWriter(writer, Constants.LogicalJson, watch);
            }
        }

        /// <summary>
        /// Write integral value.
        /// </summary>
        public sealed class WriteIntegral : IProfile
        {
            /// <summary>
            /// Execute a single test iteration.
            /// </summary>
            /// <param name="watch">The stopwatch timing the iteration.</param>
            /// <returns>A value indicating whether or not the test is valid.</returns>
            public bool Execute(Stopwatch watch)
            {
                JsonBuilder writer = JsonDna.PerpareWriter();
                writer.WriteValue(Constants.IntegralValue);
                return JsonDna.ConfirmWriter(writer, Constants.IntegralJson, watch);
            }
        }

        /// <summary>
        /// Write floating-point value.
        /// </summary>
        public sealed class WriteFloat : IProfile
        {
            /// <summary>
            /// Execute a single test iteration.
            /// </summary>
            /// <param name="watch">The stopwatch timing the iteration.</param>
            /// <returns>A value indicating whether or not the test is valid.</returns>
            public bool Execute(Stopwatch watch)
            {
                JsonBuilder writer = JsonDna.PerpareWriter();
                writer.WriteValue(Constants.FloatValue);
                return JsonDna.ConfirmWriter(writer, Constants.FloatJson, watch);
            }
        }

        /// <summary>
        /// Write text value.
        /// </summary>
        public sealed class WriteText : IProfile
        {
            /// <summary>
            /// Execute a single test iteration.
            /// </summary>
            /// <param name="watch">The stopwatch timing the iteration.</param>
            /// <returns>A value indicating whether or not the test is valid.</returns>
            public bool Execute(Stopwatch watch)
            {
                JsonBuilder writer = JsonDna.PerpareWriter();
                writer.WriteValue(Constants.TextValue);
                return JsonDna.ConfirmWriter(writer, Constants.TextJson, watch);
            }
        }

        /// <summary>
        /// Write array.
        /// </summary>
        public sealed class WriteArray : IProfile
        {
            /// <summary>
            /// Execute a single test iteration.
            /// </summary>
            /// <param name="watch">The stopwatch timing the iteration.</param>
            /// <returns>A value indicating whether or not the test is valid.</returns>
            public bool Execute(Stopwatch watch)
            {
                JsonBuilder writer = JsonDna.PerpareWriter();
                writer.OpenArray();
                writer.WriteValue(Constants.LogicalValue);
                writer.WriteValue(Constants.IntegralValue);
                writer.WriteValue(Constants.FloatValue);
                writer.WriteValue(Constants.TextValue);
                writer.CloseToken();
                return JsonDna.ConfirmWriter(writer, Constants.ArrayJson, watch);
            }
        }

        /// <summary>
        /// Write object.
        /// </summary>
        public sealed class WriteObject : IProfile
        {
            /// <summary>
            /// Execute a single test iteration.
            /// </summary>
            /// <param name="watch">The stopwatch timing the iteration.</param>
            /// <returns>A value indicating whether or not the test is valid.</returns>
            public bool Execute(Stopwatch watch)
            {
                JsonBuilder writer = JsonDna.PerpareWriter();
                JsonDna.RecursiveObject(writer, 0, 0);
                return JsonDna.ConfirmWriter(writer, Constants.ObjectJson, watch);
            }
        }

        /// <summary>
        /// Write complex object.
        /// </summary>
        public sealed class WriteComplex : IProfile
        {
            /// <summary>
            /// Execute a single test iteration.
            /// </summary>
            /// <param name="watch">The stopwatch timing the iteration.</param>
            /// <returns>A value indicating whether or not the test is valid.</returns>
            public bool Execute(Stopwatch watch)
            {
                JsonBuilder writer = JsonDna.PerpareWriter();
                JsonDna.RecursiveObject(writer, 3, 3);
                return JsonDna.ConfirmWriter(writer, Constants.ComplexJson, watch);
            }
        }
    }
}
