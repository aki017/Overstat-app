using System;
using OpenCvSharp;
using Overstat.Properties;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;
using MapFlags = SharpDX.DXGI.MapFlags;
using Resource = SharpDX.DXGI.Resource;
using ResultCode = SharpDX.DXGI.ResultCode;

namespace Overstat.Capture
{
  // ReSharper disable once InconsistentNaming
  public class DXGICapture : ICapture, IDisposable
  {
    public string ApiName()
    {
      return "Desktop Duplication API";
    }

    public uint AdapterID
    {
      get { return Settings.Default.AdapterID; }
      set
      {
        Settings.Default.AdapterID = value;
        Settings.Default.Save();
        ChangeOutput();
      }
    }

    public uint OutputID
    {
      get { return Settings.Default.OutputID; }
      set
      {
        Settings.Default.OutputID = value;
        Settings.Default.Save();
        ChangeOutput();
      }
    }
    private readonly Device device;
    private readonly Factory1 factory;
    private readonly Texture2DDescription texdes;
    private OutputDuplication duplicatedOutput;

    public Adapter1[] Adapters => factory.Adapters1;
    public Output[] Outputs => factory.Adapters1[AdapterID].Outputs;

    public DXGICapture()
    {
      device = new Device(DriverType.Hardware);
      factory = new Factory1();

      texdes = new Texture2DDescription();
      texdes.CpuAccessFlags = CpuAccessFlags.Read;
      texdes.BindFlags = BindFlags.None;
      texdes.Format = Format.B8G8R8A8_UNorm;
      texdes.Height = 1080;
      texdes.Width = 1920;
      texdes.OptionFlags = ResourceOptionFlags.None;
      texdes.MipLevels = 1;
      texdes.ArraySize = 1;
      texdes.SampleDescription.Count = 1;
      texdes.SampleDescription.Quality = 0;
      texdes.Usage = ResourceUsage.Staging;
      ChangeOutput();
    }

    private void ChangeOutput()
    {
      if (Adapters[AdapterID].Outputs.Length <= OutputID)
      {
        if (Adapters[AdapterID].Outputs.Length == 0)
        {
          AdapterID = 0;
        }
        OutputID = 0;
      }
      var output = new Output1(Adapters[AdapterID].Outputs[OutputID].NativePointer);
      duplicatedOutput?.Dispose();
      try
      {
        duplicatedOutput = output.DuplicateOutput(device);
      }
      catch
      {
        duplicatedOutput = null;
      }
    }

    private Mat CopyImageToMat(int width, int height, DataStream stream)
    {
      using (var mat = new Mat(height, width, MatType.CV_8UC4))
      {
        stream.Read(mat.Data, 0, width * height * 4);
        return mat.CvtColor(ColorConversionCodes.BGRA2BGR);
      }
    }

    public Mat GetCapture(int x = 0, int y = 0, int width = 1920, int height = 1080)
    {
      if (duplicatedOutput == null) return null;
      using (var screenTexture = new Texture2D(device, texdes))
      {
        Resource screenResource = null;
        DataStream dataStream;
        Surface screenSurface;


        OutputDuplicateFrameInformation duplicateFrameInformation;
        try
        {
          duplicatedOutput.AcquireNextFrame(1000, out duplicateFrameInformation, out screenResource);
        }
        catch (SharpDXException ee)
        {
          if (ee.ResultCode.Code == ResultCode.WaitTimeout.Result.Code)
          {
            return GetCapture(x, y, width, height);
          }
          else if (ee.ResultCode.Code == ResultCode.AccessLost.Result.Code)
          {
            return GetCapture(x, y, width, height);
          }
          else
          {
            throw ee;
          }
        }

        device.ImmediateContext.CopyResource(screenResource.QueryInterface<SharpDX.Direct3D11.Resource>(), screenTexture);
        screenSurface = screenTexture.QueryInterface<Surface>();
        screenSurface.Map(MapFlags.Read, out dataStream);
        using (var a = CopyImageToMat(1920, 1080, dataStream))
        {
          dataStream.Close();
          screenSurface.Unmap();
          screenSurface.Dispose();
          screenResource.Dispose();
          try
          {
            duplicatedOutput.ReleaseFrame();
          }
          catch
          {
          }
          using (var b = a.ColRange(x, x + width))
          {
            return b.RowRange(y, y + height);
          }
        }
      }
    }

    public void Dispose()
    {
      device?.Dispose();
      factory?.Dispose();
      duplicatedOutput?.Dispose();
    }
  }
}
