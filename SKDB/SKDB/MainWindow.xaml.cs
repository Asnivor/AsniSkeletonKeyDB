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
        /// Processes DAT files from disk
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnProcessDATs_Click(object sender, RoutedEventArgs e)
        {
            var dats = DATFile.GetDatFiles();
            Console.WriteLine("Retrieving local DAT file archives - Count: " + dats.Count());
            int counter = 0;
            foreach (var dat in dats)//.Where(a => a.ToLower().Contains("lynx")))
            {
                counter++;
                Console.WriteLine("Processing " + System.IO.Path.GetFileName(dat) + " (" + counter + " of " + dats.Count() + ")");
                Archive arch = new Archive();

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
                        using (Stream archiveStream = File.OpenRead(dat))
                        {
                            //archiveStream.Seek(0, SeekOrigin.Begin);
                            byte[] fData = Archive.ExtractFileToByteArray(archiveStream, file.InternalPath);
                            var str = Encoding.ASCII.GetString(fData);
                            string fix = XML.FixXml(str, this);
                        }
                        cnt++;

                        // percentage complete
                        int currPos = Convert.ToInt32((double)(((double)cnt / (double)rCount) * 100));

                        if (currPos > cnter)
                        {
                            Console.Write(".");
                            cnter = currPos;
                        }

                        if (currPos % 2 != 0)
                        {
                            
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
    }
}
