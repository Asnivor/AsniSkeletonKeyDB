using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.IO;

using SKDB.Classes;

namespace SKDB
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        TextBoxOutputter outputter;

        public List<string> DetectedTags { get; set; }        

        public MainWindow()
        {
            InitializeComponent();

            DetectedTags = new List<string>();

            outputter = new TextBoxOutputter(TestBox);
            Console.SetOut(outputter);
            Console.WriteLine("Started");

            //var timer1 = new Timer(TimerTick, "Timer1", 0, 1000);
            //var timer2 = new Timer(TimerTick, "Timer2", 0, 500);
        }

        void TimerTick(object state)
        {
            var who = state as string;
            Console.WriteLine(who);
        }

        /// <summary>
        /// Application init
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Starting up...");
            Console.WriteLine("Scanning for existing SQLite database");
        }

        /// <summary>
        /// Prints current system db entries
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnEnumDbSystems_Click(object sender, RoutedEventArgs e)
        {
            var systems = SkeletonDB.GetSystems();

            Console.WriteLine("Retrieving systems from the database - Count: " + systems.Count());
            Console.WriteLine("Enumerating.....");
            foreach (var sys in systems)
            {
                Console.WriteLine(sys.pid.ToString() + "\t:\t" + sys.name);
            }
            Console.WriteLine("------------Done------------");
        }

        /// <summary>
        /// Prints current source DAT files
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnEnumDatFiles_Click(object sender, RoutedEventArgs e)
        {
            var dats = DATFile.GetDatFiles();

            Console.WriteLine("Retrieving local DAT file archives - Count: " + dats.Count());
            Console.WriteLine("Enumerating.....");
            foreach (var dat in dats)
            {
                Console.WriteLine(Path.GetFileName(dat));                
            }
            Console.WriteLine("------------Done------------");
        }

        /// <summary>
        /// Processes DAT files from disk (from archives)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnProcessDATs_Click(object sender, RoutedEventArgs e)
        {
            var dats = DATFile.GetDatFiles();
            Console.WriteLine("Retrieving local DAT file archives - Count: " + dats.Count());
            int counter = 0;

            // setup output directory
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string exportedDir = "C:\\skeletonKey\\ExportedDats\\";
            string baseExportedDir = "rj\\scrapeart\\";
            string rootPath = exportedDir + baseExportedDir;
            Directory.CreateDirectory(rootPath);

            foreach (var dat in dats)//.Where(a => a.ToLower().Contains("lynx")))
            {
                // create dir for DAT
                string datExportDir = rootPath + Path.GetFileNameWithoutExtension(dat);
                Directory.CreateDirectory(datExportDir);

                counter++;
                Console.WriteLine("Processing " + System.IO.Path.GetFileName(dat) + " (" + counter + " of " + dats.Count() + ")");
                Archive arch = new Archive();
                Console.WriteLine("(Exporting to " + datExportDir + ")");

                await Task.Run(() =>
                {
                    var results = arch.ProcessArchive(dat);
                    var rCount = results.Results.Count();
                    var percentage = rCount / 50;
                    int cnt = 0;
                    int cnter = 0;

                    // process each archived file
                    foreach (var file in results.Results)
                    {
                        string finalXml = string.Empty;
                        using (Stream archiveStream = File.OpenRead(dat))
                        {
                            byte[] fData = Archive.ExtractFileToByteArray(archiveStream, file.InternalPath);
                            var str = Encoding.ASCII.GetString(fData);
                            string fix = XML.FixXml(str, this);
                            finalXml = fix;
                        }
                        cnt++;


                        // write file to disk
                        File.WriteAllText(datExportDir + "\\" + file.FileName, finalXml);

                        // percentage complete
                        int currPos = Convert.ToInt32((double)(((double)cnt / (double)rCount) * 100));

                        if (currPos > cnter)
                        {
                            Console.Write(".");
                            cnter = currPos;
                        }
                    }
                    Console.Write("\n");

                    foreach (var res in results.Results)
                    {
                        //Console.WriteLine(res.InternalPath);
                    }
                });

                
            }
            Console.WriteLine("------------Done------------");
        }

        /// <summary>
        /// Scans the DAT archives returning the different system names
        /// then imports them to the database
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnImportSystems_Click(object sender, RoutedEventArgs e)
        {
            var dats = DATFile.GetDatFiles();

            Console.WriteLine("Retrieving local DAT file archives - Count: " + dats.Count());
            Console.WriteLine("Enumerating.....");

            List<string> sysNames = new List<string>();

            foreach (var dat in dats)
            {
                string dName = Path.GetFileNameWithoutExtension(dat);
                sysNames.Add(dName);
                Console.WriteLine(dName);
            }

            Console.WriteLine("Adding/updating systems in the database...");

            Database.DBFunctions.AddUpdateSystems(sysNames);

            Console.WriteLine("...Done");

            Console.WriteLine("------------Done------------");
        }

        /// <summary>
        /// Extracts all dat archives to C:\skeletonKey\SourceDats\
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnDeArchiveDATs_Click(object sender, RoutedEventArgs e)
        {
            var dats = DATFile.GetDatFiles();
            string outputDir = @"C:\skeletonKey\SourceDats\";
            Directory.CreateDirectory(outputDir);
            Console.WriteLine("Retrieving local DAT file archives - Count: " + dats.Count());
            Console.WriteLine("Extracting DATs to " + outputDir);

            await Task.Run(() =>
            {
                foreach (var d in dats)
                {
                    try
                    {
                        using (SevenZip.SevenZipExtractor archive = new SevenZip.SevenZipExtractor(d))
                        {
                            Console.WriteLine("Extracting: " + Path.GetFileName(d) + " To: " + outputDir + Path.GetFileNameWithoutExtension(d));
                            archive.ExtractArchive(outputDir);
                        }
                    }
                    catch (Exception ex)
                    {
                        string eS = ex.ToString();
                    }
                }
            });

            Console.WriteLine("------------Done------------");
        }

        /// <summary>
        /// Processes DAT files from disk (already extracted)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnProcessDATsNoArchive_Click(object sender, RoutedEventArgs e)
        {
            var dats = DATFile.GetDatFolders();

            List<string> DATErrors = new List<string>();

            Console.WriteLine("Retrieving local DAT folders - Count: " + dats.Count());
            int counter = 0;

            // setup output directory
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string exportedDir = "C:\\skeletonKey\\ExportedDats\\";
            string baseExportedDir = "rj\\scrapeart\\";
            string rootPath = exportedDir + baseExportedDir;
            Directory.CreateDirectory(rootPath);

            foreach (var dat in dats)//.Where(a => a.ToLower().Contains("lynx")))
            {
                // create dir for DAT
                string datName = new DirectoryInfo(dat).Name;
                string datExportDir = rootPath + datName;
                Directory.CreateDirectory(datExportDir);

                counter++;
                Console.WriteLine("Processing " + datName + " (" + counter + " of " + dats.Count() + ")");
                //Archive arch = new Archive();
                Console.WriteLine("(Exporting to " + datExportDir + ")");

                await Task.Run(() =>
                {
                    var results = Directory.GetFiles(dat).Where(a => Path.GetExtension(a).ToLower() == ".xml");
                    var rCount = results.Count();
                    var percentage = rCount / 50;
                    int cnt = 0;
                    int cnter = 0;

                    // process each archived file
                    foreach (var file in results)
                    {
                        string finalXml = string.Empty;
                        var str = File.ReadAllText(file);
                        string fix = XML.FixXml(str, this);
                        if (fix == null)
                        {
                            // there was an error parsing this file
                            StringBuilder sb = new StringBuilder();
                            sb.Append("SKIPPED: ");
                            sb.Append(datName);
                            sb.Append(@"\");
                            sb.Append(Path.GetFileName(file));
                            DATErrors.Add(sb.ToString());
                        }
                        finalXml = fix;

                        /*
                        using (Stream archiveStream = File.OpenRead(dat))
                        {
                            byte[] fData = Archive.ExtractFileToByteArray(archiveStream, file.InternalPath);
                            var str = Encoding.ASCII.GetString(fData);
                            string fix = XML.FixXml(str, this);
                            finalXml = fix;
                        }
                        */
                        cnt++;


                        // write file to disk
                        File.WriteAllText(datExportDir + "\\" + Path.GetFileName(file), finalXml);

                        // percentage complete
                        int currPos = Convert.ToInt32((double)(((double)cnt / (double)rCount) * 100));

                        if (currPos > cnter)
                        {
                            Console.Write(".");
                            cnter = currPos;
                        }
                    }
                    Console.Write("\n");
                });
            }

            Console.WriteLine("----------------");
            Console.WriteLine("ERRORs Detected: " + DATErrors.Count());
            Console.WriteLine("ERROR Listings:");
            Console.WriteLine();

            foreach (var error in DATErrors)
            {
                Console.WriteLine(error);
            }

            Console.WriteLine("------------Done------------");
        }

        /// <summary>
        /// Re-archives all processed DATs and puts them in ..\..\ExportedDATs
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnArchiveDATs_Click(object sender, RoutedEventArgs e)
        {
            string exportedDir = "C:\\skeletonKey\\ExportedDats\\";
            string baseExportedDir = "rj\\scrapeart\\";
            string rootPath = exportedDir + baseExportedDir;
            string outputDir = "C:\\skeletonKey\\ExportedArchives\\";

            Directory.CreateDirectory(outputDir);

            // get all sub folders
            var folders = Directory.GetDirectories(rootPath);

            Console.WriteLine("Starting Compression...");

            // create a 7zip archive for each directory
            Archive archive = new Archive();

            await Task.Run(() =>
            {
                foreach (var folder in folders)
                {
                    string name = new DirectoryInfo(folder).Name;
                    bool result = archive.CreateArchiveFromFolder(folder, outputDir, name);

                    if (result)
                    {
                        Console.WriteLine("SUCCESS: " + outputDir + name + ".7z");
                    }
                    else
                    {
                        Console.WriteLine("FAILURE: " + outputDir + name + ".7z");
                    }
                }
            });
        }
    }
}
