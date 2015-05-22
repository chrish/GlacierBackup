using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlacierBackup
{
    class FileScanner
    {
        private Dictionary<string, DateTime> _fileRegistry;

        public Dictionary<string, DateTime> fileRegistry {
            get { return this._fileRegistry; }
        }

        public FileScanner(string directory)
        {
            this._fileRegistry = new Dictionary<string, DateTime>();

            this.getFiles(directory);
        }

        private void getFiles(string path)
        {
            DirectoryInfo di = new DirectoryInfo(path);

            var files = di.GetFiles();
            var dirs = di.GetDirectories();

            foreach (DirectoryInfo dir in dirs)
            {
                this.getFiles(dir.FullName);
            }

            foreach (FileInfo file in files)
            {
                this._fileRegistry.Add(file.FullName, file.LastWriteTime);
            }
        }
    }
}
