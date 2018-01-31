using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SKDB.Classes
{
    /// <summary>
    /// DAT file related functions
    /// </summary>
    public class DATFile
    {
        public static string DatFolder = AppDomain.CurrentDomain.BaseDirectory + @"..\..\SourceDATs";

        public static string[] GetDatFiles()
        {
            var files = Directory.GetFiles(DatFolder).Where(a => Path.GetExtension(a).ToLower() == ".7z");
            return files.ToArray();
        }
    }
}
