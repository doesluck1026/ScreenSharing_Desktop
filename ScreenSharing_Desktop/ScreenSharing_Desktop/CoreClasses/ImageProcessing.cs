﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using ScreenCapturerNS;
class ImageProcessing
{
    #region Variables
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
    public static int Rotation = 0;

    public static ImageFormat Image_Format = ImageFormat.Jpeg;
    public static void StartScreenCapturer()
    {
        ScreenCapturer.OnScreenUpdated += ScreenCapturer_OnScreenUpdated;
        ScreenCapturer.OnCaptureStop += ScreenCapturer_OnCaptureStop;
        ScreenCapturer.StartCapture();
        ScreenCapturer.SkipFirstFrame = true;
        System.Threading.Thread.Sleep(500);
    }

    private static void ScreenCapturer_OnCaptureStop(object sender, OnCaptureStopEventArgs e)
    {
        ScreenCapturer.StartCapture();
    }

    private static void ScreenCapturer_OnScreenUpdated(object sender, OnScreenUpdatedEventArgs e)
    {
        lock (Lck_ScreenImage)
            _screenImage = e.Bitmap.Clone(new Rectangle(0,0,e.Bitmap.Size.Width, e.Bitmap.Size.Height),PixelFormat.DontCare);
    }

    public static void StopScreenCapturer()
    {
        Stopwatch stp = Stopwatch.StartNew();
        while (ScreenCapturer.IsActive)
        {
            ScreenCapturer.StopCapture();
            System.Threading.Thread.Sleep(50);
            if (stp.ElapsedMilliseconds > 500)
                break;
        }
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
        Bitmap img = GetScreenShot();
        if (img == null)
            return null;
        
        DrawPointToImage(ref img);
        byte[] imageBytes;
        imageBytes = ImageToByteArray(img);
        return imageBytes;
    }
    private static void DrawPointToImage(ref Bitmap bitmap)
    {
        int pointHalfWidth =6;
        for(int i=-pointHalfWidth; i< pointHalfWidth; i++)
        {
            for (int j = -pointHalfWidth; j < pointHalfWidth; j++)
            {
                int posx = Math.Min(Math.Max(CursorPosition.X + i, 0), bitmap.Size.Width - 1);
                int posy = Math.Min(Math.Max(CursorPosition.Y + j, 0), bitmap.Size.Height - 1);
                bitmap.SetPixel(posx, posy, Color.DarkGray);
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

    /// <summary>
    /// Converts Bitmap Image to Byte array using given format
    /// </summary>
    /// <param name="img"></param>
    /// <returns>Byte array that contains image bytes</returns>
    public static byte[] ImageToByteArray(Bitmap img)
    {
        if (img == null)
            return null;
        try
        {
            using (var stream = new MemoryStream())
            {
                img.Save(stream, Image_Format);
                return stream.ToArray();
            }
        }
        catch
        {
            return null;
        }
    }
    /// <summary>
    /// Converts Given byte array that contains image bytes, to an image
    /// </summary>
    /// <param name="imageBytes"></param>
    /// <returns></returns>
    public static Bitmap ImageFromByteArray(byte[] imageBytes)
    {
        Bitmap bmp;
        using (var ms = new MemoryStream(imageBytes))
        {
            bmp = new Bitmap(ms);
        }
        if (Rotation == 90)
            bmp.RotateFlip(RotateFlipType.Rotate90FlipNone);
        else if (Rotation == 180)
            bmp.RotateFlip(RotateFlipType.Rotate180FlipNone);
        else if (Rotation == 270)
            bmp.RotateFlip(RotateFlipType.Rotate270FlipNone);
        return bmp;
    }
}
