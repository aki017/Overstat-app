using OpenCvSharp;

namespace Overstat.Capture
{
  public interface ICapture
  {
    string ApiName();
    Mat GetCapture(int x = 0, int y = 0, int width = 1920, int height = 1080);
  }
}