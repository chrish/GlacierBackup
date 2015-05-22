using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace GlacierBackup
{

    /// <summary>
    /// Represents a scanning session. Will store the time of the session as DateTime.ToString()
    /// </summary>
    class Session
    {
        private DateTimeOffset _sessionDateTime;
        private Dictionary<string, DateTime> items;

        /// <summary>
        /// New session
        /// </summary>
        public Session()
        {
            this._sessionDateTime = DateTimeOffset.Now;
            this.items = new Dictionary<string, DateTime>();
        }

        public string getSessionDateTime() {
                string format = "yyyy-MM-ddTHH:mm:s K";
                return this._sessionDateTime.ToString(format);
        }


        /// <summary>
        /// Load existing session
        /// </summary>
        /// <param name="when"></param>
        /// <param name="files"></param>
        public Session(DateTimeOffset when, Dictionary<string, DateTime> files)
        {
            this._sessionDateTime = when;
            this.items = files;
        }

        public void addFile(string path, DateTime lastModified)
        {
            this.items.Add(path, lastModified);
        }

        public Boolean isEmpty()
        {
            if (this.items.Count == 0)
            {
                return true;
            }
            return false;
        }

        public bool hasFile(string file)
        {
            var files = from f in this.items
                where f.Key.Equals(file)
                select f;

            if (files.Any())
            {
                return true;
            }

            return false;
        }

        public DateTime getLastModified(string file)
        {
            var modified = from item in this.items
                where item.Key.Equals(file)
                select item.Value;

            if (modified.Any())
            {
                DateTime lastModified = Convert.ToDateTime(modified.First());
                return lastModified;
            }

            return new DateTime(9999); // Should indicate error
        }

        public XElement toXml()
        {
            XElement session = new XElement("session");
            XAttribute sessionTime = new XAttribute("sessionTime", this.getSessionDateTime());

            session.Add(sessionTime);

            foreach (KeyValuePair<string, DateTime> k in this.items)
            {
                XElement file = new XElement("file");
                file.Add(new XAttribute("fullPath", k.Key));
                file.Add(new XAttribute("lastModified", k.Value));

                session.Add(file);
            }

            return session;
        }
    }
}
