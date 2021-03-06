﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using KGP.Frames;

namespace KGP.Tests
{
    [TestClass]
    public class DepthFrameDataTests
    {
        [TestMethod]
        public void TestConstrutor()
        {
            DepthFrameData data = new DepthFrameData();
            bool pass = data.DataPointer != IntPtr.Zero;
            data.Dispose();
            Assert.AreEqual(true, pass);
        }

        [TestMethod]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void TestDispose()
        {
            DepthFrameData data = new DepthFrameData();
            data.Dispose();
            Assert.AreEqual(data.DataPointer, IntPtr.Zero);
        }

        [TestMethod]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void TestDisposeAccess()
        {
            DepthFrameData data = new DepthFrameData();
            data.Dispose();

            //Should throw exception
            var pointer = data.DataPointer;
        }

        [TestMethod]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void TestMultipleDispose()
        {
            DepthFrameData data = new DepthFrameData();
            data.Dispose();
            //Second call to dispose should do nothing
            data.Dispose();
            
            Assert.AreEqual(data.DataPointer, IntPtr.Zero);
        }

        [TestMethod]
        public void TestSize()
        {
            DepthFrameData data = new DepthFrameData();
            int expected = 512 * 424 * 2;
            bool pass = data.SizeInBytes == expected;
            data.Dispose();
            Assert.AreEqual(pass, true);
        }

        [TestMethod]
        public void TestDisposedSize()
        {
            DepthFrameData data = new DepthFrameData();
            data.Dispose();
            Assert.AreEqual(data.SizeInBytes, 0);
        }
    }
}
