using System.Collections.Generic;
using System.Text;
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
                .Replace("œ", "oe")
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
            
            byte[] bytes = Encoding.Default.GetBytes(name);
            string cleanName = Encoding.UTF8.GetString(bytes)
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

            while (true)
            {
                string newCleanName = Regex.Replace(cleanName, RemovalPattern, string.Empty);

                if (newCleanName == cleanName)
                {
                    break;
                }

                cleanName = newCleanName;
            }

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

            compliantName = Regex.Replace(compliantName, "[ľ]", "l'");
            compliantName = Regex.Replace(compliantName, "[ť]", "t'");

            compliantName = Regex.Replace(compliantName, "[ĂĀ]", "Ã");
            compliantName = Regex.Replace(compliantName, "[Č]", "C");
            compliantName = Regex.Replace(compliantName, "[ĖĒĚ]", "E");
            compliantName = Regex.Replace(compliantName, "[İĪ]", "I");
            compliantName = Regex.Replace(compliantName, "[ŁĻ]", "L");
            compliantName = Regex.Replace(compliantName, "[Ř]", "R");
            compliantName = Regex.Replace(compliantName, "[Ș]", "S");
            compliantName = Regex.Replace(compliantName, "[ȚŢ]", "T");
            compliantName = Regex.Replace(compliantName, "[ŪŬ]", "U");
            compliantName = Regex.Replace(compliantName, "[Ż]", "Z");
            compliantName = Regex.Replace(compliantName, "[ăā]", "ã");
            compliantName = Regex.Replace(compliantName, "[č]", "c");
            compliantName = Regex.Replace(compliantName, "[ď]", "d");
            compliantName = Regex.Replace(compliantName, "[ėēě]", "e");
            compliantName = Regex.Replace(compliantName, "[ğ]", "g");
            compliantName = Regex.Replace(compliantName, "[ıīі]", "i");
            compliantName = Regex.Replace(compliantName, "[ĳ]", "ij");
            compliantName = Regex.Replace(compliantName, "[łļļ]", "l");
            compliantName = Regex.Replace(compliantName, "[ńňņ]", "n");
            compliantName = Regex.Replace(compliantName, "[ř]", "r");
            compliantName = Regex.Replace(compliantName, "[șş]", "s");
            compliantName = Regex.Replace(compliantName, "[țţ]", "t");
            compliantName = Regex.Replace(compliantName, "[ūŭ]", "u");
            compliantName = Regex.Replace(compliantName, "[ƿ]", "w");
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
            "^Abbaye d'",
            "^Afon ",
            "^Arondisamant ",
            "^Arrondissement de ",
            "^Arrondissement di ",
            "^Bad ",
            "^Betelried ",
            "^Bezirk ",
            "^Byen ",
            "^Camp de ",
            "^Campo de concentración ",
            "^Campo di sterminio di ",
            "^Cantó de ",
            "^Canton ",
            "^Cantón de ",
            "^Cantone di ",
            "^Castell ",
            "^Castell'",
            "^Cathair ",
            "^Cerro ",
            "^Château de la ",
            "^Circondario del ",
            "^Circondario di ",
            "^Circulus ",
            "^Città di ",
            "^Città di ",
            "^Ciudad de ",
            "^Ciutat d'",
            "^Ciutat de ",
            "^Comuna ",
            "^Comuna de ",
            "^Dinas ",
            "^Distretto di ",
            "^Districte de ",
            "^Districtul ",
            "^Distrito de ",
            "^Dvorac ",
            "^Estado de ",
            "^Estat d'",
            "^Försterei ",
            "^Freie Hansestod ",
            "^Gemeen ",
            "^Gmina ",
            "^Kanton ",
            "^Kastell ",
            "^Kloster ",
            "^Kommun ",
            "^Koncentračný tábor ",
            "^Konzentrationslager ",
            "^Kreis ",
            "^Lagărul de exterminare ",
            "^Landkreis ",
            "^Loster ",
            "^Lutherstadt ",
            "^Magaalada ",
            "^Mestna občina ",
            "^Mont ",
            "^Montañas ",
            "^Monte ",
            "^Municipalità di ",
            "^Obsjtina ",
            "^Powiat ",
            "^Provincia d",
            "^Prowincja ",
            "^Republica ",
            "^Respublica ",
            "^Río ",
            "^Ruine ",
            "^Schloss ",
            "^Schloß ",
            "^Schlote ",
            "^Seno ",
            "^Södra ",
            "^St$",
            "^Statul ",
            "^Zemský okres ",


            " [Cc]ity$",
            " [Cc]oonty$",
            " [Mm]aakond$",
            " bykommune$",
            " civitas$",
            " grad$",
            " hiria$",
            " ili$",
            " járás$",
            " keskitysleiri$",
            " kommun$",
            " kommune$",
            " koncentrationslejr$",
            " mäed$",
            " megye$",
            " miestas$",
            " montes$",
            " saar$",
            " valsčius$",
            " vár$",
            "-hegység$",
            "-hérað$",
            "-Lutherstadt$",
            "i liidumaa$",
            "i ringkond$",
            "i vald$",

            " ringkond$",
            " vald$",


            " [Vv]eliki$",
            " [Vv]elikij$",


            " Moldawski$",
            "^[CČ]esk[aáeéyý] ",
            "^Bosanski ",
            "^Czeska ",
            "^Nagy Magyar ",
            "^Schwäbisch ",
            "^Švabijos ",
            "^Wendisch ",


            " - .*",
            " [A-Z][A-Z]$",
            " [ab]$",
            " i\\.d\\. OPf\\.$",
            " i\\.d\\.OPf\\.$",
            "·",
            "^[A-Z][A-Z] ",


            " [ií] \\p{Lu}\\p{Ll}*-\\p{Lu}\\p{Ll}*$",
            " [ií] \\p{Lu}\\p{Ll}*$",
            " \\p{Lu}\\p{Ll}* ääres$",
            " a[dmn] \\p{Lu}\\p{Ll}* \\p{Lu}\\p{Ll}*$",
            " a[dmn] \\p{Lu}\\p{Ll}*$",
            " aan de \\p{Lu}\\p{Ll}*$",
            " an da \\p{Lu}\\p{Ll}*$",
            " an der \\p{Lu}\\p{Ll}*$",
            " an['’]n] \\p{Lu}\\p{Ll}*$",
            " bei \\p{Lu}\\p{Ll}*$",
            " d['’]\\p{Lu}\\p{Ll}*$",
            " d['’]o \\p{Lu}\\p{Ll}*$",
            " d[eio] \\p{Lu}\\p{Ll}*$",
            " d\\p{Lu}\\p{Ll}*$",
            " de l['’]\\p{Lu}\\p{Ll}*$",
            " del \\p{Lu}\\p{Ll}*$",
            " en \\p{Lu}\\p{Ll}*$",
            " en el \\p{Lu}\\p{Ll}* \\p{Lu}\\p{Ll}*$",
            " en el \\p{Lu}\\p{Ll}*$",
            " en la \\p{Lu}\\p{Ll}* \\p{Lu}\\p{Ll}*$",
            " i[mn] \\p{Lu}\\p{Ll}*$",
            " in der \\p{Lu}\\p{Ll}*$",
            " na \\p{Lu}\\p{Ll}*$",
            " nad \\p{Lu}\\p{Ll}*$",
            " ob der \\p{Lu}\\p{Ll}*$",
            " o[gn] \\p{Lu}\\p{Ll}*$",
            " pe \\p{Lu}\\p{Ll}*$",
            " pie \\p{Lu}\\p{Ll}*$",
            " prie \\p{Lu}\\p{Ll}*$",
            " s[uü][lr] \\p{Lu}\\p{Ll}*$",
            " sulla \\p{Lu}\\p{Ll}*$",
            " suprô \\p{Lu}\\p{Ll}*$",
            " ved \\p{Lu}\\p{Ll}*$",
            " vorm \\p{Lu}\\p{Ll}*$",
            "-[Aa]m-\\p{Lu}\\p{Ll}*$",
            "-[ae]n-\\p{Lu}\\p{Ll}*$",
            "-[lp]e-\\p{Lu}\\p{Ll}*$",
            "-an-der-\\p{Lu}\\p{Ll}*$",
            "-ar-\\p{Lu}\\p{Ll}*$",
            "-l['’]\\p{Lu}\\p{Ll}*$",
            "-na-\\p{Lu}\\p{Ll}*$",
            "-sur-\\p{Lu}\\p{Ll}*$",
            "-sur-l['’]\\p{Lu}\\p{Ll}*$",
            "-sur-l\\p{Lu}\\p{Ll}*$",
            "-sur-le-\\p{Lu}\\p{Ll}*$",


            " by$",
            " i$",
            "^de ",
        };
    }
}