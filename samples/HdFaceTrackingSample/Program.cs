﻿using FeralTic.DX11;
using FeralTic.DX11.Geometry;
using FeralTic.DX11.Resources;
using KGP;
using KGP.Direct3D11;
using KGP.Direct3D11.Buffers;
using KGP.Direct3D11.DataTables;
using KGP.Direct3D11.Textures;
using KGP.Frames;
using KGP.Processors;
using KGP.Providers;
using KGP.Providers.Sensor;
using Microsoft.Kinect;
using Microsoft.Kinect.Face;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using SharpDX.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JointColorSample
{
    [StructLayout(LayoutKind.Sequential)]
    public struct cbCamera
    {
        public Matrix View;
        public Matrix Projection;
    }

    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            RenderForm form = new RenderForm("Kinect Simple hd face sample");

            RenderDevice device = new RenderDevice(SharpDX.Direct3D11.DeviceCreationFlags.BgraSupport | DeviceCreationFlags.Debug);
            RenderContext context = new RenderContext(device);
            DX11SwapChain swapChain = DX11SwapChain.FromHandle(device, form.Handle);

            VertexShader vertexShader = ShaderCompiler.CompileFromFile <VertexShader>(device, "HdFaceView.fx", "VS");
            PixelShader pixelShader = ShaderCompiler.CompileFromFile<PixelShader>(device, "HdFaceView.fx", "PS");


            HdFaceIndexBuffer faceIndexBuffer = new HdFaceIndexBuffer(device, 1);
            DynamicHdFaceStructuredBuffer faceVertexBuffer = new DynamicHdFaceStructuredBuffer(device, 1);

            KinectSensor sensor = KinectSensor.GetDefault();
            sensor.Open();

            cbCamera camera = new cbCamera();
            camera.Projection = Matrix.PerspectiveFovLH(1.57f*0.5f, 1.3f, 0.01f, 100.0f);
            camera.View = Matrix.Translation(0.0f, 0.0f, 0.5f);

            camera.Projection.Transpose();
            camera.View.Transpose();

            ConstantBuffer<cbCamera> cameraBuffer = new ConstantBuffer<cbCamera>(device);
            cameraBuffer.Update(context, ref camera);

            bool doQuit = false;
            bool doUpload = false;


            KinectBody[] bodyFrame = null;
            KinectSensorBodyFrameProvider provider = new KinectSensorBodyFrameProvider(sensor);
            

            form.KeyDown += (sender, args) => { if (args.KeyCode == Keys.Escape) { doQuit = true; } };

            FaceModel currentFaceModel = new FaceModel();
            FaceAlignment currentFaceAlignment = new FaceAlignment();

            SingleHdFaceProcessor hdFaceProcessor = new SingleHdFaceProcessor(sensor);
            hdFaceProcessor.HdFrameReceived += (sender, args) => { currentFaceModel = args.FaceModel; currentFaceAlignment = args.FaceAlignment; doUpload = true; };

            provider.FrameReceived += (sender, args) => 
            { 
                bodyFrame = args.FrameData;
                var body = bodyFrame.TrackedOnly().ClosestBodies().FirstOrDefault();
                if (body != null)
                {
                    hdFaceProcessor.AssignBody(body);
                }
                else
                {
                    hdFaceProcessor.Suspend();
                }
            };
            
            context.Context.Rasterizer.State = device.RasterizerStates.WireFrame;

            RenderLoop.Run(form, () =>
            {
                if (doQuit)
                {
                    form.Dispose();
                    return;
                }

                if (doUpload)
                {
                    var vertices = currentFaceModel.CalculateVerticesForAlignment(currentFaceAlignment).ToArray();
                    faceVertexBuffer.Copy(context, vertices);
                    doUpload = false;
                }

                context.Context.ClearRenderTargetView(swapChain.RenderView, SharpDX.Color.Black);

                if (hdFaceProcessor.IsValid)
                {
                    context.RenderTargetStack.Push(swapChain);
                    context.Context.VertexShader.SetShaderResource(0, faceVertexBuffer.ShaderView);
                    context.Context.VertexShader.SetConstantBuffer(0, cameraBuffer.Buffer);

                    //Draw lines
                    context.Context.PixelShader.Set(pixelShader);
                    context.Context.VertexShader.Set(vertexShader);

                    //Attach index buffer, null topology since we fetch
                    faceIndexBuffer.AttachWithLayout(context);
                    faceIndexBuffer.Draw(context, 1);
                    context.RenderTargetStack.Pop();
                }

                swapChain.Present(0, SharpDX.DXGI.PresentFlags.None);
            });

            swapChain.Dispose();
            context.Dispose();
            device.Dispose();

            cameraBuffer.Dispose();
            faceIndexBuffer.Dispose();
            faceVertexBuffer.Dispose();

            provider.Dispose();
            pixelShader.Dispose();
            vertexShader.Dispose();

            hdFaceProcessor.Dispose();
            sensor.Close();
        }
    }
}
