namespace Compendium.Directory
{
    public interface IDesktopIniDistributor
    {
        string GetDesktopIniContent(string directory);
        void SaveDesktopIniToDirectory(string directory);
    }
}