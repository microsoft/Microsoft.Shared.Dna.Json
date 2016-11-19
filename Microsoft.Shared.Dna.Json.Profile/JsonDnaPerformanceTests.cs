//-----------------------------------------------------------------------------
// <copyright file="JsonDnaPerformanceTests.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
//     Licensed under the MIT license. See license file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------------

namespace Microsoft.Shared.Dna.Json.Profile
{
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Performance tests for OSG Data and Analytics JSON code.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class JsonDnaPerformanceTests
    {
        /// <summary>
        /// Gets or sets the test context.
        /// </summary>
        public TestContext TestContext { get; set; }

        /// <summary>
        /// Read logical.
        /// </summary>
        [TestMethod]
        public void JsonDna_Read_Logical()
        {
            double meanExecutionMilliseconds = 0D;
            Assert.IsTrue(Profiler.Execute<JsonDna.ReadLogical>(Constants.PerformanceIterations, out meanExecutionMilliseconds));
            this.TestContext.WriteLine("Average execution time (ms): {0}", meanExecutionMilliseconds);
        }

        /// <summary>
        /// Read integral.
        /// </summary>
        [TestMethod]
        public void JsonDna_Read_Integral()
        {
            double meanExecutionMilliseconds = 0D;
            Assert.IsTrue(Profiler.Execute<JsonDna.ReadIntegral>(Constants.PerformanceIterations, out meanExecutionMilliseconds));
            this.TestContext.WriteLine("Average execution time (ms): {0}", meanExecutionMilliseconds);
        }

        /// <summary>
        /// Read float.
        /// </summary>
        [TestMethod]
        public void JsonDna_Read_Float()
        {
            double meanExecutionMilliseconds = 0D;
            Assert.IsTrue(Profiler.Execute<JsonDna.ReadFloat>(Constants.PerformanceIterations, out meanExecutionMilliseconds));
            this.TestContext.WriteLine("Average execution time (ms): {0}", meanExecutionMilliseconds);
        }

        /// <summary>
        /// Read text.
        /// </summary>
        [TestMethod]
        public void JsonDna_Read_Text()
        {
            double meanExecutionMilliseconds = 0D;
            Assert.IsTrue(Profiler.Execute<JsonDna.ReadText>(Constants.PerformanceIterations, out meanExecutionMilliseconds));
            this.TestContext.WriteLine("Average execution time (ms): {0}", meanExecutionMilliseconds);
        }

        /// <summary>
        /// Read array.
        /// </summary>
        [TestMethod]
        public void JsonDna_Read_Array()
        {
            double meanExecutionMilliseconds = 0D;
            Assert.IsTrue(Profiler.Execute<JsonDna.ReadArray>(Constants.PerformanceIterations, out meanExecutionMilliseconds));
            this.TestContext.WriteLine("Average execution time (ms): {0}", meanExecutionMilliseconds);
        }

        /// <summary>
        /// Read object.
        /// </summary>
        [TestMethod]
        public void JsonDna_Read_Object()
        {
            double meanExecutionMilliseconds = 0D;
            Assert.IsTrue(Profiler.Execute<JsonDna.ReadObject>(Constants.PerformanceIterations, out meanExecutionMilliseconds));
            this.TestContext.WriteLine("Average execution time (ms): {0}", meanExecutionMilliseconds);
        }

        /// <summary>
        /// Read complex.
        /// </summary>
        [TestMethod]
        public void JsonDna_Read_Complex()
        {
            double meanExecutionMilliseconds = 0D;
            Assert.IsTrue(Profiler.Execute<JsonDna.ReadComplex>(Constants.PerformanceIterations, out meanExecutionMilliseconds));
            this.TestContext.WriteLine("Average execution time (ms): {0}", meanExecutionMilliseconds);
        }

        /// <summary>
        /// Write logical.
        /// </summary>
        [TestMethod]
        public void JsonDna_Write_Logical()
        {
            double meanExecutionMilliseconds = 0D;
            Assert.IsTrue(Profiler.Execute<JsonDna.WriteLogical>(Constants.PerformanceIterations, out meanExecutionMilliseconds));
            this.TestContext.WriteLine("Average execution time (ms): {0}", meanExecutionMilliseconds);
        }

        /// <summary>
        /// Write integral.
        /// </summary>
        [TestMethod]
        public void JsonDna_Write_Integral()
        {
            double meanExecutionMilliseconds = 0D;
            Assert.IsTrue(Profiler.Execute<JsonDna.WriteIntegral>(Constants.PerformanceIterations, out meanExecutionMilliseconds));
            this.TestContext.WriteLine("Average execution time (ms): {0}", meanExecutionMilliseconds);
        }

        /// <summary>
        /// Write float.
        /// </summary>
        [TestMethod]
        public void JsonDna_Write_Float()
        {
            double meanExecutionMilliseconds = 0D;
            Assert.IsTrue(Profiler.Execute<JsonDna.WriteFloat>(Constants.PerformanceIterations, out meanExecutionMilliseconds));
            this.TestContext.WriteLine("Average execution time (ms): {0}", meanExecutionMilliseconds);
        }

        /// <summary>
        /// Write text.
        /// </summary>
        [TestMethod]
        public void JsonDna_Write_Text()
        {
            double meanExecutionMilliseconds = 0D;
            Assert.IsTrue(Profiler.Execute<JsonDna.WriteText>(Constants.PerformanceIterations, out meanExecutionMilliseconds));
            this.TestContext.WriteLine("Average execution time (ms): {0}", meanExecutionMilliseconds);
        }

        /// <summary>
        /// Write array.
        /// </summary>
        [TestMethod]
        public void JsonDna_Write_Array()
        {
            double meanExecutionMilliseconds = 0D;
            Assert.IsTrue(Profiler.Execute<JsonDna.WriteArray>(Constants.PerformanceIterations, out meanExecutionMilliseconds));
            this.TestContext.WriteLine("Average execution time (ms): {0}", meanExecutionMilliseconds);
        }

        /// <summary>
        /// Write object.
        /// </summary>
        [TestMethod]
        public void JsonDna_Write_Object()
        {
            double meanExecutionMilliseconds = 0D;
            Assert.IsTrue(Profiler.Execute<JsonDna.WriteObject>(Constants.PerformanceIterations, out meanExecutionMilliseconds));
            this.TestContext.WriteLine("Average execution time (ms): {0}", meanExecutionMilliseconds);
        }

        /// <summary>
        /// Write complex.
        /// </summary>
        [TestMethod]
        public void JsonDna_Write_Complex()
        {
            double meanExecutionMilliseconds = 0D;
            Assert.IsTrue(Profiler.Execute<JsonDna.WriteComplex>(Constants.PerformanceIterations, out meanExecutionMilliseconds));
            this.TestContext.WriteLine("Average execution time (ms): {0}", meanExecutionMilliseconds);
        }
    }
}
