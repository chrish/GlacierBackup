using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;

namespace GlacierBackup
{

    /// <summary>
    /// Receives a list of all files found during a scan. Will check the history and remove any files unchanged since last backup. 
    /// Will then create a tgz-file with the remainder of the files. 
    /// </summary>
    class Archiver
    {
        private HistoryTracker history;

        public Archiver(Dictionary<string, DateTime> fsFiles)
        {
            this.history = new HistoryTracker(Program.ARCHIVELOCATION);

            Dictionary<string, DateTime> filesToArchive = this.fileSorter(fsFiles);

            this.archiveFiles(filesToArchive);
        }

        /// <summary>
        /// Sorts through the files found during a scan, returning only new files or files 
        /// changed since last backup. 
        /// </summary>
        /// <param name="fsFiles"></param>
        /// <returns></returns>
        protected Dictionary<string, DateTime> fileSorter(Dictionary<string, DateTime> fsFiles)
        {
            Dictionary<string, DateTime> filesToArchive = new Dictionary<string, DateTime>();

            foreach (KeyValuePair<string, DateTime> fsFile in fsFiles)
            {
                string filename = @fsFile.Key;
                if (this.history.hasFile(filename))
                {
                    if (this.history.hasNewerFile(filename, fsFile.Value))
                    {
                        filesToArchive.Add(fsFile.Key, fsFile.Value);
                    }
                }
                else
                {
                    filesToArchive.Add(fsFile.Key, fsFile.Value);
                }
            }

            return filesToArchive;
        }

        protected void archiveFiles(Dictionary<string, DateTime> files)
        {
            if (files.Count > 0)
            {

                
                Stream tarStream = null;
                Session session = new Session();

                string tarFile = Program.TEMPLOCATION + Path.DirectorySeparatorChar + "session-" +
                                 session.getSessionDateTime().Replace(':', '.') +
                                 ".tar";


                try
                {
                    tarStream = File.Create(tarFile);
                }
                catch (NotSupportedException e)
                {
                    Console.WriteLine("ERROR - Path not supported: " + tarFile);
                }

                TarOutputStream archiveStream = new TarOutputStream(tarStream);

                foreach (var file in files)
                {
                    // We won't bother adding empty directories to the archive...
                    if (File.Exists(file.Key))
                    {
                        addFileToStream(archiveStream, file.Key);
                        session.addFile(file.Key, file.Value);
                    }
                    else
                    {
                        // Empty dir or unreadable file
                    }
                    archiveStream.CloseEntry();
                }

                archiveStream.Close();

                this.history.addSession(session);
                this.history.saveHistory();
            }
        }

        protected void addFileToStream(TarOutputStream archiveStream, string file )
        {
            using (Stream inputStream = File.OpenRead(file))
            {
                string filename = file.Substring(3);
                long size = inputStream.Length;
                TarEntry entry = TarEntry.CreateEntryFromFile(file);
                entry.Size = size;

                archiveStream.PutNextEntry(entry);

                // this is copied from TarArchive.WriteEntryCore
                byte[] localBuffer = new byte[32 * 1024];
                while (true)
                {
                    int numRead = inputStream.Read(localBuffer, 0, localBuffer.Length);
                    if (numRead <= 0)
                    {
                        break;
                    }
                    archiveStream.Write(localBuffer, 0, numRead);
                }
            }

        }
    }
}
