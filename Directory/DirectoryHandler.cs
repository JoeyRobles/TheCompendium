namespace Compendium.Directory
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class DirectoryHandler
    {
        public IDirectoryLister list { get; set; }

        public IDesktopIniDistributor iniDistributor { get; set; }

        public DirectoryHandler(IDirectoryLister directoryLister, IDesktopIniDistributor iniDistributor)
        {
            this.list = directoryLister;
            this.iniDistributor = iniDistributor;
        }
    }
}
