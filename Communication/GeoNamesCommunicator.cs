using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

using NuciDAL.Repositories;
using NuciExtensions;

using CK2LandedTitlesManager.BusinessLogic;

namespace CK2LandedTitlesManager.Communication
{
    public sealed class GeoNamesCommunicator : IGeoNamesCommunicator
    {
        const string GeoNamesApiUrl = "http://api.geonames.org";
        const string NoResultsFoundPlaceholder = "NoResultsFound";

        DateTime lastCacheSave;

        readonly IEnumerable<string> Usernames = new List<string>
        { 
            "geonamesfreeaccountt", "freacctest1", "freacctest2", "commando.gaztons", "berestesievici", "izvoli.prostagma",
            "gesturioso", "random.name.ftw", "b75268", "b75375", "b75445", "b75445",
            "botu0", "botu1", "botu2", "botu3", "botu4", "botu5", "botu6", "botu7", "botu8", "botu9",
            "elBot0", "elBot1", "elBot2", "elBot3", "elBot4", "elBot5", "elBot6", "elBot7", "elBot8", "elBot9",
            "botman0", "botman1", "botman2", "botman3", "botman4", "botman5", "botman6", "botman7", "botman8", "botman9",
            "botean0", "botean1", "botean2", "botean3", "botean4", "botean5"
        };

        readonly HttpClient httpClient;
        
        readonly INameCleaner nameCleaner;

        readonly IRepository<CachedGeoNameExonym> cache;

        public GeoNamesCommunicator()
        {
            httpClient = new HttpClient();
            nameCleaner = new NameCleaner();
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

            string normalisedPlaceName = nameCleaner.Clean(placeName);
            string exonym = GetExonymFromCache(normalisedPlaceName, language);
            
            if (exonym is null)
            {
                exonym = await GetExonymFromApi(normalisedPlaceName, language);
            }

            string cleanExonym = nameCleaner.Clean(exonym);

            if (cleanExonym.Equals(normalisedPlaceName, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            return cleanExonym;
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
            string id = $"{NormaliseName(placeName)}_{language}";

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
            cachedExonym.Id = $"{NormaliseName(placeName)}_{language}";
            cachedExonym.PlaceName = placeName;
            cachedExonym.LanguageId = language;
            cachedExonym.Exonym = exonym ?? string.Empty;

            cache.Add(cachedExonym);

            if ((DateTime.Now - lastCacheSave).TotalSeconds > 20)
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
        }

        // TODO: The searchName parameter doesn't belong here
        async Task<string> DeserialiseSuccessResponse(HttpResponseMessage httpResponse, string searchName)
        {
            const string toponymNamePattern = "<toponymName>(.*)</toponymName>";
            const string alternateNamePattern = "<name>(.*)</name>";

            string responseString = await httpResponse.Content.ReadAsStringAsync();

            if (responseString.Contains("<totalResultsCount>0</totalResultsCount>"))
            {
                return NoResultsFoundPlaceholder;
            }

            string toponymName = Regex.Match(responseString, toponymNamePattern).Groups[1].Value;
            string alternateName = Regex.Match(responseString, alternateNamePattern).Groups[1].Value;

            string normalisedToponymName = NormaliseName(toponymName);
            string normalisedAlternateName = NormaliseName(alternateName);

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

        string NormaliseName(string name)
        {
            return nameCleaner.Clean(name)
                .Replace("-", " ")
                .Replace(" ", "_")
                .Replace("'", "")
                .Replace("Æ", "Ae")
                .Replace("æ", "ae")
                .Replace("ß", "ss")
                .ToLower();
        }
    }
}
