using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using CK2LandedTitlesExtractor.Entities;

namespace CK2LandedTitlesExtractor.Repositories
{
    public class TitleRepository : Repository<Title>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CK2LandedTitlesExtractor.Repositories.TitleRepository"/> class.
        /// </summary>
        public TitleRepository()
        {
        }

        /// <summary>
        /// Loads the titles
        /// </summary>
        /// <param name="fileName">Path to the input file</param>
        public void Load(string fileName)
        {
            List<string> lines = File.ReadAllLines(Path.GetFullPath(fileName)).ToList();
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

                    Add(title);
                }
            }
        }
    }
}
