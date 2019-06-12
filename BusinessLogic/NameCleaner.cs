using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

using NuciExtensions;

using CK2LandedTitlesManager.BusinessLogic;

namespace CK2LandedTitlesManager.BusinessLogic
{
    public sealed class NameCleaner : INameCleaner
    {
        readonly IDictionary<string, string> normalisationCache;
        readonly IDictionary<string, string> cleaningCache;

        readonly INameTransliterator nameTransliterator;

        public NameCleaner()
        {
            normalisationCache = new Dictionary<string, string>();
            cleaningCache = new Dictionary<string, string>();

            nameTransliterator = new NameTransliterator();

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
                .Replace(".", "")
                .Replace("'", "")
                .Replace("-", " ")
                .Replace("Saint ", "St ")
                .Replace(" ", "_")
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
            string utf8Name = Encoding.UTF8.GetString(bytes);
            string cleanName = nameTransliterator.Transliterate(utf8Name)
                .Replace("_", " ")
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

            compliantName = Regex.Replace(compliantName, "[ď]", "d'");
            compliantName = Regex.Replace(compliantName, "[ľ]", "l'");
            compliantName = Regex.Replace(compliantName, "[ť]", "t'");
            compliantName = Regex.Replace(compliantName, "[ĳ]", "ij");

            compliantName = Regex.Replace(compliantName, "[АΑĄ]", "A");
            compliantName = Regex.Replace(compliantName, "[Б]", "B");
            compliantName = Regex.Replace(compliantName, "[Č]", "C");
            compliantName = Regex.Replace(compliantName, "[Д]", "D");
            compliantName = Regex.Replace(compliantName, "[ЕĖĒĚ]", "E");
            compliantName = Regex.Replace(compliantName, "[Ф]", "F");
            compliantName = Regex.Replace(compliantName, "[Ģ]", "G");
            compliantName = Regex.Replace(compliantName, "[Ḩ]", "H");
            compliantName = Regex.Replace(compliantName, "[İĪ]", "I");
            compliantName = Regex.Replace(compliantName, "[Ķ]", "K");
            compliantName = Regex.Replace(compliantName, "[ŁĻ]", "L");
            compliantName = Regex.Replace(compliantName, "[М]", "M");
            compliantName = Regex.Replace(compliantName, "[ŃŇŅН]", "N");
            compliantName = Regex.Replace(compliantName, "[ОΟŌ]", "O");
            compliantName = Regex.Replace(compliantName, "[П]", "P");
            compliantName = Regex.Replace(compliantName, "[ŘР]", "R");
            compliantName = Regex.Replace(compliantName, "[ȘŞŚСΣ]", "S");
            compliantName = Regex.Replace(compliantName, "[ТΤȚŢ]", "T");
            compliantName = Regex.Replace(compliantName, "[ŪŬ]", "U");
            compliantName = Regex.Replace(compliantName, "[Ŷ]", "Y");
            compliantName = Regex.Replace(compliantName, "[Ż]", "Z");
            
            compliantName = Regex.Replace(compliantName, "[аąα]", "a");
            compliantName = Regex.Replace(compliantName, "[б]", "b");
            compliantName = Regex.Replace(compliantName, "[č]", "c");
            compliantName = Regex.Replace(compliantName, "[д]", "d");
            compliantName = Regex.Replace(compliantName, "[еėēě]", "e");
            compliantName = Regex.Replace(compliantName, "[ф]", "f");
            compliantName = Regex.Replace(compliantName, "[ğģ]", "g");
            compliantName = Regex.Replace(compliantName, "[ḩ]", "h");
            compliantName = Regex.Replace(compliantName, "[ıīі]", "i");
            compliantName = Regex.Replace(compliantName, "[к]", "k");
            compliantName = Regex.Replace(compliantName, "[łļļ]", "l");
            compliantName = Regex.Replace(compliantName, "[м]", "m");
            compliantName = Regex.Replace(compliantName, "[ńňņнν]", "n");
            compliantName = Regex.Replace(compliantName, "[оοō]", "o");
            compliantName = Regex.Replace(compliantName, "[п]", "p");
            compliantName = Regex.Replace(compliantName, "[řр]", "r");
            compliantName = Regex.Replace(compliantName, "[șşśс]", "s");
            compliantName = Regex.Replace(compliantName, "[тτțţ]", "t");
            compliantName = Regex.Replace(compliantName, "[ūŭ]", "u");
            compliantName = Regex.Replace(compliantName, "[ƿѡ]", "w");
            compliantName = Regex.Replace(compliantName, "[уŷ]", "y");
            compliantName = Regex.Replace(compliantName, "[ż]", "z");

            compliantName = Regex.Replace(compliantName, "[ĂĀ]", "Ã");
            compliantName = Regex.Replace(compliantName, "[Ő]", "Ö");

            compliantName = Regex.Replace(compliantName, "[ăā]", "ã");
            compliantName = Regex.Replace(compliantName, "[ő]", "ö");

            return compliantName;
        }

