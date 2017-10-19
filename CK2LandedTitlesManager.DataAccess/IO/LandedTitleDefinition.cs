using System.Linq;

using CK2LandedTitlesManager.DataAccess.DataObjects;
using CK2LandedTitlesManager.Infrastructure.Extensions;

using Pdoxcl2Sharp;

namespace CK2LandedTitlesManager.DataAccess.IO
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
                // TODO: Implement these
                case "allow":
                case "coat_of_arms":
                case "gain_effect":
                case "pagan_coa":
                    parser.ReadInsideBrackets((p) => { });
                    break;

                case "color":
                    LandedTitleEntity.PrimaryColour = parser.ReadIntList().ToArray();
                    break;

                case "color2":
                    LandedTitleEntity.SecondaryColour = parser.ReadIntList().ToArray();
                    break;

                case "caliphate":
                    LandedTitleEntity.IsCaliphate = parser.ReadBool();
                    break;

                case "capital":
                    LandedTitleEntity.CapitalId = parser.ReadInt32();
                    break;

                case "controls_religion":
                    LandedTitleEntity.ControlledReligionId = parser.ReadString();
                    break;

                case "creation_requires_capital":
                    LandedTitleEntity.CreationRequiresCapital = parser.ReadBool();
                    break;

                case "culture":
                    LandedTitleEntity.CultureId = parser.ReadString();
                    break;

                case "dignity":
                    LandedTitleEntity.Dignity = parser.ReadInt32();
                    break;

                case "female_names":
                    LandedTitleEntity.FemaleNames = parser.ReadStringList();
                    break;

                case "foa":
                    LandedTitleEntity.TitleFormOfAddress = parser.ReadString();
                    break;

                case "graphical_culture":
                    LandedTitleEntity.GraphicalCulture = parser.ReadString();
                    break;
                    
                case "has_top_de_jure_capital":
                    LandedTitleEntity.HasTopDeJureCapital = parser.ReadBool();
                    break;

                case "holy_order":
                    LandedTitleEntity.IsHolyOrder = parser.ReadBool();
                    break;

                case "holy_site":
                    LandedTitleEntity.HolySites.Add(parser.ReadString());
                    break;

                case "independent":
                    LandedTitleEntity.IsIndependent = parser.ReadBool();
                    break;

                case "landless":
                    LandedTitleEntity.IsLandless = parser.ReadBool();
                    break;

                case "location_ruler_title":
                    LandedTitleEntity.TitleContainsCapital = parser.ReadBool();
                    break;

                case "male_names":
                    LandedTitleEntity.MaleNames = parser.ReadStringList();
                    break;

                case "mercenary":
                    LandedTitleEntity.IsMercenaryGroup = parser.ReadBool();
                    break;

                case "mercenary_type":
                    LandedTitleEntity.MercenaryType = parser.ReadString();
                    break;

                case "monthly_income":
                    LandedTitleEntity.MonthlyIncome = parser.ReadInt32();
                    break;

                case "name_tier":
                    LandedTitleEntity.TitleNameTierId = parser.ReadString();
                    break;

                case "primary":
                    LandedTitleEntity.IsPrimaryTitle = parser.ReadBool();
                    break;

                case "purple_born_heirs":
                    LandedTitleEntity.HasPurpleBornHeirs = parser.ReadBool();
                    break;

                case "religion":
                    LandedTitleEntity.ReligionId = parser.ReadString();
                    break;

                case "short_name":
                    LandedTitleEntity.UseShortName = parser.ReadBool();
                    break;

                case "strength_growth_per_century":
                    LandedTitleEntity.StrengthGrowthPerCentury = parser.ReadFloat();
                    break;

                case "title":
                    LandedTitleEntity.TitleLocalisationId = parser.ReadString();
                    break;

                case "title_female":
                    LandedTitleEntity.TitleLocalisationFemaleId = parser.ReadString();
                    break;

                case "title_prefix":
                    LandedTitleEntity.TitleLocalisationPrefixId = parser.ReadString();
                    break;

                case "tribe":
                    LandedTitleEntity.IsTribe = parser.ReadBool();
                    break;

                default:
                    string stringValue = parser.ReadString();
                    int intValue;

                    if (int.TryParse(stringValue, out intValue))
                    {
                        LandedTitleEntity.ReligiousValues.AddOrUpdate(token, intValue);
                    }
                    else
                    {
                        LandedTitleEntity.DynamicNames.AddOrUpdate(token, stringValue);
                    }

                    break;
            }
        }
        
        public void Write(ParadoxStreamWriter writer)
        {
            foreach(var dynamicName in LandedTitleEntity.DynamicNames)
            {
                writer.WriteLine(dynamicName.Key, dynamicName.Value, ValueWrite.Quoted);
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
