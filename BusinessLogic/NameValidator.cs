using System.Collections.Generic;
using System.Linq;

namespace CK2LandedTitlesManager.BusinessLogic  
{
    public sealed class NameValidator : INameValidator
    {
        public bool IsNameValid(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return false;
            }
            
            if (!name.All(c => AllowedCharacters.Contains(c)))
            {
                return false;
            }

            return true;
        }

        public bool IsNameValid(string name, string cultureId)
        {
            if (!IsNameValid(name))
            {
                return false;
            }

            if (DisallowedCharactersPerCulture.ContainsKey(cultureId))
            {
                return name.ToLower().All(c => !DisallowedCharactersPerCulture[cultureId].Contains(c));
            }

            return true;
        }
        
        readonly string AllowedCharacters =
            "\".,'‘’“”`÷×- " +
            "ßÞþð" +
            "AÁÀÂÅÄÃÆBCÇDÐEÉÈÊËFGHIÍÌÎÏJKLMNÑOÓÒÔÖÕØŒPQRSŠTUÚÙÛÜVWXYÝŸZŽ" +
            "aáàâåäãæbcçdeéèêëfghiíìîïjklmnñoóòôöõøœpqrsštuúùûüvwxyýÿzž";
        
        readonly IDictionary<string, string> DisallowedCharactersPerCulture = new Dictionary<string, string>()
        {
            { "bavarian", "ížšñçã" },
            { "bosnian", "üåæíñßþçã" },
            { "bulgarian", "üåæíñßþçã" },
            { "carantanian", "üåæíñßþçã" },
            { "croatian", "üåæíñßþçã" },
            { "german", "ížšñçã" },
            { "italian", "üåæçðžøœšñßžÞýþçãš" },
            { "langobardisch", "ížšñçã" },
            { "low_frankish", "ížšñçã" },
            { "low_german", "ížšñçã" },
            { "low_saxon", "ížšñçã" },
            { "romanian", "üåæçðžøœšñßžÞýþçší" },
            { "serbian", "üåæíñßþçã" },
            { "swabian", "ížšñçã" },
            { "swedish", "ížšñçã" },
            { "thuringian", "ížšñçã" },
        };
    }
}
