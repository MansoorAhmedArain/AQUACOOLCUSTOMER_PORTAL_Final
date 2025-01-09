using System;
using System.Configuration;
using System.IO;

namespace AQUACOOLCUSTOMER_PORTAL.Models
{
    public class Log
    {
        public static void WriteLine(string message)
        {
           // File.AppendAllText(ConfigurationManager.AppSettings["LogPath"], message);
        }
    }
}
