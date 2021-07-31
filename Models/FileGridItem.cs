namespace Compendium.Models
{
    using System.Windows.Media.Imaging;

    public class FileGridItem
    {
        public int Index { get; set; }

        public string ImageFile { get; set; }

        public string Title { get; set; }
        
        public BitmapImage Image { get; set; }
    }
}
