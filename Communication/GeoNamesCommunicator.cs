using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

using NuciDAL.Repositories;
using NuciExtensions;

namespace CK2LandedTitlesManager.Communication
{
    public sealed class GeoNamesCommunicator : IGeoNamesCommunicator
    {
        const string GeoNamesApiUrl = "http://api.geonames.org";

        DateTime lastCacheSave;

        readonly IEnumerable<string> Usernames = new List<string>
        { 
            "geonamesfreeaccountt", "freacctest1", "commando.gaztons", "berestesievici", "izvoli.prostagma",
            "gesturioso", "random.name.ftw", "b75268", "b75375", "b75445", "b75445",
            "botu0", "botu1", "botu2", "botu3", "botu4", "botu5", "botu6", "botu7", "botu8", "botu9",
            "elBot0", "elBot1", "elBot2", "elBot3", "elBot4", "elBot5", "elBot6", "elBot7", "elBot8", "elBot9",
            "botman0", "botman1", "botman2", "botman3", "botman4", "botman5", "botman6", "botman7", "botman8", "botman9",
        };

        readonly HttpClient httpClient;

        readonly IRepository<CachedGeoNameExonym> cache;

        public GeoNamesCommunicator()
        {
            httpClient = new HttpClient();
            cache = new CsvRepository<CachedGeoNameExonym>("geonames-cache.txt");

            lastCacheSave = DateTime.Now;
        }

        public async Task<string> TryGatherExonym(string placeName, string language)
        {
            try
            {
                return await GatherExonym(placeName, language);
            }
            catch
            {
                return null;
            }
        }