        string BuildRemovalPattern()
        {
            string removalPattern = "(";
            removalPattern += string.Join("|", cleaningRemovalPatterns);
            removalPattern = removalPattern.Substring(0, removalPattern.Length - 1) + ")";

            return removalPattern;
        }

        string Transliterate(string name)
        {
            string transliteratedName = name;

            return transliteratedName;
        }

        readonly string RemovalPattern;
            
        readonly IEnumerable<string> cleaningRemovalPatterns = new List<string>
        {
            "^[CK]ant[oó]n[ -]",
            "^[CK]aste[l]*[o]*[ -']",
            "^[CK]o[m]*un[ae]* ",
            "^[IÎ][l]*e[s]*[ -]",
            "^[SŠ]ampanes ",
            "^Abbaye d'",
            "^Afon ",
            "^Arondisamant ",
            "^Arrondissement d[ei] ",
            "^Bad ",
            "^Betelried ",
            "^Bezirk ",
            "^Byen ",
            "^Camp de ",
            "^Campo de concentración ",
            "^Campo di sterminio di ",
            "^Can[pt][oó] d[ei] ",
            "^Cantone di ",
            "^Cathair ",
            "^Cerro ",
            "^Château de la ",
            "^Circondario del ",
            "^Circondario di ",
            "^Circulus ",
            "^Città di ",
            "^Ciu[dt]a[dt] d[e']*",
            "^Cúige ",
            "^Dinas ",
            "^Distr[ei][ct]*[eou][l]* ",
            "^Dvorac ",
            "^Esta[dt][o]* ",
            "^Fanum ",
            "^Försterei ",
            "^Freie Hansestod ",
            "^Gemeen ",
            "^Gmina ",
            "^Isole ",
            "^Kloster ",
            "^Knížectví ",
            "^Koncentračný tábor ",
            "^Konzentrationslager ",
            "^Kreis ",
            "^Lagărul de exterminare ",
            "^Landkreis ",
            "^Loster ",
            "^Lutherstadt[ -]",
            "^Magaalada ",
            "^Mestna ob[cč]ina ",
            "^Mont[e]*[ -'’]",
            "^Montañas ",
            "^Municipalità ",
            "^Oase ",
            "^Obcina ",
            "^Obsjtina ",
            "^Powiat ",
            "^Pr[ou][vw][aií][nñ][csz]*[eijo]*[aen]*[ -'’]",
            "^Re[s]*publica ",
            "^Río ",
            "^Ruine ",
            "^Schlo[sßt]+[e]*[ -'’]",
            "^Seno ",
            "^Södra ",
            "^Statul ",
            "^Vi[l]*a[ -]",
            "^Zemský okres ",


            " [Aa]dalari$",
            " [Cc]ity$",
            " [Cc]o[ou]nty$",
            " [Mm]aakond$",
            " bykommune$",
            " civitas$",
            " Golf Club$",
            " grad$",
            " hiria$",
            " ili$",
            " járás$",
            " keskitysleiri$",
            " koncentrationslejr$",
            " mäed$",
            " megye$",
            " miestas$",
            " montes$",
            " provin[ct][eis][j]*[a]*$",
            " saare[dt]$",
            " valsčius$",
            " vár$",
            "[ -][ck]o[m]*un[e]*$",
            "[ -][Ee]ilan[dn]en$",
            "[ -][Ss]alos$",
            "[ -]åsene$",
            "[ -]hegység$",
            "[ -]hérað$",
            "[ -]Lutherstadt$",
            "[ -]Oderi ääres$",
            "[ -]szigetek$",
            "i liidumaa$",
            "i ringkond$",
            "i vald$",
            "ko probintzia$",
            "n maakunta$",
            "ski[ -][Oo]toci$",

            " ringkond$",
            " saar$",
            " vald$",


            " [Vv]eliki[j]*$",


            " Moldawski$",
            "^[CČ][z]*esk[aáeéyý] ",
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
            "^[A-Z][A-Z][A-Z] ",
            "^St$",

            " [a]*n d[ae]([ -]\\p{Lu}\\p{Ll}*)+$",
            " [aie]([ -]\\p{Lu}\\p{Ll}*)+$",
            " [ií]([ -]\\p{Lu}\\p{Ll}*)+$",
            " an['’]n([ -]\\p{Lu}\\p{Ll}*)+$",
            " bei([ -]\\p{Lu}\\p{Ll}*)+$",
            " d['’]\\p{Lu}\\p{Ll}*$",
            " d['’]o([ -]\\p{Lu}\\p{Ll}*)+$",
            " d[eio]([ -]\\p{Lu}\\p{Ll}*)+$",
            " d\\p{Lu}\\p{Ll}*$",
            " dau([ -]\\p{Lu}\\p{Ll}*)+$",
            " de l['’]\\p{Lu}\\p{Ll}*$",
            " de[l]*[a]*([ -]\\p{Lu}\\p{Ll}*)+$",
            " i[mn]([ -]\\p{Lu}\\p{Ll}*)+$",
            " in der([ -]\\p{Lu}\\p{Ll}*)+$",
            " na[d]*([ -]\\p{Lu}\\p{Ll}*)+$",
            " o[bgn]([ -]\\p{Lu}\\p{Ll}*)+$",
            " ob der([ -]\\p{Lu}\\p{Ll}*)+$",
            " p[åe]([ -]\\p{Lu}\\p{Ll}*)+$",
            " p[r]*ie([ -]\\p{Lu}\\p{Ll}*)+$",
            " s[uü][lr]([ -]\\p{Lu}\\p{Ll}*)+$",
            " sulla([ -]\\p{Lu}\\p{Ll}*)+$",
            " ved([ -]\\p{Lu}\\p{Ll}*)+$",
            " vorm([ -]\\p{Lu}\\p{Ll}*)+$",
            "-[lp]e-\\p{Lu}\\p{Ll}*$",
            "-l['’]\\p{Lu}\\p{Ll}*$",
            "-na-\\p{Lu}\\p{Ll}*$",
            "-s[ou]*[rs]*-\\p{Lu}\\p{Ll}*$",
            "-sur-l[e'’]*([ -]*\\p{Lu}\\p{Ll}*)+$",
            "([ -]\\p{Lu}\\p{Ll}*)+$ ääres$",
            "[ -][Aa][dmn]([ -]\\p{Lu}\\p{Ll}*)+$",
            "[ -][ae]n[ -]*[el]*[al]*([ -]\\p{Lu}\\p{Ll}*)+$",
            "[ -]an[ -]der([ -]\\p{Lu}\\p{Ll}*)+$",
            "[ -]ar([ -]\\p{Lu}\\p{Ll}*)+$",
            "[ -]nel([ -]\\p{Lu}\\p{Ll}*)+$",
            "[ -]s[ou][bp][er][eroô][ -]*[o]*([ -]\\p{Lu}\\p{Ll}*)+$",


            " by$",
            " i$",
            "^[d]*[eo] ",
        };
    }
}