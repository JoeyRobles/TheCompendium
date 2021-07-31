namespace Compendium.IconBuilders
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class IconBuilder
    {
        private readonly Bitmap folderIcon;

        public IconBuilder(Bitmap folderIcon)
        {
            this.folderIcon = folderIcon;
        }

        public Bitmap DrawIcon(Bitmap imageLayer, Color colorScheme)
        {
            var result = new Bitmap(256, 256, PixelFormat.Format32bppArgb);
            var folderIconCopy = folderIcon.Clone(new Rectangle(0, 0, result.Width, result.Height), PixelFormat.Format32bppArgb);
            Graphics g = Graphics.FromImage(result);

            g.DrawImage(imageLayer, 0, 0, result.Width, result.Height);
            g.CompositingMode = CompositingMode.SourceOver;
            g.DrawImage(folderIconCopy, 0, 0, result.Width, result.Height);
            var transparentWhite = Color.FromArgb(0, 255, 255, 255);
            var transparentColorSource = Color.FromArgb(0, 100, 100);
            var schemeColorSource = Color.FromArgb(100, 100, 0);
            result = ReplaceColors(result, new List<Color>() { transparentColorSource, schemeColorSource}, new List<Color>() { Color.Transparent, colorScheme });
            g.Flush();
            g.Save();

            //result.Save("temp.ico", ImageFormat.Icon);
            /*
            var stream = new MemoryStream();
            using(FileStream fs = new FileStream("temp.ico", FileMode.Truncate))
            {
                fs.CopyTo(stream);
                stream.Position = 0;
                IconWriter.Write(stream, new List<Image> { result });
            }*/
            result.MakeTransparent();
            this.ConvertToIco(result, "temp.ico", 256);
            g.Dispose();
            return result;
        }

        private Bitmap ReplaceColors(Bitmap source, List<Color> oldColors, List<Color> newColors)
        {
            for (int x = 0; x < source.Width; x++)
            {
                for (int y = 0; y < source.Height; y++)
                {
                    var curColor = source.GetPixel(x, y);
                    for (int i = 0; i < oldColors.Count; i++)
                    {
                        var oldColor = oldColors[i];
                        var newColor = newColors[i];
                        if (curColor.R == oldColor.R && curColor.G == oldColor.G && curColor.B == oldColor.B)
                        {
                            var transparency = oldColor.A < 255 ? 0 : oldColor.A;
                            var color = Color.FromArgb(transparency, newColor.R, newColor.G, newColor.B);
                            source.SetPixel(x, y, color);
                            break;
                        }
                    }
                }
            }
            return source;
        }

        private string debugBase64Image(Bitmap image)
        {
            System.IO.MemoryStream stream = new System.IO.MemoryStream();
            image.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);
            byte[] bytes = stream.ToArray();
            var base64string = System.Convert.ToBase64String(bytes);
            return base64string;
        }

        public void ConvertToIco(Image img, string file, int size)
        {
            Icon icon;
            using (var msImg = new MemoryStream())
            using (var msIco = new MemoryStream())
            {
                img.Save(msImg, ImageFormat.Png);
                using (var bw = new BinaryWriter(msIco))
                {
                    bw.Write((short)0);           //0-1 reserved
                    bw.Write((short)1);           //2-3 image type, 1 = icon, 2 = cursor
                    bw.Write((short)1);           //4-5 number of images
                    bw.Write((byte)size);         //6 image width
                    bw.Write((byte)size);         //7 image height
                    bw.Write((byte)0);            //8 number of colors
                    bw.Write((byte)0);            //9 reserved
                    bw.Write((short)0);           //10-11 color planes
                    bw.Write((short)32);          //12-13 bits per pixel
                    bw.Write((int)msImg.Length);  //14-17 size of image data
                    bw.Write(22);                 //18-21 offset of image data
                    bw.Write(msImg.ToArray());    // write image data
                    bw.Flush();
                    bw.Seek(0, SeekOrigin.Begin);
                    icon = new Icon(msIco);
                }
            }
            using (var fs = new FileStream(file, FileMode.Create, FileAccess.Write))
            {
                icon.Save(fs);
            }
        }
    }
}
