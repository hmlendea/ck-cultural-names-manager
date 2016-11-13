using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace CK2LandedTitlesExtractor
{
    public class Program
    {
        static Dictionary<int, string> titles;

        public static void Main(string[] args)
        {
            // TODO: Proper handling
            if (args.Length == 0)
                return;

            // TODO: Check path validity
            string fileName = args[0];

            titles = new Dictionary<int, string>();

            LoadTitles(fileName);
        }

        private static void LoadTitles(string fileName)
        {
            List<string> lines = File.ReadAllLines(fileName).ToList();
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
    }
}
