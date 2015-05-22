using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.GZip;

namespace GlacierBackup
{
    class Program
    {
        /*
            -Traverse given path recursively
            -Log all directories and files
            -Create a tar file in a new location
            -Add all listed files
            -gzip tar once completed
            -Log all files
            -Upload to destination
         */

        public static string TEMPLOCATION = @"C:\temp\_glacierbackups";
        public static string HISTORYLOCATION = TEMPLOCATION;
        public static string ARCHIVELOCATION = TEMPLOCATION + Path.DirectorySeparatorChar + "history.xml";

        public static HistoryTracker HISTORYTRACKER;

        static void Main(string[] args)
        {
            Program p = new Program();
        }

        public Program()
        {
            FileScanner fs = new FileScanner(@"C:\temp\TagElectrical");
            Archiver ar = new Archiver(fs.fileRegistry);

        }
    }
}
