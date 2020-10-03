using System.Collections.Generic;
using System.Linq;

using Pdoxcl2Sharp;
using NuciExtensions;

using CKCulturalNamesManager.DataAccess.DataObjects;

namespace CKCulturalNamesManager.DataAccess.IO
{
    public sealed class LandedTitleDefinition : IParadoxRead, IParadoxWrite
    {
        public LandedTitleEntity LandedTitleEntity { get; set; }

        public LandedTitleDefinition()
        {
            LandedTitleEntity = new LandedTitleEntity();
        }
        
        public void TokenCallback(ParadoxParser parser, string token)
        {
            if (token[1] == '_') // Like e_something or c_something
            {
                LandedTitleDefinition landedTitle = new LandedTitleDefinition();
                landedTitle.LandedTitleEntity.ParentId = LandedTitleEntity.Id;
                landedTitle.LandedTitleEntity.Id = token;

                LandedTitleEntity.Children.Add(parser.Parse(landedTitle).LandedTitleEntity);
                return;
            }

            switch (token)
            {
                // TODO: Skip brackets
                case "allow":
                case "coat_of_arms":
                case "color":
                case "color2":
                case "female_names":
                case "gain_effect":
                case "male_names":
                case "pagan_coa":
                    parser.ReadInsideBrackets((p) => { });
                    break;

                // TODO: Skip values
                case "assimilate":
                case "caliphate":
                case "can_be_claimed":
                case "can_be_usurped":
                case "capital":
                case "controls_religion":
                case "creation_requires_capital":
                case "culture":
                case "dignity":
                case "dynasty_title_names":
                case "foa":
                case "graphical_culture":
                case "has_top_de_jure_capital":
                case "holy_order":
                case "holy_site":
                case "independent":
                case "landless":
                case "location_ruler_title":
                case "mercenary_type":
                case "mercenary":
                case "monthly_income":
                case "name_tier":
                case "pentarchy":
                case "pirate":
                case "primary":
                case "purple_born_heirs":
                case "rebel":
                case "religion":
                case "replace_captain_on_death":
                case "short_name":
                case "strength_growth_per_century":
                case "title_female":
                case "title_prefix":
                case "title":
                case "top_de_jure_capital":
                case "tribe":
                case "used_for_dynasty_names":
                    parser.ReadString();
                    break;

                default:
                    string stringValue = parser.ReadString();
                    int intValue;

                    if (!int.TryParse(stringValue, out intValue)) // If it's not a relivious value
                    {
                        LandedTitleEntity.Names.AddOrUpdate(token, stringValue);
                    }

                    break;
            }
        }
        
        public void Write(ParadoxStreamWriter writer)
        {
            List<KeyValuePair<string, string>> sortedNames = LandedTitleEntity.Names.ToList().OrderBy(x => x.Key).ToList();

            foreach(var name in sortedNames)
            {
                writer.WriteLine(name.Key, name.Value, ValueWrite.Quoted);
            }

            foreach (LandedTitleEntity landedTitle in LandedTitleEntity.Children)
            {
                LandedTitleDefinition landedTitleDefinition = new LandedTitleDefinition
                {
                    LandedTitleEntity = landedTitle
                };

                writer.Write(landedTitle.Id, landedTitleDefinition);
            }
        }
    }
}
