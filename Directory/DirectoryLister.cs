namespace Compendium.Directory
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Compendium.Models;

    public class DirectoryLister : IDirectoryLister
    {
        public List<FileGridItem> items { get; private set; }

        public DirectoryLister()
        {
            items = new List<FileGridItem>();
        }

        public void SetItems(List<FileGridItem> items)
        {
            this.items = items;
            ReloadImages();
        }

        public void ReloadImages()
        {
            loadBitMaps();
        }

        private void loadBitMaps()
        {
            foreach (var item in items)
            {
                if(File.Exists(item.ImageFile))
                {
                    item.Image = toBitmap(File.ReadAllBytes(item.ImageFile));
                }
                else
                {
                    var bitmap = new Bitmap(128, 128);
                    using (Graphics graph = Graphics.FromImage(bitmap))
                    {
                        Rectangle ImageSize = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
                        graph.FillRectangle(System.Drawing.Brushes.Black, ImageSize);
                    }
                    item.Image = Bitmap2BitmapImage(bitmap);
                }
            }
        }
        public static BitmapImage Bitmap2BitmapImage(Bitmap bitmap)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                bitmap.Save(ms, ImageFormat.Png);
                ms.Position = 0;
                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.StreamSource = ms;
                bi.EndInit();

                return bi;
            }
        }

        public static BitmapImage toBitmap(byte[] value)
        {
            if (value != null && value is byte[])
            {
                byte[] ByteArray = value as byte[];
                BitmapImage bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.StreamSource = new MemoryStream(ByteArray);
                bmp.EndInit();
                return bmp;
            }
            return null;
        }
    }
}