        public async Task<string> GatherExonym(string placeName, string language)
        {
            if (string.IsNullOrWhiteSpace(placeName))
            {
                throw new ArgumentNullException(placeName);
            }

            if (string.IsNullOrWhiteSpace(language))
            {
                throw new ArgumentNullException(language);
            }

            string normalisedPlaceName = NormalisePlaceName(placeName);
            string exonym = GetExonymFromCache(normalisedPlaceName, language);
            
            if (exonym is null)
            {
                exonym = await GetExonymFromApi(normalisedPlaceName, language);
            }

            if (CleanName(exonym).Equals(normalisedPlaceName, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            return CleanName(exonym);
        }

        async Task<string> GetExonymFromApi(string placeName, string language)
        {
            string endpoint = BuildRequestUrl(placeName, language);
            HttpResponseMessage httpResponse = await httpClient.GetAsync(endpoint);

            await ValdiateHttpRespone(httpResponse);

            string exonym = await DeserialiseSuccessResponse(httpResponse, placeName);

            SaveExonymInCache(placeName, language, exonym);

            return exonym;
        }

        string GetExonymFromCache(string placeName, string language)
        {
            string id = $"{placeName}_{language}";

            CachedGeoNameExonym cachedExonym = cache.TryGet(id);

            if (cachedExonym is null)
            {
                return null;
            }

            return cachedExonym.Exonym;
        }

        void SaveExonymInCache(string placeName, string language, string exonym)
        {
            CachedGeoNameExonym cachedExonym = new CachedGeoNameExonym();
            cachedExonym.Id = $"{placeName}_{language}";
            cachedExonym.PlaceName = placeName;
            cachedExonym.LanguageId = language;
            cachedExonym.Exonym = exonym ?? string.Empty;

            cache.Add(cachedExonym);

            if ((DateTime.Now - lastCacheSave).TotalSeconds > 10)
            {
                lastCacheSave = DateTime.Now;
                cache.ApplyChanges();
            }
        }

        string BuildRequestUrl(string placeName, string language)
        {
            return
                $"{GeoNamesApiUrl}/search" +
                $"?name={HttpUtility.UrlEncode(placeName)}" +
                //$"&cities=cities500" +
                $"&lang={language}" +
                $"&username={Usernames.GetRandomElement()}";
        }

        async Task ValdiateHttpRespone(HttpResponseMessage httpResponse)
        {
            string responseString = await httpResponse.Content.ReadAsStringAsync();

            if (responseString.Contains("status message"))
            {
                string errorMessage = await DeserialiseErrorResponse(httpResponse);
                throw new Exception(errorMessage);
            }

            if (responseString.Contains("<totalResultsCount>0</totalResultsCount>"))
            {
                throw new Exception("No results");
            }
        }

        // TODO: The searchName parameter doesn't belong here
        async Task<string> DeserialiseSuccessResponse(HttpResponseMessage httpResponse, string searchName)
        {
            const string toponymNamePattern = "<toponymName>(.*)</toponymName>";
            const string alternateNamePattern = "<name>(.*)</name>";

            string responseString = await httpResponse.Content.ReadAsStringAsync();
            string toponymName = Regex.Match(responseString, toponymNamePattern).Groups[1].Value;
            string alternateName = Regex.Match(responseString, alternateNamePattern).Groups[1].Value;

            alternateName = CleanName(alternateName);

            string normalisedToponymName = NormalisePlaceName(toponymName);
            string normalisedAlternateName = NormalisePlaceName(alternateName);

            if (normalisedToponymName == normalisedAlternateName ||
                normalisedAlternateName == searchName ||
                normalisedToponymName.Length != searchName.Length)
            {
                return null;
            }

            return alternateName;
        }

        async Task<string> DeserialiseErrorResponse(HttpResponseMessage httpResponse)
        {
            const string errorMessagePattern = "<status message=\"(.*)\" value";

            string responseString = await httpResponse.Content.ReadAsStringAsync();
            string errorMessage = Regex.Match(responseString, errorMessagePattern).Groups[1].Value;

            return errorMessage;
        }

        string NormalisePlaceName(string name)
        {
            string normalisedName = CleanName(name);

            normalisedName = Regex.Replace(normalisedName, "-|'", string.Empty);

            /*
            normalisedName = Regex.Replace(normalisedName, "âăãáä", "a");
            normalisedName = Regex.Replace(normalisedName, "čç", "c");
            normalisedName = Regex.Replace(normalisedName, "ď", "d");
            normalisedName = Regex.Replace(normalisedName, "ėéè", "e");
            normalisedName = Regex.Replace(normalisedName, "îí", "i");
            normalisedName = Regex.Replace(normalisedName, "ł", "l");
            normalisedName = Regex.Replace(normalisedName, "ñ", "n");
            normalisedName = Regex.Replace(normalisedName, "øðó", "o");
            normalisedName = Regex.Replace(normalisedName, "șš", "s");
            normalisedName = Regex.Replace(normalisedName, "ț", "t");
            normalisedName = Regex.Replace(normalisedName, "üú", "u");
            normalisedName = Regex.Replace(normalisedName, "ý", "y");
            normalisedName = Regex.Replace(normalisedName, "żž", "z");
            */

            return name.ToLower();
        }

        string CleanName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return name;
            }

            string cleanName = name
                .Split('/')[0]
                .Split('.')[0]
                .Split(',')[0]
                .Split('[')[0]
                .Split('(')[0];

            cleanName = Regex.Replace(cleanName, "_", " ");
            cleanName = Regex.Replace(cleanName, "Æ", "Ae");
            cleanName = Regex.Replace(cleanName, "æ", "ae");
            cleanName = Regex.Replace(cleanName, "ß", "ss");
            cleanName = Regex.Replace(cleanName, "ĳ", "ij");

            IEnumerable<string> removePatterns = new List<string>
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
                " miestas$",
                " pie \\p{Lu}\\p{Ll}*$",
                " valsčius$",
                " ved \\p{Lu}\\p{Ll}*$",
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
                "^Kreis ",
                "^Loster ",
                "^Lutherstadt ",
                "^Magaalada ",
                "^Mestna občina ",
                "^Mont ",
                "^Monte ",
                "^Powiat ",
                "^Prowincja ",
                "^St$",
                "^Statul "
            };

            string removePatternsCombined = "(";
            removePatternsCombined += string.Join("|", removePatterns);
            removePatternsCombined = removePatternsCombined.Substring(0, removePatternsCombined.Length - 1) + ")";

            cleanName = Regex.Replace(cleanName, removePatternsCombined, string.Empty);

            // non-Windows1252 characters
            cleanName = Regex.Replace(cleanName, "[Ă]", "Ã");
            cleanName = Regex.Replace(cleanName, "[İ]", "I");
            cleanName = Regex.Replace(cleanName, "[Ż]", "Z");
            cleanName = Regex.Replace(cleanName, "[ăā]", "ã");
            cleanName = Regex.Replace(cleanName, "[č]", "c");
            cleanName = Regex.Replace(cleanName, "[ď]", "d");
            cleanName = Regex.Replace(cleanName, "[ėē]", "e");
            cleanName = Regex.Replace(cleanName, "[ıī]", "i");
            cleanName = Regex.Replace(cleanName, "[ł]", "l");
            cleanName = Regex.Replace(cleanName, "[ș]", "s");
            cleanName = Regex.Replace(cleanName, "[ț]", "t");
            cleanName = Regex.Replace(cleanName, "[ū]", "u");
            cleanName = Regex.Replace(cleanName, "[ż]", "z");

            return cleanName.Trim();
        }
    }
}
