using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKDB.Database
{
    public class DBFunctions
    {
        /// <summary>
        /// Adds or updates list of systems in the database
        /// </summary>
        /// <param name="systemNames"></param>
        public static void AddUpdateSystems(List<string> systemNames)
        {
            skeletonKeyEntities sk = new skeletonKeyEntities();

            foreach (var n in systemNames)
            {
                var lookup = sk.SK_System.Where(a => a.name == n).FirstOrDefault();

                if (lookup == null)
                {
                    SK_System s = new SK_System();
                    s.name = n;
                    sk.SK_System.Add(s);
                }
            }

            sk.SaveChanges();
        }
    }
}
