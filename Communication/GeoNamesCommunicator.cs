using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
            "geonamesfreeaccountt", "freacctest1", "freacctest2", "commando.gaztons", "berestesievici", "izvoli.prostagma",
            "gesturioso", "random.name.ftw", "b75268", "b75375", "b75445", "b75445",
            "botu0", "botu1", "botu2", "botu3", "botu4", "botu5", "botu6", "botu7", "botu8", "botu9",
            "elBot0", "elBot1", "elBot2", "elBot3", "elBot4", "elBot5", "elBot6", "elBot7", "elBot8", "elBot9",
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

            string exonym = FindExonymInCahce(placeName, language);

            if (!(exonym is null))
            {
                return exonym;
            }

            string endpoint = BuildRequestUrl(placeName, language);
            HttpResponseMessage httpResponse = await httpClient.GetAsync(endpoint);

            await ValdiateHttpRespone(httpResponse);

            exonym = await DeserialiseSuccessResponse(httpResponse, placeName);

            SaveExonymInCache(placeName, language, exonym);

            return exonym;
        }

        string FindExonymInCahce(string placeName, string language)
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
                $"?name={placeName}" +
                $"&cities=cities500" +
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
                searchName == normalisedAlternateName ||
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

            normalisedName = Regex.Replace(normalisedName, "-|_|'|,|.", "");
            normalisedName = Regex.Replace(normalisedName, "Æ|æ", "ae");
            normalisedName = Regex.Replace(normalisedName, "ß", "ss");

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

            return name;
        }

        string CleanName(string name)
        {
            string cleanName = name
                .Split('/')[0]
                .Split(',')[0]
                .Split('[')[0]
                .Split('(')[0];

            cleanName = Regex.Replace(cleanName, "Æ|æ", "ae");
            cleanName = Regex.Replace(cleanName, "ß", "ss");

            cleanName = Regex.Replace(cleanName, " BL$", "");
            cleanName = Regex.Replace(cleanName, " bykommune$", "");
            cleanName = Regex.Replace(cleanName, " ili$", "");
            cleanName = Regex.Replace(cleanName, " kommune$", "");
            cleanName = Regex.Replace(cleanName, " miestas$", "");
            cleanName = Regex.Replace(cleanName, " SH$", "");
            cleanName = Regex.Replace(cleanName, " TG$", "");
            cleanName = Regex.Replace(cleanName, " valsčius$", "");
            cleanName = Regex.Replace(cleanName, "^Abbaye d'", "");
            cleanName = Regex.Replace(cleanName, "^Campo di sterminio di ", "");
            cleanName = Regex.Replace(cleanName, "^Cathair ", "");
            cleanName = Regex.Replace(cleanName, "^Ciudad de ", "");
            cleanName = Regex.Replace(cleanName, "^Ciutat d'", "");
            cleanName = Regex.Replace(cleanName, "^Ciutat de ", "");
            cleanName = Regex.Replace(cleanName, "^Comuna de ", "");
            cleanName = Regex.Replace(cleanName, "^Dinas ", "");
            cleanName = Regex.Replace(cleanName, "^Districtul ", "");
            cleanName = Regex.Replace(cleanName, "^Estado de ", "");
            cleanName = Regex.Replace(cleanName, "^Gmina ", "");
            cleanName = Regex.Replace(cleanName, "^Loster ", "");
            cleanName = Regex.Replace(cleanName, "^Magaalada ", "");
            cleanName = Regex.Replace(cleanName, "^Mestna občina ", "");
            cleanName = Regex.Replace(cleanName, "^Prowincja ", "");
            cleanName = Regex.Replace(cleanName, "^Statul ", "");

            return cleanName.Trim();
        }
    }
}
