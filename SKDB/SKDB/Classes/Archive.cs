using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SevenZip;
using System.IO;

namespace SKDB.Classes
{
    /// <summary>
    /// Handled archiving operations
    /// </summary>
    public class Archive
    {

        public CompressionResults ProcessArchive(string ArchivePath)
        {
            CompressionResults crs = new CompressionResults(ArchivePath);

            // if file does not exist
            if (!File.Exists(ArchivePath))
                return null;

            // mount the archive

            try
            {
                using (Stream archiveStream = File.OpenRead(ArchivePath))
                {
                    using (var ar = new SevenZipExtractor(archiveStream))
                    {
                        var structure = ar.ArchiveFileData;

                        List<ArchiveFileInfo> allowedFiles = new List<ArchiveFileInfo>();

                        foreach (var s in structure)
                        {
                            if (s.IsDirectory == true)
                                continue;

                            string fileName = s.FileName;
                            string extension = Path.GetExtension(fileName);

                            if (Path.GetExtension(s.FileName).ToLower() == ".xml")
                            {
                                allowedFiles.Add(s);
                                continue;
                            }
                        }

                        // now we should have a list of allowed files
                        foreach (var file in allowedFiles)
                        {
                            CompressionResult cr = new CompressionResult();
                            cr.ArchivePath = ArchivePath;
                            cr.InternalPath = file.FileName.Replace("\\", "/");
                            cr.FileName = cr.InternalPath.Split('/').LastOrDefault();
                            cr.CRC32 = file.Crc.ToString("X");
                            cr.Extension = Path.GetExtension(cr.FileName);
                            cr.RomName = cr.FileName.Replace(cr.Extension, "");
                            
                            // 7zip and zip extraction too slow, use CRC32 instead
                            cr.MD5 = cr.CRC32;

                            crs.Results.Add(cr);

                            // build status message
                            StringBuilder sb1 = new StringBuilder();
                            sb1.Append("Scanning: " + Path.GetFileName(ArchivePath) + "\n\n");
                            sb1.Append("Processing: " + cr.InternalPath);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // problem with archive?
                string exceptionMsg = ex.ToString();
            }



            // var ar = new SevenZipExtractor(ArchivePath);


            return crs;
        }
    }


    public class CompressionResult
    {
        public string FileName { get; set; }        // filename
        public string RomName { get; set; }         // filename without extension
        public string Extension { get; set; }       // just the extension
        public string RelativePath { get; set; }
        public string InternalPath { get; set; }
        public string ArchivePath { get; set; }     // full path to archive
        public string DBPathString { get; set; }
        public string MD5 { get; set; }
        public string CRC32 { get; set; }

        public CompressionResult CalculateDBPathString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(ArchivePath);
            sb.Append("*/");
            sb.Append(InternalPath);
            DBPathString = sb.ToString();

            // work out extension
            string[] dots = FileName.Split('.');
            if (dots.Length > 0)
            {
                string ext = "." + dots.Last();
                Extension = ext;
            }

            // romname (without extension)
            RomName = FileName.Replace(Extension, "");

            return this;
        }
    }

    public class CompressionResults
    {
        public List<CompressionResult> Results { get; set; }
        public string ArchivePath { get; set; }
        public string ArchiveMD5 { get; set; }

        public CompressionResults(string archivePath)
        {
            Results = new List<CompressionResult>();
            ArchivePath = archivePath;
            //ArchiveMD5 = Crypto.Converters.GetMD5Hash(ArchivePath);
        }
    }
}
