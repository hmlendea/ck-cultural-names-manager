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

            return await DeserialiseSuccessResponse(httpResponse);
        }

        string BuildRequestUrl(string placeName, string language)
        {
            return
                $"{GeoNamesApiUrl}/search" +
                $"?q={placeName}" +
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

        async Task<string> DeserialiseSuccessResponse(HttpResponseMessage httpResponse)
        {
            const string namePattern = "<name>(.*)</name>";

            string responseString = await httpResponse.Content.ReadAsStringAsync();
            string name = Regex.Match(responseString, namePattern).Groups[1].Value;

            return name;
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
