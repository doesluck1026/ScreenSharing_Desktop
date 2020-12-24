using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;

class Screen
{
    public  static byte[] GetImageBytes()
    {
        var originalImage = GetScreenShot();
        return ImageToByteArray(originalImage);
    }
    private static Bitmap GetScreenShot()
    {
        try

        {
            Bitmap bmp = new Bitmap(System.Windows.Forms.Screen.AllScreens[0].Bounds.Width, System.Windows.Forms.Screen.AllScreens[0].Bounds.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(0, 0, 0, 0, System.Windows.Forms.Screen.AllScreens[0].Bounds.Size);
                bmp.Save("C:\\Users\\CDS_Software02\\Desktop\\screenshot.png");  // saves the image
            }
            return bmp;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ScreenShot Error: "+ex.Message);

        }
        return null;
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
