using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CK2LandedTitlesManager.Communication
{
    public sealed class GeoNamesCommunicator : IGeoNamesCommunicator
    {
        const string GeoNamesApiUrl = "http://api.geonames.org";

        readonly HttpClient httpClient;

        public GeoNamesCommunicator()
        {
            httpClient = new HttpClient();
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

            string endpoint = BuildRequestUrl(placeName, language);
            HttpResponseMessage httpResponse = await httpClient.GetAsync(endpoint);
            
            await ValdiateHttpRespone(httpResponse);

            return await DeserialiseSuccessResponse(httpResponse, placeName);
        }

        string BuildRequestUrl(string placeName, string language)
        {
            return
                $"{GeoNamesApiUrl}/search" +
                $"?name={placeName}" +
                $"&cities=cities15000" +
                $"&lang={language}" +
                $"&username=geonamesfreeaccountt";
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

            if (toponymName == alternateName ||
                toponymName.Length != searchName.Length)
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
    }
}
