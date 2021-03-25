using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using ScreenCapturerNS;
class ImageProcessing
{
    #region Variables
    public static int FPS;
    private static Bitmap ScreenImage
    {
        get
        {
            lock (Lck_ScreenImage)
                return _screenImage;
        }
        set
        {
            lock (Lck_ScreenImage)
                _screenImage = value;
        }
    }
    private static object Lck_ScreenImage = new object();
    private static Bitmap _screenImage;
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
        lock (Lck_ScreenImage)
            _screenImage = e.Bitmap.Clone(new Rectangle(0,0,e.Bitmap.Size.Width, e.Bitmap.Size.Height),PixelFormat.DontCare);
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
        Bitmap img = GetScreenShot();
        if (img == null)
            throw new Exception("ScreenShotException");
        //double t1 = stp.Elapsed.TotalMilliseconds;
        //Image<Bgr, byte> img = new Image<Bgr, byte>(originalImage);
        DrawCircle(ref img);
        byte[] imageBytes;
        imageBytes = ImageToByteArray(img);
        ElapsedTime += (int)stp.ElapsedMilliseconds;
        return imageBytes;
    }
    private static void DrawCircle(ref Bitmap bitmap)
    {
        int pointHalfWidth =6;
        for(int i=-pointHalfWidth; i< pointHalfWidth; i++)
        {
            for (int j = -pointHalfWidth; j < pointHalfWidth; j++)
            {
                int posx = Math.Min(Math.Max(CursorPosition.X + i, 0), bitmap.Size.Width - 1);
                int posy = Math.Min(Math.Max(CursorPosition.Y + j, 0), bitmap.Size.Height - 1);
                bitmap.SetPixel(posx, posy, Color.Blue);
            }

        }
    }
    private static Bitmap GetScreenShot()
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
