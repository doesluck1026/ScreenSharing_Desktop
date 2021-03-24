using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV.Structure;
using Emgu.CV;
using ScreenCapturerNS;
class ImageProcessing
{
    #region Variables
    public static int FPS;
    private static double ResizeRatio = 0.7;
    private static Image<Bgr, byte> ScreenImage
    {
        get
        {
            lock (lck_ScreenImage)
                return _screenImage;
        }
        set
        {
            lock (lck_ScreenImage)
                _screenImage = value;
        }
    }
    private static Image<Bgr, byte> _screenImage = null;



    private static object lck_ScreenImage = new object();


    public static void StartGettingFrame()
    {
        ScreenCapturer.OnScreenUpdated += ScreenCapturer_OnScreenUpdated;
        ScreenCapturer.OnCaptureStop += ScreenCapturer_OnCaptureStop;
        ScreenCapturer.StartCapture();
        ScreenCapturer.SkipFirstFrame = true;
    }

    private static void ScreenCapturer_OnCaptureStop(object sender, OnCaptureStopEventArgs e)
    {
      //  ScreenCapturer.StartCapture();
    }

    private static void ScreenCapturer_OnScreenUpdated(object sender, OnScreenUpdatedEventArgs e)
    {
            lock (lck_ScreenImage)
                _screenImage = new Image<Bgr, byte>(e.Bitmap);
        
        // ScreenImage.Save("C:\\Users\\CDS_Software02\\Desktop\\image.jpg");
    }

    public static void StopGettingFrames()
    {
        while (ScreenCapturer.IsActive)
        {
            ScreenCapturer.StopCapture();
            System.Threading.Thread.Sleep(50);
        }
    }
    public static Point CursorPosition
    {
        get;
        protected set;
    }

    #endregion
    private static int ElapsedTime = 0;
    public static byte[] GetScreenBytes()
    {
        Stopwatch stp = Stopwatch.StartNew();
        Image<Bgr, byte> img = GetScreenShot();
        if (img == null)
            throw new Exception("ScreenShotException");
        //double t1 = stp.Elapsed.TotalMilliseconds;
        //Image<Bgr, byte> img = new Image<Bgr, byte>(originalImage);
        img.Draw(new CircleF(new PointF((float)CursorPosition.X, (float)CursorPosition.Y), 8), new Bgr(255, 0, 0), 2);
       // double t2 = stp.Elapsed.TotalMilliseconds;
       // double t3;
        byte[] imageBytes;
        //if (ElapsedTime > 600)
        //{
        //    if (FPS < 26)
        //    {
        //        ElapsedTime = 0;
        //        ResizeRatio = Math.Max(0.5, ResizeRatio * 0.99);
        //    }
        //    else if(FPS > 30)
        //    {
        //        ElapsedTime = 0;
        //        ResizeRatio = Math.Min(1, ResizeRatio / 0.99);
        //    }
        //    Debug.WriteLine("Resizing ratio updated: " + ResizeRatio);

        //}
        //Image<Bgr, byte> resizedImage = img.Resize(1, Emgu.CV.CvEnum.Inter.Linear);

        imageBytes = ImageToByteArray(img.Bitmap);

        //Debug.WriteLine("Resize Ratio: " + ResizeRatio);
        //double t4 = stp.Elapsed.TotalMilliseconds;
        //Debug.WriteLine("  screenShot Time: " + t1 + " ms  drawTime: " + (t2 - t1) + " ms   resizeTime: " + (t3 - t2) + " ms  byte Array Time: " + (t4 - t3) + " ms");
        ElapsedTime += (int)stp.ElapsedMilliseconds;
        return imageBytes;
    }
    private static Image<Bgr, byte> GetScreenShot()
    {
        try
        {
            CursorPosition = Cursor.Position;
            return ScreenImage;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ScreenShot Error: " + ex.Message);
        }
        return null;
    }
    public static byte[] ImageToByteArray(Bitmap img)
    {
        if (img == null)
            return null;
        try
        {
            using (var stream = new MemoryStream())
            {
                img.Save(stream, ImageFormat.Jpeg);
                return stream.ToArray();
            }
        }
        catch
        {
            return null;
        }
    }
    public static Bitmap ImageFromByteArray(byte[] imageBytes)
    {
        Bitmap bmp;
        using (var ms = new MemoryStream(imageBytes))
        {
            bmp = new Bitmap(ms);
        }
        return bmp;
    }
}
