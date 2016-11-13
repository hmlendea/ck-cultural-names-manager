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
        }

        private static void LoadFile(string fileName)
        {
            List<string> lines = File.ReadAllLines(fileName).ToList();

            LoadTitles(lines);
            LoadNames(lines);
        }

        private static void LoadTitles(List<string> lines)
        {
            Regex regex = new Regex("([bcdke](_[a-z]*)+)");
            int i = 0;

            foreach (string line in lines)
            {
                MatchCollection matches = regex.Matches(line);

                foreach (Match match in matches)
                {
                    titles.Add(i, match.Value);

                    i += 1;
                }
            }
        }

        private static void LoadNames(List<string> lines)
        {
            List<string> blacklistPatterns = File.ReadAllLines("non_cultures.lst").ToList();
            Regex regex = new Regex(@"([a-z]* = " + "\"" + @"*\p{L}+( \p{L}+)*" + "\"*)");
            int i = 0;

            foreach (string line in lines)
            {
                MatchCollection matches = regex.Matches(line);

                foreach (Match match in matches)
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
                        i += 1;
                    }
                }
            }
        }
    }
}
