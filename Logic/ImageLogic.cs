using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace NEXUSDataLayerScaffold.Logic;

public class ImageLogic
{
    public static void ResizeJpg(string path, bool isMainPic)
    {
        var nWidth = 375;
        var nHeight = 250;

        if (!isMainPic)
        {
            nWidth = 275;
            nHeight = 550;
        }

        using (var image = Image.Load(path))
        {
            // Resize the image in place and return it for chaining.
            // 'x' signifies the current image processing context.
            image.Mutate(x => x.Resize(nWidth, nHeight));

            // The library automatically picks an encoder based on the file extension then
            // encodes and write the data to disk.
            // You can optionally set the encoder to choose.
            image.Save(path);
        }
    }
}