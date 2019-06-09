using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using NuciDAL.Repositories;
using NuciExtensions;

using CK2LandedTitlesManager.BusinessLogic.Models;

namespace CK2LandedTitlesManager.BusinessLogic
{
    public sealed class GeoNamesCommunicator : IGeoNamesCommunicator
    {
        const string GeoNamesApiUrl = "http://api.geonames.org";

        DateTime lastCacheSave;

        readonly IEnumerable<string> usernames = new List<string>
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

        readonly IRepository<CachedGeoNameExonym> nameCache;
        readonly IDictionary<string, string> responseCache;

        readonly ILocalisationProvider localisationProvider;

        public GeoNamesCommunicator(ILocalisationProvider localisationProvider)
        {
            httpClient = new HttpClient();
            nameCleaner = new NameCleaner();

            nameCache = new CsvRepository<CachedGeoNameExonym>("geonames-cache.txt");
            responseCache = new Dictionary<string, string>();

            lastCacheSave = DateTime.Now;

            this.localisationProvider = localisationProvider;
        }

        public async Task<string> TryGatherExonym(string titleId, string cultureId)
        {
            try
            {
                return await GatherExonym(titleId, cultureId);
            }
            catch
            {
                return null;
            }
        }

        public async Task<string> GatherExonym(string titleId, string cultureId)
        {
            if (string.IsNullOrWhiteSpace(titleId))
            {
                throw new ArgumentNullException(titleId);
            }

            if (string.IsNullOrWhiteSpace(cultureId))
            {
                throw new ArgumentNullException(cultureId);
            }

            string searchName = localisationProvider.GetLocalisation(titleId);
            return await GetExonym(searchName, cultureId);
        }

