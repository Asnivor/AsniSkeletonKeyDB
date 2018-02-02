using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace SKDB.Classes
{
    /// <summary>
    /// Xml related operations and base class for the final parsed XML object
    /// </summary>
    public class XML
    {
        public string name { get; set; }

        /// <summary>
        /// Attempts to parse a potentially fubar skeletonKey xml dat string and returns a fixed one
        /// </summary>
        /// <param name="xmlString"></param>
        /// <returns></returns>
        public static string FixXml(string xmlString, MainWindow mw)
        {
            //MainWindow mw = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();

            bool errorDetected = false;

            List<XMLTag> allTags = new List<XMLTag>();

            // get rid of the parent <game></game> tags
            xmlString = xmlString.Replace("<Game>", "").Replace("</game>", "")
                .Replace("<game>", "").Replace("</Game>", "");

            // get all tags
            Regex regex = new Regex("<(.*?)>");
            MatchCollection v = regex.Matches(xmlString);

            // iterate through each pair of tags
            for (int i = 0; i < v.Count; i += 2)
            {
                // create new xmltag object
                XMLTag t = new XMLTag();

                Match item = v[i];
                string tag = item.Groups[1].Value;
                t.TagName = tag;

                // extract the data between the two tags
                try
                {
                    string value = new string(xmlString.Skip(v[i].Index).Take(v[i + 1].Index - v[i].Index).ToArray()).Replace(v[i].Groups[0].Value, "");
                    t.TagData = value;

                    // add tag to list
                    allTags.Add(t);
                }
                catch (Exception ex)
                {
                    return null;
                }
                

            }

            if (errorDetected)
            {

            }

            // sort out duplicates
            List<XMLTag> newTags = new List<XMLTag>();

            foreach (var ta in allTags)
            {
                bool noNumber = false;
                // first check whether the tag has numbers at the end and strip them out if so
                string workingTag = ta.TagName;
                Regex numCheck = new Regex(@"\d");

                // do an initial match first - if there is no number then we need to know before moving on
                Match initial = numCheck.Match(workingTag);
                if (initial.Success == false)
                {
                    noNumber = true;
                }

                // attempt to match numbers 3 times - each iteration will remove 1 number
                for (int i = 0; i < 3; i++)
                {
                    Match m = numCheck.Match(workingTag);
                    if (m.Success == true)
                    {
                        // contains a number - strip this out
                        workingTag = workingTag.Remove(ta.TagName.LastIndexOf(m.Value) - i);
                    }
                    else
                    {
                        // no number
                        break;
                    }
                }

                // see if there is a match already
                var lookup = newTags.Where(a => a.TagName.ToLower() == workingTag.ToLower()).FirstOrDefault();

                // workingTag should now have no number - duplicate mitigation
                if (noNumber == true)
                {
                    // tag had no number in the first place
                    if (lookup == null)
                    {
                        // just add as is
                        newTags.Add(new XMLTag { TagData = ta.TagData, TagName = workingTag });
                    }
                    else
                    {
                        // tagname has no number originally but is duplicated for some reason
                        // skip this for now?
                    }
                }
                else
                {
                    // tag had a number to begin with - loop through starting at 1 to try and find the next free number
                    // for this tag
                    for (int i = 1; i < 31; i++)
                    {
                        var l = newTags.Where(a => a.TagName.ToLower() == workingTag.ToLower() + i).FirstOrDefault();
                        if (l == null)
                        {
                            // no match found - add it
                            newTags.Add(new XMLTag { TagData = ta.TagData, TagName = workingTag + i });
                            break;
                        }
                        else
                        {
                            // duplicate found
                            continue;
                        }
                    }
                }
            }

            foreach (var tag in newTags)
            {
                mw.DetectedTags.Add(tag.TagName);
            }

            mw.DetectedTags = mw.DetectedTags.Distinct().ToList();

            /*
            foreach (var ta in allTags)
            {
                // see whether this tag already exists in the new list
                var lookup = newTags.Where(a => a.TagName.ToLower() == ta.TagName.ToLower()).FirstOrDefault();

                if (lookup == null)
                {
                    // tag does not exist - check whether it is numbered correctly
                    Regex numCheck = new Regex(@"\d");
                    Match m = numCheck.Match(ta.TagName);
                    

                    if (m.Success == false)
                    {
                        // contains no number - just add it
                        newTags.Add(ta);
                    }
                    else
                    {
                        newTags.Add(ta);
                    }

                   
                }
                else
                {
                    // tag already exists - check whether it has a number
                    Regex numCheck = new Regex(@"\d");
                    Match m = numCheck.Match(ta.TagName);

                    if (m.Success == true)
                    {
                        // get current number
                        int currNo = Convert.ToInt32(m.Value);

                        // get string without current number
                        string bare = ta.TagName.Remove(ta.TagName.LastIndexOf(m.Value));

                        // iterate through and find the next available number for this tag
                        for (int i = currNo + 1; i < 40; i++)
                        {
                            var look = newTags.Where(a => a.TagName.ToLower() == bare.ToLower() + i.ToString()).FirstOrDefault();

                            if (look == null)
                            {
                                // no match found - good to go
                                XMLTag newT = new XMLTag();
                                newT.TagData = ta.TagData;
                                newT.TagName = bare + i.ToString();
                                newTags.Add(newT);
                                break;
                            }
                        }
                    }

                    
                }
            }

    */

            // now build the new xml string
            StringBuilder sb = new StringBuilder();
            sb.Append("<Game>");
            sb.Append("\r\n");

            foreach (var t in newTags)
            {
                sb.Append("\t");
                sb.Append("<" + t.TagName + ">");
                sb.Append(t.TagData);
                sb.Append("</" + t.TagName + ">");
                sb.Append("\r\n");
            }

            sb.Append("</game>");

            return sb.ToString();
        }

        
    }

    public class XMLTag
    {
        public string TagName { get; set; }
        public string TagData { get; set; }
    }
}
