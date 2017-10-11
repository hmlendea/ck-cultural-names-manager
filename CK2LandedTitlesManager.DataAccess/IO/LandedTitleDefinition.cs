using System.Collections.Generic;
using System.Text.RegularExpressions;

using Pdoxcl2Sharp;

namespace CK2LandedTitlesManager.DataAccess.IO
{
    public sealed class LandedTitleDefinition : IParadoxRead, IParadoxWrite
    {
        public IList<LandedTitleDefinition> LandedTitles { get; set; }

        public string Id { get; set; }

        public LandedTitleDefinition()
        {
            LandedTitles = new List<LandedTitleDefinition>();
        }

        public void TokenCallback(ParadoxParser parser, string token)
        {
            if (token[1] == '_') // Like e_something or c_something
            {
                LandedTitleDefinition landedTitle = new LandedTitleDefinition
                {
                    Id = token
                };

                LandedTitles.Add(parser.Parse(landedTitle));
            }
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
