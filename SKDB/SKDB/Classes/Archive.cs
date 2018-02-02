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
        public bool CreateArchiveFromFolder(string folderPath, string outputdir, string archivename)
        {
            SevenZipCompressor compressor = new SevenZipCompressor();

            try
            {
                if (File.Exists(outputdir + archivename + ".7z"))
                {
                    File.Delete(outputdir + archivename + ".7z");
                }

                compressor.CompressionMode = CompressionMode.Create;
                compressor.CompressionLevel = CompressionLevel.Normal;
                compressor.CustomParameters.Add("d", "22");
                compressor.ArchiveFormat = OutArchiveFormat.SevenZip;
                compressor.PreserveDirectoryRoot = true;
                compressor.CompressDirectory(folderPath, outputdir + archivename + ".7z");

                return true;
            }
            catch(Exception ex)
            {
                string exc = ex.ToString();
            }

            return false;
        }

        public CompressionResults ProcessArchive(string ArchivePath)
        {
            var path = Path.GetFullPath(ArchivePath);

            CompressionResults crs = new CompressionResults(path);

            // if file does not exist
            if (!File.Exists(path))
                return null;

            // mount the archive

            try
            {
                using (Stream archiveStream = File.OpenRead(path))
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
                            cr.ArchivePath = path;
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


        /// <summary>
        /// Extract a single file from an archive using an archivestream to a byte array
        /// </summary>
        /// <param name="archiveStream"></param>
        /// <param name="internalFileName"></param>
        /// <param name="outputDirectory"></param>
        public static byte[] ExtractFileToByteArray(Stream archiveStream, string internalFileName)
        {
            byte[] content = null; 

            try
            {
                using (SevenZipExtractor extr = new SevenZipExtractor(archiveStream))
                {
                    foreach (ArchiveFileInfo archiveFileInfo in extr.ArchiveFileData.Where(a => a.FileName.Replace("\\", "/") == internalFileName))
                    {
                        if (!archiveFileInfo.IsDirectory)
                        {
                            using (var mem = new MemoryStream())
                            {
                                extr.ExtractFile(archiveFileInfo.Index, mem);

                                string shortFileName = Path.GetFileName(archiveFileInfo.FileName);
                                content = mem.ToArray();
                                //File.WriteAllBytes(outputDirectory + @"\" + shortFileName, content);

                                //return content;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // problem with archive?
                string exceptionMsg = ex.ToString();

            }


            return content;
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
