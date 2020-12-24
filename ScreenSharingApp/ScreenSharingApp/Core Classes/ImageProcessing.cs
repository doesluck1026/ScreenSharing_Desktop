using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
    private static double ResizeRatio=1;
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
        ScreenCapturer.StartCapture();
        ScreenCapturer.OnScreenUpdated += ScreenCapturer_OnScreenUpdated;
        ScreenCapturer.OnCaptureStop += ScreenCapturer_OnCaptureStop;
    }

    private static void ScreenCapturer_OnCaptureStop(object sender, OnCaptureStopEventArgs e)
    {
       Debug.WriteLine("Exception in capture: "+ e.Exception.ToString());
        ScreenCapturer.StartCapture();
    }

    private static void ScreenCapturer_OnScreenUpdated(object sender, OnScreenUpdatedEventArgs e)
    {
        lock(lck_ScreenImage)
            _screenImage = new Image<Bgr, byte>(e.Bitmap);
        // ScreenImage.Save("C:\\Users\\CDS_Software02\\Desktop\\image.jpg");
    }

    public static void StopGettingFrames()
    {
        ScreenCapturer.StopCapture();
    }
    public static Point CursorPosition
    {
        get;
        protected set;
    }

    #endregion

    public static byte[] GetScreenBytes()
    {
        Stopwatch stp = Stopwatch.StartNew();
        Image<Bgr, byte> img = GetScreenShot();
        //double t1 = stp.Elapsed.TotalMilliseconds;
        //Image<Bgr, byte> img = new Image<Bgr, byte>(originalImage);
        img.Draw(new CircleF(new PointF((float)CursorPosition.X, (float)CursorPosition.Y), 8), new Bgr(255, 0, 0),2);
       // double t2 = stp.Elapsed.TotalMilliseconds;
        //double t3;
        byte[] imageBytes;
        if(FPS<30)
        {
            ResizeRatio = FPS / 30.0;
            if (ResizeRatio == 0)
                ResizeRatio = 0.1;
            var resizedImage = img.Resize(ResizeRatio, Emgu.CV.CvEnum.Inter.Linear);
            //t3 = stp.Elapsed.TotalMilliseconds;
            imageBytes = ImageToByteArray(resizedImage.Bitmap);
        }
        else
        {
            ResizeRatio = 1;
           // t3 = stp.Elapsed.TotalMilliseconds;
            imageBytes = ImageToByteArray(img.Bitmap);
        }
        //Debug.WriteLine("Resize Ratio: " + ResizeRatio);
        double t4 = stp.Elapsed.TotalMilliseconds;
       // Debug.WriteLine("  screenShot Time: " + t1 +" ms  drawTime: " + (t2 - t1) + " ms   resizeTime: " + (t3 - t2) + " ms  byte Array Time: " + (t4 - t3) + " ms");
        return imageBytes;
    }
    private static Image<Bgr,byte> GetScreenShot()
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
        using (var stream = new MemoryStream())
        {
            img.Save(stream, ImageFormat.Jpeg);
            //var stream2 = new MemoryStream();
            //var stream3 = new MemoryStream();
            //img.Save(stream2, ImageFormat.Jpeg);
            //img.Save(stream3, ImageFormat.Bmp);
            //Debug.WriteLine("png_len: " + stream.ToArray().Length + " jpegLen: " + stream2.ToArray().Length + " bmp_len: " + stream3.ToArray().Length);
            return stream.ToArray();
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
