using System.Collections.Generic;

using Pdoxcl2Sharp;

namespace CK2LandedTitlesManager.DataAccess.IO
{
    public sealed class LandedTitlesFile : IParadoxRead, IParadoxWrite
    {
        public IList<LandedTitleDefinition> LandedTitles { get; set; }

        public LandedTitlesFile()
        {
            LandedTitles = new List<LandedTitleDefinition>();
        }

        public void TokenCallback(ParadoxParser parser, string token)
        {
            LandedTitleDefinition landedTitle = new LandedTitleDefinition
            {
                Id = token
            };

            LandedTitles.Add(parser.Parse(landedTitle));
        }
        
        public void Write(ParadoxStreamWriter writer)
        {
            foreach (LandedTitleDefinition landedTitle in LandedTitles)
            {
                writer.Write(landedTitle.Id, landedTitle);
            }
        }
    }
}
