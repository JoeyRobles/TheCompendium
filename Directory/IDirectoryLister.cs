namespace Compendium.Directory
{
    using System.Collections.Generic;
    using System.Windows.Media.Imaging;
    using Compendium.Models;

    public interface IDirectoryLister
    {
        List<FileGridItem> items { get; }

        void ReloadImages();

        void SetItems(List<FileGridItem> items);
    }
}