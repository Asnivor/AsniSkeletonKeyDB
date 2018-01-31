using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SKDB.Database;

namespace SKDB.Classes
{
    public class SkeletonDB
    {
        /// <summary>
        /// gets all system entries from the database
        /// </summary>
        /// <returns></returns>
        public static List<SK_System> GetSystems()
        {
            skeletonKeyEntities db = new skeletonKeyEntities();
            return db.SK_System.ToList();
        }
    }
}
