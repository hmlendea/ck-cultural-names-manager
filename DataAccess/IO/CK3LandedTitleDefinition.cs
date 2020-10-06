using System.Collections.Generic;
using System.Linq;

using Pdoxcl2Sharp;
using NuciExtensions;

using CKCulturalNamesManager.DataAccess.DataObjects;

namespace CKCulturalNamesManager.DataAccess.IO
{
    public sealed class CK3LandedTitleDefinition : CKLandedTitleDefinition
    {
        public CK3LandedTitleDefinition() : base()
        {
        }
        
        public override void TokenCallback(ParadoxParser parser, string token)
        {
            if (token.Length >= 3 && token[1] == '_') // Like e_something or c_something
            {
                CK3LandedTitleDefinition landedTitle = new CK3LandedTitleDefinition();
                landedTitle.LandedTitleEntity.ParentId = LandedTitleEntity.Id;
                landedTitle.LandedTitleEntity.Id = token;

                LandedTitleEntity.Children.Add(parser.Parse(landedTitle).LandedTitleEntity);
                return;
            }

            switch (token)
            {
                case "cultural_names":
                    CK3CulturalNamesDefinition culturalNames = parser.Parse(new CK3CulturalNamesDefinition());
                    
                    foreach (var culturalName in culturalNames.Names)
                    {
                        LandedTitleEntity.Names.AddOrUpdate(culturalName.Key, culturalName.Value);
                    }

                    break;

                // TODO: Skip brackets
                case "ai_primary_priority":
                case "allow":
                case "can_create":
                case "female_names":
                case "male_names":
                    parser.ReadInsideBrackets((p) => { });
                    break;

                case "color":
                case "color2":
                    string method = parser.ReadString();

                    if (method == "hsv")
                    {
                        parser.ReadFloat();
                    }
                    else
                    {
                        parser.ReadInt16();
                        parser.ReadInt16();
                    }

                    break;
                
                default:
                    parser.ReadString();
                    break;
            }
        }
        
        public override void Write(ParadoxStreamWriter writer)
        {
            List<KeyValuePair<string, string>> sortedNames = LandedTitleEntity.Names.ToList().OrderBy(x => x.Key).ToList();

            foreach(var name in sortedNames)
            {
                writer.WriteLine(name.Key, name.Value, ValueWrite.Quoted);
            }

            foreach (LandedTitleEntity landedTitle in LandedTitleEntity.Children)
            {
                CK3LandedTitleDefinition landedTitleDefinition = new CK3LandedTitleDefinition
                {
                    LandedTitleEntity = landedTitle
                };

                writer.Write(landedTitle.Id, landedTitleDefinition);
            }
        }
    }
}
