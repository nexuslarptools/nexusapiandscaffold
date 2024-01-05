using System.Drawing.Imaging;
using System;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Linq;

namespace NEXUSDataLayerScaffold.Logic
{
    public class ImageLogic
    {
        public static void ResizeJpg(string path, bool isMainPic)
        { 
            int nWidth = 375;
            int nHeight = 250;

            if (!isMainPic)
            {
                nWidth = 275;
                nHeight = 550;
            }

            using (var result = new Bitmap(nWidth, nHeight))
            {
                using (var input = new Bitmap(path))
                {
                    using (Graphics g = Graphics.FromImage((System.Drawing.Image)result))
                    {
                        g.DrawImage(input, 0, 0, nWidth, nHeight);
                    }
                }

                var ici = ImageCodecInfo.GetImageEncoders().FirstOrDefault(ie => ie.MimeType == "image/jpeg");
                var eps = new EncoderParameters(1);
                eps.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 100L);
                result.Save(path, ici, eps);
            }
        }

    }
}
