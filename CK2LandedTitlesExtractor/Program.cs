using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace CK2LandedTitlesExtractor
{
    /// <summary>
    /// The title name class
    /// </summary>
    public class TitleName
    {
        public string Culture { get; set; }
        public string Name { get; set; }
        public int TitleId { get; set; }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to the current <see cref="CK2LandedTitlesExtractor.TitleName"/>.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with the current <see cref="CK2LandedTitlesExtractor.TitleName"/>.</param>
        /// <returns><c>true</c> if the specified <see cref="System.Object"/> is equal to the current
        /// <see cref="CK2LandedTitlesExtractor.TitleName"/>; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            TitleName other = obj as TitleName;

            if (this.TitleId == other.TitleId &&
                this.Culture == other.Culture &&
                this.Name == other.Name)
                return true;

            return false;
        }
    }

    public class Program
    {
        static Dictionary<int, string> titles;
        static Dictionary<int, TitleName> names;

        /// <summary>
        /// The entry point of the program, where the program control starts and ends.
        /// </summary>
        /// /// <param name="args">CLI arguments</param>
        public static void Main(string[] args)
        {
            // TODO: Proper handling
            if (args.Length == 0)
                return;

            // TODO: Check path validity
            string fileName = args[0];

            titles = new Dictionary<int, string>();
            names = new Dictionary<int, TitleName>();

            LoadFile(fileName);

            Console.Write("Linking names with titles... ");
            LinkNamesWithTitles();
            Console.WriteLine("OK");

            Console.Write("Cleaning titles and names... ");
            CleanTitlesAndNames();
            Console.WriteLine("OK");

            //DisplayLandedTitles();

            Console.Write("Writing output... ");
            SaveLandedTitles(fileName + ".output.txt");
            Console.WriteLine("OK");
        }

        /// <summary>
        /// Outputs the title names
        /// </summary>
        private static void DisplayLandedTitles()
        {
            foreach (TitleName name in names.Values)
            {
                string title = titles[name.TitleId].PadRight(23, ' ');

                Console.WriteLine("{0} = {{ {1} = \"{2}\" }}", title, name.Culture, name.Name);
            }
        }

        /// <summary>
        /// Saves the landed titles to the specified file
        /// </summary>
        /// /// <param name="fileName">Path to the output landed_title file</param>
        private static void SaveLandedTitles(string fileName)
        {
            if (File.Exists(fileName))
                File.Delete(fileName);

            StreamWriter sw = new StreamWriter(File.OpenWrite(fileName));

            foreach (int titleId in titles.Keys)
            {
                string title = titles[titleId];
                int nameCount = names.Values.ToList().FindAll(x => x.TitleId == titleId).Count;

                if (nameCount == 0)
                    continue;

                sw.WriteLine("{0} = {{", title);

                foreach (TitleName titleName in names.Values)
                    if (titleName.TitleId == titleId)
                        sw.WriteLine("  {0} = \"{1}\"", titleName.Culture, titleName.Name);

                sw.WriteLine("}");
            }

            sw.Dispose();
        }

        /// <summary>
        /// Cleans the titles and names
        /// </summary>
        private static void CleanTitlesAndNames()
        {
            List<int> titlesToRemove = new List<int>();
            List<int> namesToRemove = new List<int>();

            Dictionary<int, string> uniqueTitlesByKey = new Dictionary<int, string>();
            Dictionary<string, int> uniqueTitlesByVal = new Dictionary<string, int>();
            Dictionary<int, TitleName> uniqueNames = new Dictionary<int, TitleName>();

            foreach (int titleKey in titles.Keys)
            {
                string title = titles[titleKey];

                if (uniqueTitlesByVal.ContainsKey(title))
                {
                    int titleKeyFinal = uniqueTitlesByVal[title];

                    foreach (TitleName name in names.Values.Where(x => x.TitleId == titleKey))
                        name.TitleId = titleKeyFinal;

                    titlesToRemove.Add(titleKey);
                }
                else
                {
                    uniqueTitlesByVal.Add(title, titleKey);
                    uniqueTitlesByKey.Add(titleKey, title);
                }
            }

            foreach (int nameKey in names.Keys)
            {
                TitleName name = names[nameKey];

                if (uniqueNames.ContainsValue(name))
                    namesToRemove.Add(nameKey);
                else
                    uniqueNames.Add(nameKey, name);
            }

            foreach (int titleKeyToRemove in titlesToRemove)
                titles.Remove(titleKeyToRemove);

            foreach (int nameKeyToRemove in namesToRemove)
            {
                names.Remove(nameKeyToRemove);
            }
        }

        /// <summary>
        /// Loads the landed_title file
        /// </summary>
        /// <param name="fileName">Path to the landed_title file</param>
        private static void LoadFile(string fileName)
        {
            List<string> lines = File.ReadAllLines(fileName).ToList();

            Console.Write("Loading titles... ");
            LoadTitles(lines);
            Console.WriteLine("OK ({0} titles)", titles.Count);

            Console.Write("Loading names... ");
            LoadNames(lines);
            Console.WriteLine("OK ({0} names)", names.Count);
        }

        /// <summary>
        /// Loads the titles
        /// </summary>
        /// <param name="lines">Lines of the landed_titles file</param>
        private static void LoadTitles(List<string> lines)
        {
            Regex regex = new Regex("^([bcdke](_[a-z]*(_[a-z]*)*))");

            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i].Trim();
                Match match = regex.Match(line);

                if (match.Success)
                {
                    string title = match.Value.Trim();
                    titles.Add(i, title);
                }
            }
        }

        /// <summary>
        /// Loads the title names
        /// </summary>
        /// <param name="lines">Lines of the landed_titles file</param>
        private static void LoadNames(List<string> lines)
        {
            List<string> blacklistPatterns = File.ReadAllLines("non_cultures.lst").ToList();
            Regex regex = new Regex(@"^([a-z]* = " + "\"" + @"*\p{L}+( \p{L}+)*" + "\"*)$");

            for (int i = 1; i < lines.Count; i++)
            {
                string line = lines[i].Trim();
                Match match = regex.Match(line);

                if (match.Success)
                {
                    string[] split = line.Split('=');
                    string culture = split[0].Trim();
                    string name = split[1].Trim().Replace("\"", "").Replace("_", " ");
                    bool valid = true;

                    foreach (string pattern in blacklistPatterns)
                    {
                        Regex blacklistRegex = new Regex(pattern);

                        if (blacklistRegex.IsMatch(culture))
                        {
                            valid = false;
                            break;
                        }
                    }

                    if (valid)
                    {
                        TitleName titleName = new TitleName
                        {
                            Culture = culture,
                            Name = name
                        };

                        names.Add(i, titleName);
                    }
                }
            }
        }

        /// <summary>
        /// Links names with their respective titles
        /// </summary>
        private static void LinkNamesWithTitles()
        {
            foreach (int nameKey in names.Keys)
            {
                TitleName name = names[nameKey];

                for (int titleKey = nameKey; titleKey > 0; titleKey--)
                    if (titles.Keys.Contains(titleKey))
                    {
                        name.TitleId = titleKey;
                        break;
                    }
            }
        }
    }
}
