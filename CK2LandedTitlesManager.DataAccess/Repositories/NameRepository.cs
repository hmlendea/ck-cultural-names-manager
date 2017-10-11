using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using CK2LandedTitlesManager.Models;

namespace CK2LandedTitlesManager.DataAccess.Repositories
{
    public class NameRepository : Repository<DynamicName>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NameRepository"/> class.
        /// </summary>
        public NameRepository()
        {
        }

        /// <summary>
        /// Gets all names by the landed title identifier.
        /// </summary>
        /// <returns>List of names.</returns>
        /// <param name="titleId">The landed title identifier</param>
        public IEnumerable<DynamicName> GetAllByTitleId(int titleId)
        {
            IEnumerable<DynamicName> names = GetAll();

            return names.Where(x => x.LandedTitleId == titleId);
        }

        /// <summary>
        /// Gets all names by name.
        /// </summary>
        /// <returns>List of names.</returns>
        /// <param name="text">The name</param>
        public IEnumerable<DynamicName> GetAllByText(string text)
        {
            IEnumerable<DynamicName> names = GetAll();

            return names.Where(x => x.Name == text);
        }

        /// <summary>
        /// Loads the names
        /// </summary>
        /// <param name="fileName">Path to the input file</param>
        public void Load(string fileName)
        {
            List<string> lines = File.ReadAllLines(Path.GetFullPath(fileName)).ToList();
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
                    string text = split[1].Trim().Replace("\"", "").Replace("_", " ");
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
                        DynamicName name = new DynamicName
                        {
                            Id = i,
                            CultureId = culture,
                            Name = text
                        };
                        
                        Add(name);
                    }
                }
            }
        }
    }
}
