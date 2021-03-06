﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using KGP.Direct3D11.Textures;
using KGP.Frames;

namespace KDP.Direct3D11.Tests.Textures
{
    [TestClass]
    public class DynamicColorToDepthTextureTests : IDisposable
    {
        private SharpDX.Direct3D11.Device device;


        public DynamicColorToDepthTextureTests()
        {
            device = new SharpDX.Direct3D11.Device(SharpDX.Direct3D.DriverType.Reference);
        }

        [TestMethod]
        public void TestCreate()
        {
            using (DynamicColorToDepthTexture texture = new DynamicColorToDepthTexture(device))
            {
                Assert.AreNotEqual(texture.ShaderView.NativePointer, IntPtr.Zero);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestNullDevice()
        {
            using (DynamicColorToDepthTexture texture = new DynamicColorToDepthTexture(null))
            {
            }
        }

        [TestMethod]
        public void TestCopy()
        {
            using (ColorToDepthFrameData frame = new ColorToDepthFrameData())
            {
                using (DynamicColorToDepthTexture texture = new DynamicColorToDepthTexture(device))
                {
                    texture.Copy(device.ImmediateContext, frame);
                }
            }
        }

        public void Dispose()
        {
            device.Dispose();
        }
    }
}
