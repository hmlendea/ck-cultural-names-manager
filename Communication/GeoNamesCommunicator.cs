using System;
using System.Collections.Generic;
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

        readonly IEnumerable<string> Usernames = new List<string>
        { 
            "geonamesfreeaccountt", "freacctest1", "freacctest2", "commando.gaztons", "berestesievici", "izvoli.prostagma",
            "gesturioso", "random.name.ftw", "botu1", "botu2", "botu3", "botu4", "botu5", "botu6", "botu7", "botu8", "botu9"
        };

        readonly HttpClient httpClient;

        readonly IRepository<CachedGeoNameExonym> cache;

        public GeoNamesCommunicator()
        {
            httpClient = new HttpClient();
            cache = new CsvRepository<CachedGeoNameExonym>("geonames-cache.txt");
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
            cache.ApplyChanges();
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

            string normalisedToponymName = NormalisePlaceName(toponymName);
            string normalisedAlternateName = NormalisePlaceName(alternateName);

            if (searchName.RemoveDiacritics() == searchName &&
                normalisedToponymName.RemoveDiacritics() == normalisedToponymName &&
                searchName.Equals(normalisedToponymName, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            if (normalisedToponymName == normalisedAlternateName ||
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
            return name
                .Replace(" ", "")
                .Replace("-", "")
                .Split('/')[0]
                .ToLower();
        }
    }
}
