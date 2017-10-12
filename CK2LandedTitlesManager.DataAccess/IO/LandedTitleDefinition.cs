using System.Collections.Generic;

using CK2LandedTitlesManager.DataAccess.DataObjects;
using CK2LandedTitlesManager.Infrastructure.Extensions;

using Pdoxcl2Sharp;

namespace CK2LandedTitlesManager.DataAccess.IO
{
    public sealed class LandedTitleDefinition : LandedTitleEntity, IParadoxRead, IParadoxWrite
    {
        public LandedTitleDefinition()
        {
            DynamicNames = new Dictionary<string, string>();
            Children = new List<LandedTitleEntity>();
        }

        public void TokenCallback(ParadoxParser parser, string token)
        {
            if (token[1] == '_') // Like e_something or c_something
            {
                LandedTitleDefinition landedTitle = new LandedTitleDefinition
                {
                    Id = token,
                    ParentId = Id
                };

                Children.Add(parser.Parse(landedTitle));
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

            foreach (LandedTitleDefinition landedTitle in Children)
            {
                writer.Write(landedTitle.Id, landedTitle);
            }
        }
    }
}
