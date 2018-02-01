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

        public MainWindow()
        {
            InitializeComponent();

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
        private void btnProcessDATs_Click(object sender, RoutedEventArgs e)
        {
            var dats = DATFile.GetDatFiles();
            Console.WriteLine("Retrieving local DAT file archives - Count: " + dats.Count());
            foreach (var dat in dats)
            {
                Archive arch = new Archive();
                var results = arch.ProcessArchive(dat);

                // process each archived file
                foreach (var file in results.Results)
                {
                    using (Stream archiveStream = File.OpenRead(dat))
                    {                    
                        //archiveStream.Seek(0, SeekOrigin.Begin);
                        byte[] fData = Archive.ExtractFileToByteArray(archiveStream, file.InternalPath);
                        var str = Encoding.ASCII.GetString(fData);
                    }
                }

                foreach (var res in results.Results)
                {
                    //Console.WriteLine(res.InternalPath);
                }
            }
            Console.WriteLine("------------Done------------");
        }
    }
}
