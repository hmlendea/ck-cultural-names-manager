using System.Collections.Generic;

using CK2LandedTitlesManager.Infrastructure.Extensions;

using Pdoxcl2Sharp;

namespace CK2LandedTitlesManager.DataAccess.IO
{
    public sealed class LandedTitleDefinition : IParadoxRead, IParadoxWrite
    {
        public IDictionary<string, string> DynamicNames { get; set; }

        public IList<LandedTitleDefinition> LandedTitles { get; set; }

        public string Id { get; set; }

        public LandedTitleDefinition()
        {
            DynamicNames = new Dictionary<string, string>();
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
            else
            {
                DynamicNames.AddOrUpdate(token, parser.ReadString());
            }
        }
        
        public void Write(ParadoxStreamWriter writer)
        {
            foreach(var dynamicName in DynamicNames)
            {
                writer.WriteLine(dynamicName.Key, dynamicName.Value, ValueWrite.Quoted);
            }

            foreach (LandedTitleDefinition landedTitle in LandedTitles)
            {
                writer.Write(landedTitle.Id, landedTitle);
            }
        }
    }
}
