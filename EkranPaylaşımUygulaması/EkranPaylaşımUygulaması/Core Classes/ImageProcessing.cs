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

class ImageProcessing
{
    public  static byte[] GetImageBytes()
    {
        var originalImage = GetScreenShot();
        //Image<Bgr, byte> img = new Image<Bgr, byte>(originalImage);
        //var resizedImage= img.Resize(0.5, Emgu.CV.CvEnum.Inter.Linear);
        return ImageToByteArray(originalImage);
    }
    private static Bitmap GetScreenShot()
    {
        try

        {
            Rectangle bounds= Screen.PrimaryScreen.Bounds;
            CursorPosition = Cursor.Position;
            bounds.Width = 1920;
            bounds.Height = 1080;
            var result = new Bitmap(bounds.Width, bounds.Height);
            using (var g = Graphics.FromImage(result))
            {
                g.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);
            }
            return result;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ScreenShot Error: "+ex.Message);

        }
        return null;
    }
    public  static Point CursorPosition
    {
        get;
        protected set;
    }
    public static byte[] ImageToByteArray(Image img)
    {
        using (var stream = new MemoryStream())
        {
            img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            return stream.ToArray();
        }
    }
    public static Bitmap GetImage(byte[] imageBytes)
    {
        Bitmap bmp;
        using (var ms = new MemoryStream(imageBytes))
        {
            bmp = new Bitmap(ms);
        }
        return bmp;
    }
}
