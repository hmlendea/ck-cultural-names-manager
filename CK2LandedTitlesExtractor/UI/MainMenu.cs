using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using CK2LandedTitlesExtractor.Entities;

namespace CK2LandedTitlesExtractor.UI
{
    /// <summary>
    /// Main menu.
    /// </summary>
    public class MainMenu : Menu
    {
        static Dictionary<int, Title> titles;
        static Dictionary<int, Name> names;

        /// <summary>
        /// Initializes a new instance of the <see cref="CK2LandedTitlesExtractor.UI.Mainmenu"/> class.
        /// </summary>
        public MainMenu()
        {
            Title = "CK2 Landed Titles Extractor";

            AddCommand(
                "load",
                "Load the input file (landed_titles.txt)",
                delegate { LoadFile(); });

            AddCommand(
                "save",
                "save the output file",
                delegate { SaveFile(); });

            AddCommand(
                "print",
                "Display the landed titles",
                delegate { DisplayLandedTitles(); });
        }

        /// <summary>
        /// Outputs the title names
        /// </summary>
        private void DisplayLandedTitles()
        {
            foreach (Name name in names.Values)
            {
                string title = titles[name.TitleId].Text.PadRight(23, ' ');

                Console.WriteLine("{0} = {{ {1} = \"{2}\" }}", title, name.Culture, name.Text);
            }
        }

        /// <summary>
        /// Saves the landed titles to the specified file
        /// </summary>
        /// /// <param name="fileName">Path to the output landed_title file</param>
        private void SaveLandedTitles(string fileName)
        {
            if (File.Exists(fileName))
                File.Delete(fileName);

            StreamWriter sw = new StreamWriter(File.OpenWrite(fileName));

            foreach (int titleId in titles.Keys)
            {
                string title = titles[titleId].Text;
                int nameCount = names.Values.ToList().FindAll(x => x.TitleId == titleId).Count;

                if (nameCount == 0)
                    continue;

                sw.WriteLine("{0} = {{", title);

                foreach (Name titleName in names.Values)
                    if (titleName.TitleId == titleId)
                        sw.WriteLine("  {0} = \"{1}\"", titleName.Culture, titleName.Text);

                sw.WriteLine("}");
            }

            sw.Dispose();
        }

        /// <summary>
        /// Cleans the titles and names
        /// </summary>
        private void CleanTitlesAndNames()
        {
            List<int> titlesToRemove = new List<int>();
            List<int> namesToRemove = new List<int>();

            Dictionary<int, string> uniqueTitlesByKey = new Dictionary<int, string>();
            Dictionary<string, int> uniqueTitlesByVal = new Dictionary<string, int>();
            Dictionary<int, Name> uniqueNames = new Dictionary<int, Name>();

            foreach (int titleKey in titles.Keys)
            {
                string title = titles[titleKey].Text;

                if (uniqueTitlesByVal.ContainsKey(title))
                {
                    int titleKeyFinal = uniqueTitlesByVal[title];

                    foreach (Name name in names.Values.Where(x => x.TitleId == titleKey))
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
                Name name = names[nameKey];

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
        private void LoadFile()
        {
            titles = new Dictionary<int, Title>();
            names = new Dictionary<int, Name>();

            string fileName = Input("Input file path (absolute) = ");
            List<string> lines = File.ReadAllLines(Path.GetFullPath(fileName)).ToList();

            Console.Write("Loading titles... ");
            LoadTitles(lines);
            Console.WriteLine("OK ({0} titles)", titles.Count);

            Console.Write("Loading names... ");
            LoadNames(lines);
            Console.WriteLine("OK ({0} names)", names.Count);

            Console.Write("Linking names with titles... ");
            LinkNamesWithTitles();
            Console.WriteLine("OK");

            Console.Write("Cleaning titles and names... ");
            CleanTitlesAndNames();
            Console.WriteLine("OK");
        }

        private void SaveFile()
        {
            string fileName = Input("Output file path (absolute) = ");

            Console.Write("Writing output... ");
            SaveLandedTitles(fileName);
            Console.WriteLine("OK");
        }

        /// <summary>
        /// Loads the titles
        /// </summary>
        /// <param name="lines">Lines of the landed_titles file</param>
        private void LoadTitles(List<string> lines)
        {
            Regex regex = new Regex("^([bcdke](_[a-z]*(_[a-z]*)*))");

            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i].Trim();
                Match match = regex.Match(line);

                if (match.Success)
                {
                    Title title = new Title()
                    {
                        Id = i,
                        Text = match.Value.Trim(),
                        DeJureTitleId = -1
                    };

                    titles.Add(title.Id, title);
                }
            }
        }

        /// <summary>
        /// Loads the title names
        /// </summary>
        /// <param name="lines">Lines of the landed_titles file</param>
        private void LoadNames(List<string> lines)
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
                        Name titleName = new Name
                        {
                            Culture = culture,
                            Text = name
                        };

                        names.Add(i, titleName);
                    }
                }
            }
        }

        /// <summary>
        /// Links names with their respective titles
        /// </summary>
        private void LinkNamesWithTitles()
        {
            foreach (int nameKey in names.Keys)
            {
                Name name = names[nameKey];

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
