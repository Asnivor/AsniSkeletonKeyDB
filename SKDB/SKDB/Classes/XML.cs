using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKDB.Classes
{
    /// <summary>
    /// Xml related operations
    /// </summary>
    public class XML
    {
        /// <summary>
        /// Attempts to parse a potentially fubar skeletonKey xml dat and returns a fixed one
        /// </summary>
        /// <param name="xmlString"></param>
        /// <returns></returns>
        public static string FixXml(string xmlString)
        {
            string newString;

            // capitalization first (make all tags lowercase)
            newString = xmlString;

            return newString;
        }
    }
}
