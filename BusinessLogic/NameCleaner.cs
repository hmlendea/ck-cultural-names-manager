using System.Collections.Generic;
using System.Text.RegularExpressions;

using NuciExtensions;

namespace CK2LandedTitlesManager.BusinessLogic
{
    public sealed class NameCleaner : INameCleaner
    {
        readonly IDictionary<string, string> normalisationCache;
        readonly IDictionary<string, string> cleaningCache;

        public NameCleaner()
        {
            normalisationCache = new Dictionary<string, string>();
            cleaningCache = new Dictionary<string, string>();

            RemovalPattern = BuildRemovalPattern();
        }

        public string Normalise(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return name;
            }

            if (normalisationCache.ContainsKey(name))
            {
                return normalisationCache[name];
            }

            string normalisedName = Clean(name)
                .Replace("-", " ")
                .Replace(" ", "_")
                .Replace("'", "")
                .Replace("Æ", "Ae")
                .Replace("æ", "ae")
                .Replace("ß", "ss")
                .RemoveDiacritics()
                .ToLower();
            
            normalisationCache.Add(name, normalisedName);
            
            return normalisedName;
        }

        public string Clean(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return name;
            }

            if (cleaningCache.ContainsKey(name))
            {
                return cleaningCache[name];
            }

            string cleanName = name
                .Replace("_", " ")
                .Replace("NoResultsFound", "")
                .Split('/')[0]
                .Split('.')[0]
                .Split(',')[0]
                .Split(';')[0]
                .Split('[')[0]
                .Split('(')[0];
            
            if (string.IsNullOrWhiteSpace(cleanName))
            {
                return cleanName;
            }

            cleanName = Regex.Replace(cleanName, RemovalPattern, string.Empty);
            cleanName = ReplaceNonWindows1252Characters(cleanName);
            cleanName = cleanName.Trim();

            cleaningCache.Add(name, cleanName);

            return cleanName;
        }

        string ReplaceNonWindows1252Characters(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return name;
            }

            string compliantName = name;

            compliantName = Regex.Replace(compliantName, "[ĂĀ]", "Ã");
            compliantName = Regex.Replace(compliantName, "[ĖĒ]", "E");
            compliantName = Regex.Replace(compliantName, "[İĪ]", "I");
            compliantName = Regex.Replace(compliantName, "[Ł]", "L");
            compliantName = Regex.Replace(compliantName, "[Ș]", "S");
            compliantName = Regex.Replace(compliantName, "[Ț]", "T");
            compliantName = Regex.Replace(compliantName, "[Ū]", "U");
            compliantName = Regex.Replace(compliantName, "[Ż]", "Z");
            compliantName = Regex.Replace(compliantName, "[ăā]", "ã");
            compliantName = Regex.Replace(compliantName, "[č]", "c");
            compliantName = Regex.Replace(compliantName, "[ď]", "d");
            compliantName = Regex.Replace(compliantName, "[ėē]", "e");
            compliantName = Regex.Replace(compliantName, "[ıī]", "i");
            compliantName = Regex.Replace(compliantName, "[ĳ]", "ij");
            compliantName = Regex.Replace(compliantName, "[ł]", "l");
            compliantName = Regex.Replace(compliantName, "[ň]", "n");
            compliantName = Regex.Replace(compliantName, "[ș]", "s");
            compliantName = Regex.Replace(compliantName, "[ț]", "t");
            compliantName = Regex.Replace(compliantName, "[ū]", "u");
            compliantName = Regex.Replace(compliantName, "[ż]", "z");

            return compliantName;
        }

        string BuildRemovalPattern()
        {
            string removalPattern = "(";
            removalPattern += string.Join("|", cleaningRemovalPatterns);
            removalPattern = removalPattern.Substring(0, removalPattern.Length - 1) + ")";

            return removalPattern;
        }

        readonly string RemovalPattern;
            
        readonly IEnumerable<string> cleaningRemovalPatterns = new List<string>
        {
            " - .*",
            " [A-Z][A-Z]$",
            " am \\p{Lu}\\p{Ll}* See$",
            " am \\p{Lu}\\p{Ll}*$",
            " an der \\p{Lu}\\p{Ll}*$",
            " bykommune$",
            " civitas$",
            " d\\p{Lu}\\p{Ll}*$",
            " de \\p{Lu}\\p{Ll}*$",
            " ili$",
            " im \\p{Lu}\\p{Ll}*$",
            " in \\p{Lu}\\p{Ll}*$",
            " kommune$",
            " mäed$",
            " miestas$",
            " nad \\p{Lu}\\p{Ll}*$",
            " pie \\p{Lu}\\p{Ll}*$",
            " saar$",
            " valsčius$",
            " ved \\p{Lu}\\p{Ll}*$",
            "-hegység$",
            "^Abbaye d'",
            "^Arrondissement de ",
            "^Campo di sterminio di ",
            "^Cathair ",
            "^Circondario del ",
            "^Ciudad de ",
            "^Ciutat d'",
            "^Ciutat de ",
            "^Comuna de ",
            "^Dinas ",
            "^Districte de ",
            "^Districtul ",
            "^Distrito de ",
            "^Estado de ",
            "^Gemeen ",
            "^Gmina ",
            "^Kanton ",
            "^Kreis ",
            "^Loster ",
            "^Lutherstadt ",
            "^Magaalada ",
            "^Mestna občina ",
            "^Mont ",
            "^Montañas ",
            "^Monte ",
            "^Obsjtina ",
            "^Powiat ",
            "^Prowincja ",
            "^St$",
            "^Statul ",
            "i liidumaa$",
            "i vald$",
        };
    }
}