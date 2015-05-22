using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace GlacierBackup
{

    /// <summary>
    /// </summary>
    class HistoryTracker
    {
        private List<Session> history;

        #region Constructors

        public HistoryTracker()
        {
            this.history = new List<Session>();
        }

        public HistoryTracker(string historyFile)
        {
            this.history = new List<Session>();

            // Load
            if (File.Exists(historyFile))
            {
                this.getHistoryFromFile(historyFile);
            }
            
            // Simple fix in cases where the history is valid xml but empty
            if (this.history == null)
            {
                this.history = new List<Session>();
            }
        }

        #endregion

        #region LoadHistory
        protected void getHistoryFromFile(string file)
        {
            /* Sample sessions.xml
             
                <sessions>
                    <session sessionTime="2015-01-14T16:04:38+01:00">
                        <file fullpath="c:\path\to\file.txt" lastModified="2015-01-14T16:04:38+01:00" />
                        <file fullpath="c:\path\to\another\file.txt" lastModified="2015-01-14T16:04:38+01:00" />
                    </session>
                </sessions>
             */

            XDocument xml = null;
            try
            {
                xml = XDocument.Load(file);


            }
            catch (Exception e)
            {
                Environment.Exit(0);
            }

            if (xml != null)
            {
                foreach (XElement session in xml.Root.Elements())
                {
                    DateTime sessionTime = Convert.ToDateTime(session.Attribute("sessionTime").Value);
                    Dictionary<string, DateTime> files = new Dictionary<string, DateTime>();

                    foreach (XElement f in session.Descendants())
                    {
                        //string fullPath = f.Attribute("fullPath").Value;
                        FileInfo fullPath = new FileInfo(f.Attribute("fullPath").Value);
                        DateTime lastModified = Convert.ToDateTime(f.Attribute("lastModified").Value);

                        files.Add(fullPath.FullName, lastModified);
                    }

                    Session s = new Session(sessionTime, files);
                    this.history.Add(s);
                }
            }
        }

        public void addSession(Session s)
        {
            this.history.Add(s);
        }

        public void saveHistory()
        {
            XDocument xml = new XDocument();

            XElement root = new XElement("Sessions");

            foreach (var session in this.history)
            {
                if (!session.isEmpty())
                {
                    root.Add(session.toXml());
                }
            }

            xml.Add(root);

            string fileName = "history.xml";

            if (File.Exists(Program.HISTORYLOCATION + Path.DirectorySeparatorChar + fileName))
            {
                if (File.Exists(Program.HISTORYLOCATION + Path.DirectorySeparatorChar + fileName.Replace(".xml", ".old.xml")))
                {
                    File.Delete(Program.HISTORYLOCATION + Path.DirectorySeparatorChar + fileName.Replace(".xml", ".old.xml"));
                }

                File.Move(Program.HISTORYLOCATION + Path.DirectorySeparatorChar + fileName, Program.HISTORYLOCATION + Path.DirectorySeparatorChar + fileName.Replace(".xml", ".old.xml"));
            }
            
            xml.Save(Program.HISTORYLOCATION + Path.DirectorySeparatorChar + fileName);
        }

        #endregion

        #region FindFiles

        /// <summary>
        /// Checks if a file exists in a previous backup. Returns true if it does. 
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public bool hasFile(string file)
        {
            try
            {
                // Should check all sessions to see if a file exists.
                var files = from sessions in this.history
                            where sessions.hasFile(file)
                            select true;
                
                if (files.Any())
                {
                    return true;
                }
            }
            catch (ArgumentNullException e)
            {
                // Might be an issue with a historyfile having a sessions-node but no sessions
                return false;
            }
            

            return false;
        }


        /// <summary>
        /// Check if we have file in our history; if found check if last modified 
        /// in the FS is newer than what we have in the history. 
        /// </summary>
        /// <param name="file"></param>
        /// <param name="foundTime"></param>
        /// <returns></returns>
        public bool hasNewerFile(string file, DateTime foundTime)
        {

            var foundTimes = from sessions in this.history
                where sessions.hasFile(file)
                select sessions.getLastModified(file);

            if (foundTimes.Any())
            {
                DateTime lastBackup = foundTimes.Max();

                if (lastBackup < foundTime)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}
