using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace CK2LandedTitlesExtractor
{
    public class TitleName
    {
        public string Culture { get; set; }
        public string Name { get; set; }
        public int TitleId { get; set; }
    }

    public class Program
    {
        static Dictionary<int, string> titles;
        static Dictionary<int, TitleName> names;

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
            AssociateNamesWithTitles();

            DisplayLandedTitles();
        }

        private static void DisplayLandedTitles()
        {
            Console.WriteLine("{0} titles", titles.Count);
            Console.WriteLine("{0} names", names.Count);

            foreach (TitleName name in names.Values)
            {
                string title = titles[name.TitleId].PadRight(24, ' ');

                Console.WriteLine("{0} = {{ {1} = \"{2}\" }}", title, name.Culture, name.Name);
            }
        }

        private static void LoadFile(string fileName)
        {
            List<string> lines = File.ReadAllLines(fileName).ToList();

            LoadTitles(lines);
            LoadNames(lines);
        }

        private static void LoadTitles(List<string> lines)
        {
            Regex regex = new Regex("^([bcdke](_[a-z]*(_[a-z]*)*))");

            for (int i = 1; i < lines.Count; i++)
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

                    foreach(string pattern in blacklistPatterns)
                    {
                        Regex blacklistRegex = new Regex(pattern);

                        if(blacklistRegex.IsMatch(culture))
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

        private static void AssociateNamesWithTitles()
        {
            foreach (int nameKey in names.Keys)
            {
                TitleName name = names[nameKey];

                for (int titleKey = nameKey; titleKey > 0; titleKey--)
                    if(titles.Keys.Contains(titleKey))
                    {
                        name.TitleId = titleKey;
                        break;
                    }
            }
        }
    }
}