        async Task<string> GetExonym(string searchName, string cultureId)
        {
            foreach (string languageId in CultureLanguages[cultureId])
            {
                string exonym = GetExonymFromCache(searchName, languageId);

                if (exonym is null)
                {
                    exonym = await GetExonymFromApi(searchName, languageId);
                }

                if (!string.IsNullOrWhiteSpace(exonym))
                {
                    string normalisedExonym = nameCleaner.Normalise(exonym);
                    string normalisedSearchname = nameCleaner.Normalise(searchName);

                    if (normalisedExonym.Equals(normalisedSearchname, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return null;
                    }

                    return nameCleaner.Clean(exonym);
                }
            }

            return null;
        }

        async Task<string> GetExonymFromApi(string searchName, string languageId)
        {
            string response = await GetResponse(searchName);
            string exonym = DeserialiseSuccessResponse(response, searchName, languageId);

            SaveExonymInCache(searchName, languageId, exonym);

            return exonym;
        }

        async Task<string> GetResponse(string searchName)
        {
            string endpoint = BuildRequestUrl(searchName);

            if (responseCache.ContainsKey(searchName))
            {
                return responseCache[searchName];
            }

            HttpResponseMessage httpResponse = await httpClient.GetAsync(endpoint);
            string responseContent = await httpResponse.Content.ReadAsStringAsync();

            ValdiateHttpRespone(responseContent);

            responseCache.Add(searchName, responseContent);

            return responseContent;
        }

        string GetExonymFromCache(string searchName, string languageId)
        {
            string id = $"{nameCleaner.Normalise(searchName)}_{languageId}";
            CachedGeoNameExonym cachedExonym = nameCache.TryGet(id);

            if (cachedExonym is null)
            {
                return null;
            }
            
            return cachedExonym.Exonym ?? string.Empty;
        }

        void SaveExonymInCache(string searchName, string language, string exonym)
        {
            CachedGeoNameExonym cachedExonym = new CachedGeoNameExonym();
            cachedExonym.Id = $"{nameCleaner.Normalise(searchName)}_{language}";
            cachedExonym.PlaceName = searchName;
            cachedExonym.LanguageId = language;
            cachedExonym.Exonym = exonym?.Replace(",", ";");

            nameCache.Add(cachedExonym);

            int passedSeconds = (int)(DateTime.Now - lastCacheSave).TotalSeconds;
            if (passedSeconds >= 5)
            {
                lastCacheSave = DateTime.Now;
                nameCache.ApplyChanges();
            }
        }

        void ValdiateHttpRespone(string content)
        {
            if (content.Contains("status message"))
            {
                string errorMessage = DeserialiseErrorResponse(content);
                Console.WriteLine(errorMessage);

                throw new Exception(errorMessage);
            }
        }

        string DeserialiseErrorResponse(string content)
        {
            JObject o = JObject.Parse(content);
            JToken status = o["status"];
            JToken message = status["message"];
            
            return (string)message;
        }

        // TODO: The searchName parameter doesn't belong here
        string DeserialiseSuccessResponse(string content, string searchName, string language)
        {
            if (content.Contains("\"totalResultsCount\": 0"))
            {
                return null;
            }

            JObject o = JObject.Parse(content);
            IList<JToken> geoNames = o["geonames"].Children().ToList();
            string normalisedSearchName = nameCleaner.Normalise(searchName);
            string exonym = null;

            foreach (JToken geoName in geoNames)
            {
                if (!IsToponymValidCandidate(geoName, searchName))
                {
                    continue;
                }

                JToken alternateNames = geoName["alternateNames"];

                if (alternateNames is null)
                {
                    continue;
                }

                string toponymName = (string)geoName["toponymName"];

                foreach(JToken alternateName in alternateNames.Children())
                {
                    string lang = (string)alternateName["lang"];
                    string name = (string)alternateName["name"];

                    if (lang != language)
                    {
                        continue;
                    }

                    string normalisedName = nameCleaner.Normalise(name);
                    string normalisedToponymName = nameCleaner.Normalise(toponymName);

                    if (normalisedName.Equals(normalisedSearchName) ||
                        normalisedName.Equals(normalisedToponymName))
                    {
                        continue;
                    }
                    
                    return name;
                }
            }

            return exonym;
        }

        bool IsToponymValidCandidate(JToken geoName, string searchName)
        {
            string toponymName = (string)geoName["toponymName"];
            string name = (string)geoName["name"];
            string asciiName = (string)geoName["asciiName"];
            string continentCode = (string)geoName["continentCode"];

            if (!allowedContinents.Contains(continentCode))
            {
                return false;
            }

            if (DoNamesMatch(toponymName, searchName) ||
                DoNamesMatch(name, searchName) ||
                DoNamesMatch(asciiName, searchName))
            {
                return true;
            }

            JToken alternateNames = geoName["alternateNames"];

            if (alternateNames is null)
            {
                return false;
            }

            foreach (JToken alternateName in alternateNames.Children())
            {
                string alternateNameValue = (string)alternateName["name"];

                if (DoNamesMatch(alternateNameValue, searchName))
                {
                    return true;
                }
            }

            return false;
        }

        bool DoNamesMatch(string name1, string name2)
        {
            string normalisedName1 = nameCleaner.Normalise(name1);
            string normalisedName2 = nameCleaner.Normalise(name2);

            if (normalisedName1.Contains(normalisedName2))
            {
                return true;
            }

            return false;
        }

        string BuildRequestUrl(string searchName)
        {
            string encodedName = HttpUtility.UrlEncode(searchName);

            return
                $"{GeoNamesApiUrl}/searchJSON" +
                $"?username={usernames.GetRandomElement()}" +
                $"&q={encodedName}" +
                $"&name={encodedName}" +
                $"&name_equals={encodedName}" +
                "&featureClass=P" +
                "&featureClass=L" +
                //"&featureClass=A" +
                //"&featureClass=H" +
                //"&featureClass=T" +
                "&isNameRequired=true" +
                "&maxRows=15" +
                "&orderby=relevance" +
                "&style=FULL";
        }

        readonly IEnumerable<string> allowedContinents = new List<string> { "AF", "AS", "EU" };

        // TODO: Shouldn't be public. Shouldn't exist at all in the current form LOL
        public IDictionary<string, string[]> CultureLanguages => new Dictionary<string, string[]>
        {
            { "afar", new string[] { "aa" } },
            { "andalusian_arabic", new string[] { "xaa" } },
            { "anglonorse", new string[] { "ang" } },
            { "aragonese", new string[] { "an" } },
            { "arberian", new string[] { "aae" } },
            { "arpitan", new string[] { "frp" } },
            { "assamese", new string[] { "as" } },
            { "avar", new string[] { "oav", "av" } },
            { "basque", new string[] { "eu", "eus" } },
            { "bavarian", new string[] { "bar" } },
            { "bohemian", new string[] { "cs" } },
            { "bolghar", new string[] { "xbo" } },
            { "breton", new string[] { "xbm", "obt", "br" } },
            { "carantanian", new string[] { "sl" } },
            { "castillan", new string[] { "osp", "es" } },
            { "catalan", new string[] { "ca" } },
            { "cornish", new string[] { "cnx", "oco", "kw" } },
            { "croatian", new string[] { "hr" } },
            { "cuman", new string[] { "qwm" } },
            { "cumbric", new string[] { "xcb" } },
            { "dalmatian", new string[] { "dlm" } },
            { "danish", new string[] { "da" } },
            { "dutch", new string[] { "dum", "odt", "nl" } },
            { "english", new string[] { "enm" } },
            { "finnish", new string[] { "fi", "fit", "fkv" } },
            { "franconian", new string[] { "vmf" } },
            { "frankish", new string[] { "fro", "frm", "fr" } },
            { "frisian", new string[] { "ofs", "fy", "fry", "frs", "frr" } },
            { "galician", new string[] { "gl" } },
            { "german", new string[] { "goh", "gmh", "gml", "de" } },
            { "hausa", new string[] { "ha" } },
            { "hungarian", new string[] { "ohu", "hu" } },
            { "icelandic", new string[] { "is" } },
            { "irish", new string[] { "mga", "sga", "ga" } },
            { "italian", new string[] { "it" } },
            { "jurchen", new string[] { "juc" } },
            { "kanuri", new string[] { "kr" } },
            { "karelian", new string[] { "krl" } },
            { "kasogi", new string[] { "zsk" } },
            { "khalaj", new string[] { "kjf", "klj" } },
            { "khanty", new string[] { "kca" } },
            { "khazar", new string[] { "zkz" } },
            { "khitan", new string[] { "zkt" } },
            { "langobardisch", new string[] { "lng" } },
            { "lappish", new string[] { "smi" } },
            { "leonese", new string[] { "ast" } },
            { "lettigallish", new string[] { "lv" } },
            { "ligurian", new string[] { "lij" } },
            { "lithuanian", new string[] { "olt", "lt" } },
            { "livonian", new string[] { "liv" } },
            { "lombard", new string[] { "lmo" } },
            { "low_german", new string[] { "nds" } },
            { "low_saxon", new string[] { "nds" } },
            { "mari", new string[] { "chm" } },
            { "nahuatl", new string[] { "nci", "nah", "nhn", "azd", "azm", "azz", "cbs", "iss", "isc", "kaq" } },
            { "neapolitan", new string[] { "nap" } },
            { "norman", new string[] { "nrf" } },
            { "norse", new string[] { "non" } },
            { "norwegian", new string[] { "no", "nn", "nb" } },
            { "occitan", new string[] { "pro", "oc" } }, // i don't think it's mediaeval
            { "old_frankish", new string[] { "frk" } },
            { "old_saxon", new string[] { "osx" } },
            { "pahlavi", new string[] { "pal" } },
            { "pecheneg", new string[] { "xpc" } },
            { "pictish", new string[] { "xpi" } },
            { "polish", new string[] { "pl" } },
            { "portuguese", new string[] { "pt" } },
            { "prussian", new string[] { "prg" } },
            { "roman", new string[] { "la" } },
            { "romanian", new string[] { "ro", "rup", "ruo", "ruq" } },
            { "samoyed", new string[] { "syd" } },
            { "sardinian", new string[] { "sc", "sro", "src", "sdn", "sdc" } },
            { "scottish", new string[] { "gd", "ghc", "sco" } },
            { "sicilian", new string[] { "scn" } },
            { "slovieni", new string[] { "sk" } },
            { "somali", new string[] { "so" } },
            { "swabian", new string[] { "swg" } },
            { "swedish", new string[] { "sv" } },
            { "turkish", new string[] { "otk", "ota", "tr", "bgx", "crh", "kmz" } },
            { "turkmen", new string[] { "tk" } },
            { "udi", new string[] { "udi" } },
            { "ugricbaltic", new string[] { "et" } },
            { "umbrian", new string[] { "xum" } },
            { "venetian", new string[] { "vec" } },
            { "vepsian", new string[] { "vep" } },
            { "welsh", new string[] { "wlm", "owl", "cy" } },
            { "wolof", new string[] { "wo" } },
            //{ "armenian", new string[] { "axm", "hy" } }, // non-latin
            //{ "bashkir", new string[] { "ba" } }, // non-latin
            //{ "bengali", new string[] { "bn" } }, // non-latin
            //{ "bosnian", new string[] { "bs" } }, // non-latin
            //{ "bulgarian", new string[] { "cu", "bg" } }, // non-latin
            //{ "egyptian_arabic", new string[] { "egx", "arz", "avl" } }, // non-latin
            //{ "georgian", new string[] { "oge", ka" } }, // non-latin
            //{ "gothic", new string[] { "got" } }, // doesn't work
            //{ "greek", new string[] { "grc", "el" } }, // non-latin
            //{ "han", new string[] { "ltc", "zh" } }, // non-latin
            //{ "hijazi", new string[] { "acv" } }, // non-latin
            //{ "hindustani", new string[] { "hi" } }, // non-latin
            //{ "kannada", new string[] { "kn" } }, // non-latin
            //{ "kirghiz", new string[] { "ky" } }, // non-latin
            //{ "komi", new string[] { "kv", "koi", "kpv" } }, // non-latin
            //{ "kurdish", new string[] { "ku" } }, // non-latin
            //{ "maghreb_arabic", new string[] { "ar" } }, // non-latin
            //{ "marathi", new string[] { "mr" } }, // non-latin
            //{ "mongol", new string[] { "xng", "cmg", "mn" } }, // non-latin
            //{ "nepali", new string[] { "ne" } }, // non-latin
            //{ "oriya", new string[] { "or" } }, // non-latin
            //{ "panjabi", new string[] { "pa" } }, // non-latin
            //{ "persian", new string[] { "fa" } }, // non-latin
            //{ "russian", new string[] { "orv", ru" } }, // non-latin
            //{ "sindhi", new string[] { "sd" } }, // non-latin
            //{ "sinhala", new string[] { "si" } }, // non-latin
            //{ "tajik", new string[] { "tg" } }, // non-latin
            //{ "tamil", new string[] { "oty", ta" } }, // non-latin
            //{ "telugu", new string[] { "te" } }, // non-latin
            //{ "uyghur", new string[] { "oui", ug" } }, // non-latin
        };
    }
}
