namespace Compendium.Directory
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class DesktopIniDistributor : IDesktopIniDistributor
    {
        private readonly string iconName;

        public DesktopIniDistributor(string iconName)
        {
            this.iconName = iconName;
        }

        public void SaveDesktopIniToDirectory(string directory)
        {
            var desktopIniDir = Path.Combine(directory, "desktop.ini");
            File.WriteAllText(desktopIniDir, GetDesktopIniContent(directory), Encoding.Unicode);
            File.SetAttributes(desktopIniDir, File.GetAttributes(desktopIniDir) | FileAttributes.Hidden | FileAttributes.System);
            var di = new DirectoryInfo(directory);
            di.Attributes |= FileAttributes.System;
        }

        public string GetDesktopIniContent(string directory)
        {
            var sb = new StringBuilder();
            var desktopIni = sb.AppendLine("[.ShellClassInfo]")
                .AppendLine($"IconFile={ this.iconName }.ico")
                .AppendLine($"IconIndex=0")
                .AppendLine($"ConfirmFileOp=0")
                .AppendLine($"IconResource={ this.iconName }.ico,0")
                .AppendLine("[ViewState]")
                .AppendLine("Mode=")
                .AppendLine("Vid=")
                .AppendLine("FolderType=Generic")
                .ToString();
            return desktopIni;
        }
    }
}
