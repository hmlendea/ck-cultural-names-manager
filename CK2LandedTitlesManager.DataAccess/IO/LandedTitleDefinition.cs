using System.Linq;

using CK2LandedTitlesManager.DataAccess.DataObjects;
using CK2LandedTitlesManager.Infrastructure.Extensions;

using Pdoxcl2Sharp;

namespace CK2LandedTitlesManager.DataAccess.IO
{
    public sealed class LandedTitleDefinition : LandedTitleEntity, IParadoxRead, IParadoxWrite
    {
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
                return;
            }

            switch (token)
            {
                // TODO: Implement these
                case "allow":
                case "coat_of_arms":
                case "gain_effect":
                    parser.ReadInsideBrackets((p) => { });
                    break;

                case "color":
                    PrimaryColour = parser.ReadIntList().ToArray();
                    break;

                case "color2":
                    SecondaryColour = parser.ReadIntList().ToArray();
                    break;

                case "caliphate":
                    IsCaliphate = parser.ReadBool();
                    break;

                case "capital":
                    CapitalId = parser.ReadInt32();
                    break;

                case "culture":
                    CultureId = parser.ReadString();
                    break;

                case "dignity":
                    Dignity = parser.ReadInt32();
                    break;

                case "foa":
                    TitleFormOfAddress = parser.ReadString();
                    break;

                case "graphical_culture":
                    GraphicalCulture = parser.ReadString();
                    break;
                    
                case "has_top_de_jure_capital":
                    HasTopDeJureCapital = parser.ReadBool();
                    break;

                case "holy_order":
                    IsHolyOrder = parser.ReadBool();
                    break;

                case "landless":
                    IsLandless = parser.ReadBool();
                    break;

                case "mercenary":
                    IsMercenaryGroup = parser.ReadBool();
                    break;

                case "mercenary_type":
                    MercenaryType = parser.ReadString();
                    break;

                case "monthly_income":
                    MonthlyIncome = parser.ReadInt32();
                    break;

                case "name_tier":
                    TitleNameTierId = parser.ReadString();
                    break;

                case "primary":
                    IsPrimaryTitle = parser.ReadBool();
                    break;

                case "purple_born_heirs":
                    HasPurpleBornHeirs = parser.ReadBool();
                    break;

                case "religion":
                    ReligionId = parser.ReadString();
                    break;

                case "short_name":
                    UseShortName = parser.ReadBool();
                    break;

                case "title":
                    TitleLocalisationId = parser.ReadString();
                    break;

                case "title_female":
                    TitleLocalisationFemaleId = parser.ReadString();
                    break;

                case "title_prefix":
                    TitleLocalisationPrefixId = parser.ReadString();
                    break;

                case "tribe":
                    IsTribe = parser.ReadBool();
                    break;

                default:
                    string stringValue = parser.ReadString();
                    int intValue;

                    if (int.TryParse(stringValue, out intValue))
                    {
                        ReligiousValues.AddOrUpdate(token, intValue);
                    }
                    else
                    {
                        DynamicNames.AddOrUpdate(token, stringValue);
                    }

                    break;
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
