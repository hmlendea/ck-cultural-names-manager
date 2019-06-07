using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using NuciExtensions;

using CK2LandedTitlesManager.BusinessLogic.Mapping;
using CK2LandedTitlesManager.Communication;
using CK2LandedTitlesManager.DataAccess.IO;
using CK2LandedTitlesManager.Models;

namespace CK2LandedTitlesManager.BusinessLogic
{
    public sealed class LandedTitleManager
    {
        List<LandedTitle> landedTitles;

        readonly IGeoNamesCommunicator geoNamesCommunicator;

        public LandedTitleManager()
        {
            landedTitles = new List<LandedTitle>();

            this.geoNamesCommunicator = new GeoNamesCommunicator();
        }

        public LandedTitle Get(string id)
        {
            return landedTitles.FirstOrDefault(x => x.Id == id);
        }

        public IEnumerable<LandedTitle> GetAll()
        {
            return landedTitles;
        }

        public void LoadTitles(string fileName)
        {
            List<LandedTitle> loadedTitles = LoadTitlesFromFile(fileName).ToList();

            landedTitles.AddRange(loadedTitles);
            landedTitles = MergeTitles(landedTitles).ToList();
        }

        public void RemoveDynamicNamesFromFile(string fileName)
        {
            IEnumerable<LandedTitle> landedTitlesToRemove = LoadTitlesFromFile(fileName);

            MergeTitles(landedTitlesToRemove);
            RemoveDynamicNames(landedTitlesToRemove);
        }

        public void RemoveDynamicNames() => RemoveDynamicNames(new List<string>());
        public void RemoveDynamicNames(IEnumerable<string> cultureIdExceptions)
        {
            foreach (LandedTitle title in landedTitles)
            {
                title.DynamicNames = title.DynamicNames
                    .Where(x => cultureIdExceptions.Contains(x.Key))
                    .ToDictionary(x => x.Key, x => x.Value);
            }
        }

        public void RemoveTitle(string titleId)
        {
            LandedTitle title = landedTitles.First(x => x.Id == titleId);

            if (!string.IsNullOrWhiteSpace(title.ParentId))
            {
                LandedTitle parentTitle = landedTitles.First(x => x.Id == title.ParentId);

                parentTitle.Children.Remove(title);
            }

            List<LandedTitle> children = landedTitles.Where(x => x.ParentId == title.Id).ToList();

            foreach (LandedTitle childTitle in children)
            {
                RemoveTitle(childTitle.Id);
            }
        }

        public void RemoveUnlocalisedTitles()
        {
            // TODO: Approach this issue better
            char[] titleLevels = new char[] { 'b', 'c', 'd', 'k', 'e' };

            foreach (char titleLevel in titleLevels)
            {
                landedTitles.RemoveAll(x =>
                    x.DynamicNames.Count == 0 &&
                    x.Id.StartsWith(titleLevel) &&
                    landedTitles.All(y => y.ParentId != x.Id));
            }
        }

        public bool CheckIntegrity(string fileName)
        {
            List<LandedTitle> masterTitles = LandedTitlesFile
                .ReadAllTitles(fileName)
                .ToDomainModels()
                .ToList();

            Dictionary<string, string> violations = new Dictionary<string, string>();

            foreach(LandedTitle title in landedTitles)
            {
                LandedTitle masterTitle = masterTitles.FirstOrDefault(x => x.Id == title.Id);

                if (landedTitles.Count(x => x.Id == title.Id) > 1)
                {
                    AddReasonToViolations(violations, title.Id, "Title is defined multiple times");
                    continue;
                }

                if (masterTitle == null)
                {
                    AddReasonToViolations(violations, title.Id, "Master file does not contain this title");
                    continue;
                }

                if (masterTitle.ParentId != title.ParentId)
                {
                    AddReasonToViolations(violations, title.Id, $"Master title has different parent ({title.ParentId} should be {masterTitle.ParentId})");
                    continue;
                }

                foreach (string cultureId in title.DynamicNames.Keys)
                {
                    if (masterTitle.DynamicNames.ContainsKey(cultureId) &&
                        masterTitle.DynamicNames[cultureId] == title.DynamicNames[cultureId])
                    {
                        AddReasonToViolations(violations, title.Id, $"Redundant dynamic name ({cultureId})");
                    }

                    if (title.DynamicNames[cultureId].Contains('?'))
                    {
                        AddReasonToViolations(violations, title.Id, $"Invalid character in title name ({cultureId})");
                    }
                }
            }

            return violations.Count == 0;
        }

        public void ApplySuggestions()
        {
            IEnumerable<CulturalGroupSuggestion> suggestions = GetCulturalGroupSuggestions();

            foreach (CulturalGroupSuggestion suggestion in suggestions)
            {
                LandedTitle landedTitle = landedTitles.First(x => x.Id == suggestion.TitleId);

                string name = $"{suggestion.SuggestedName}\" # Copied from {suggestion.SourceCultureId}. \"Cultural group match";

                landedTitle.DynamicNames.Add(suggestion.TargetCultureId, name);
            }
        }

        public void ApplySuggestions(string fileName)
        {
            List<LandedTitle> oldLandedTitles=  landedTitles.ToList();
            landedTitles = new List<LandedTitle>();
            landedTitles = LoadTitlesFromFile(fileName).ToList();
            landedTitles.AddRange(oldLandedTitles);
            landedTitles = MergeTitles(landedTitles).ToList();

            IEnumerable<CulturalGroupSuggestion> suggestions = GetCulturalGroupSuggestions();

            landedTitles = oldLandedTitles;

            foreach (CulturalGroupSuggestion suggestion in suggestions)
            {
                LandedTitle landedTitle = landedTitles.First(x => x.Id == suggestion.TitleId);

                string name = $"{suggestion.SuggestedName}\" # Copied from {suggestion.SourceCultureId}. \"Cultural group match";

                if (!landedTitle.DynamicNames.ContainsKey(suggestion.TargetCultureId))
                {
                    landedTitle.DynamicNames.Add(suggestion.TargetCultureId, name);
                }
            }
        }

        public IEnumerable<CulturalGroupSuggestion> GetCulturalGroupSuggestions()
        {
            List<CulturalGroupSuggestion> suggestions = new List<CulturalGroupSuggestion>();

            foreach (LandedTitle title in landedTitles)
            {
                if (title.DynamicNames.Count == 0)
                {
                    continue;
                }

                foreach (CultureGroup cultureGroup in cultureGroups)
                {
                    string foundTitleCultureId = cultureGroup.CultureIds.FirstOrDefault(x => title.DynamicNames.ContainsKey(x));

                    if (string.IsNullOrEmpty(foundTitleCultureId))
                    {
                        continue;
                    }

                    if (cultureGroup.MatchingMode == CulturalGroupMatchingMode.FirstOnlyPriority &&
                        foundTitleCultureId != cultureGroup.CultureIds.First())
                    {
                        continue;
                    }

                    foreach (string cultureId in cultureGroup.CultureIds)
                    {
                        if (title.DynamicNames.ContainsKey(cultureId))
                        {
                            continue;
                        }

                        if (cultureGroup.MatchingMode == CulturalGroupMatchingMode.AscendingPriority &&
                            cultureGroup.CultureIds.IndexOf(foundTitleCultureId) > cultureGroup.CultureIds.IndexOf(cultureId))
                        {
                            continue;
                        }

                        string name = title.DynamicNames[foundTitleCultureId];

                        CulturalGroupSuggestion suggestion = new CulturalGroupSuggestion
                        {
                            TitleId = title.Id,
                            SourceCultureId = foundTitleCultureId,
                            TargetCultureId = cultureId,
                            SuggestedName = name
                        };

                        suggestions.Add(suggestion);
                    }
                }
            }

            return suggestions;
        }

        // TODO: This parameter shouldn't exist
        public IEnumerable<GeoNamesSuggestion> GetGeoNamesSuggestion(bool autoAddThem = false)
        {
            List<GeoNamesSuggestion> suggestions = new List<GeoNamesSuggestion>();

            //foreach (LandedTitle title in landedTitles)
            for (int i = 4103; i < 12000; i++)
            {
                LandedTitle title = landedTitles[i];
                Console.WriteLine($"{i} - {title.Id}");

                foreach (string cultureId in CultureLanguages.Keys)
                {
                    if (title.DynamicNames.ContainsKey(cultureId))
                    {
                        continue;
                    }

                    string placeName = title.Id.Substring(2);
                    string exonym = geoNamesCommunicator.TryGatherExonym(placeName, CultureLanguages[cultureId]).Result; // TODO: Broken async

                    if (string.IsNullOrWhiteSpace(exonym) || placeName.Equals(exonym, StringComparison.InvariantCultureIgnoreCase))
                    {
                        continue;
                    }

                    GeoNamesSuggestion suggestion = new GeoNamesSuggestion
                    {
                        TitleId = title.Id,
                        CultureId = cultureId,
                        SuggestedName = exonym
                    };

                    suggestions.Add(suggestion);

                    if (autoAddThem)
                    {
                        title.DynamicNames.Add(cultureId, exonym);
                    }
                }
            }

            return suggestions;
        }

        public void CopyNamesFromCulture(string sourceCultureId, string targetCultureId)
        {
            foreach (LandedTitle title in landedTitles)
            {
                if (!title.DynamicNames.ContainsKey(sourceCultureId))
                {
                    continue;
                }

                if (title.DynamicNames.ContainsKey(targetCultureId))
                {
                    continue;
                }

                title.DynamicNames.Add(targetCultureId, title.DynamicNames[sourceCultureId]);
            }
        }

        public void CleanFile(string fileName)
        {
            List<LandedTitle> oldLandedTitles=  landedTitles.ToList();
            landedTitles = new List<LandedTitle>();
            landedTitles = LoadTitlesFromFile(fileName).ToList();

            List<string> oldLines = File
                .ReadAllLines(fileName)
                .Distinct()
                .ToList();

            SaveTitles(fileName);

            List<string> newLines = File.ReadAllLines(fileName).ToList();

            Dictionary<string, string> dict = new Dictionary<string, string>();

            for (int i = 0; i < oldLines.Count; i++)
            {
                string mcnLine = oldLines[i];

                if (string.IsNullOrWhiteSpace(mcnLine) || !mcnLine.Contains('#'))
                {
                    continue;
                }

                string key = Regex
                    .Match(mcnLine, "^([^#]*)[\t ]*#.*$")
                    .Groups[1]
                    .Value
                    .TrimEnd();

                if (string.IsNullOrWhiteSpace(key))
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(key) && !dict.ContainsKey(key))
                {
                    dict.Add(key, mcnLine);
                }
            }

            for (int i = 0; i < newLines.Count; i++)
            {
                string myLine = newLines[i];

                if (string.IsNullOrWhiteSpace(myLine))
                {
                    continue;
                }

                if (dict.ContainsKey(myLine))
                {
                    string myIndentation = Regex.Match(myLine, "( *)[^ ]*").Groups[1].Value;
                    myLine = myIndentation + dict[myLine].TrimStart();
                    newLines[i] = myLine;

                    continue;
                }
            }

            File.WriteAllLines(fileName, newLines);

            landedTitles = oldLandedTitles;
        }

        public void SaveTitles(string fileName)
        {
            LandedTitlesFile.WriteAllTitles(fileName, landedTitles.ToEntities());
            string content = File.ReadAllText(fileName);

            content = Regex.Replace(content, "\t", "    ");
            content = Regex.Replace(content, "= *(\r\n|\r|\n).*{", "={");
            content = Regex.Replace(content, "=", " = ");
            content = Regex.Replace(content, "\"(\r\n|\r|\n)( *[ekdcb]_)", "\"\n\n$2");

            File.WriteAllText(fileName, content);

            List<LandedTitle> oldLandedTitles=  landedTitles.ToList();
            landedTitles = new List<LandedTitle>();
            landedTitles = LoadTitlesFromFile(fileName).ToList();
        }

        // TODO: Better name
        // TODO: Proper return type
        public List<string> GetNamesOfTitlesWithAllCultures(List<string> cultureIds)
        {
            List<string> findings = new List<string>();

            int cultureColumnWidth = cultureIds.Max(x => x.Length);

            foreach (LandedTitle title in landedTitles)
            {
                if (cultureIds.All(title.DynamicNames.ContainsKey))
                {
                    string prefix = " ";

                    List<string> uniqueNames = title.DynamicNames
                        .Where(x => cultureIds.Contains(x.Key))
                        .Select(x => x.Value)
                        .Distinct()
                        .ToList();

                    if (uniqueNames.Count == 1)
                    {
                        prefix = "X";
                    }

                    foreach (string cultureId in cultureIds)
                    {
                        string finding = $"{prefix} {title.Id}\t{cultureId.PadRight(cultureColumnWidth + 1, ' ')}{title.DynamicNames[cultureId]}";
                        findings.Add(finding);
                    }
                }
            }

            return findings;
        }

        IEnumerable<LandedTitle> MergeTitles(IEnumerable<LandedTitle> landedTitles)
        {
            return landedTitles
                .GroupBy(o => o.Id)
                .Select(g => g.Skip(1)
                              .Aggregate(g.First(),
                                        (a, o) =>
                                        {
                                            a.DynamicNames = a.DynamicNames
                                                .Concat(o.DynamicNames)
                                                .GroupBy(e => e.Key)
                                                .ToDictionary(d => d.Key, d => d.First().Value);
                                            a.ReligiousValues = a.ReligiousValues
                                                .Concat(o.ReligiousValues)
                                                .GroupBy(e => e.Key)
                                                .ToDictionary(d => d.Key, d => d.First().Value);

                                            return a;
                                        }));
        }

        void RemoveDynamicNames(IEnumerable<LandedTitle> landedTitlesToRemove)
        {
            foreach (LandedTitle landedTitle in landedTitles)
            {
                LandedTitle landedTitleToRemove = landedTitlesToRemove.FirstOrDefault(x => x.Id == landedTitle.Id);

                if (landedTitleToRemove != null)
                {
                    List<string> cultureIds = landedTitle.DynamicNames.Keys.ToList();

                    foreach(string cultureId in cultureIds)
                    {
                        if (landedTitleToRemove.DynamicNames.ContainsKey(cultureId))
                        {
                            landedTitle.DynamicNames.Remove(cultureId);
                        }
                    }
                }
            }
        }

        void AddReasonToViolations(Dictionary<string, string> violations, string titleId, string reason)
        {
            Console.WriteLine(titleId + " " + reason);

            if (violations.ContainsKey(titleId))
            {
                violations[titleId] += $"; {reason}";
            }
            else
            {
                violations.Add(titleId, reason);
            }
        }

        IEnumerable<LandedTitle> LoadTitlesFromFile(string fileName)
        {
            IEnumerable<LandedTitle> titles = LandedTitlesFile
                .ReadAllTitles(fileName)
                .ToDomainModels();

            titles = MergeTitles(titles);

            return titles;
        }

        readonly List<CultureGroup> cultureGroups = new List<CultureGroup>
        {
            new CultureGroup(CulturalGroupMatchingMode.EqualPriority,
                "maghreb_arabic", "andalusian_arabic", "bedouin_arabic", "egyptian_arabic", "levantine_arabic", "hijazi", "yemeni"),

            new CultureGroup(CulturalGroupMatchingMode.FirstOnlyPriority,
                "italian", "dalmatian", "sardinian", "langobardisch", "laziale", "ligurian", "neapolitan", "sicilian", "tuscan", "umbrian", "venetian"),

            new CultureGroup(CulturalGroupMatchingMode.FirstOnlyPriority,
                "german", "bavarian", "franconian", "low_frankish", "low_german", "low_saxon", "swabian", "thuringian"),

            new CultureGroup(CulturalGroupMatchingMode.EqualPriority, "irish", "scottish", "welsh"),
            new CultureGroup(CulturalGroupMatchingMode.AscendingPriority, "scottish", "cumbric", "pictish"),
            new CultureGroup(CulturalGroupMatchingMode.AscendingPriority, "welsh", "breton", "cornish"),

            new CultureGroup(CulturalGroupMatchingMode.EqualPriority, "norse", "icelandic"),
            new CultureGroup(CulturalGroupMatchingMode.EqualPriority, "anglonorse", "norsegaelic"),

            new CultureGroup(CulturalGroupMatchingMode.EqualPriority, "finnish", "komi", "lappish", "livonian", "ugricbaltic"),
            new CultureGroup(CulturalGroupMatchingMode.EqualPriority, "lithuanian", "prussian", "lettigallish"),
            new CultureGroup(CulturalGroupMatchingMode.EqualPriority, "khanty", "mari", "vespian", "mordvin", "karelian", "samoyed"),

            new CultureGroup(CulturalGroupMatchingMode.FirstOnlyPriority, "greek", "crimean_gothic"),

            new CultureGroup(CulturalGroupMatchingMode.EqualPriority, "frankish", "norman", "arpitan"),

            new CultureGroup(CulturalGroupMatchingMode.AscendingPriority, "serbian", "croatian", "bosnian", "carantanian"),
            new CultureGroup(CulturalGroupMatchingMode.EqualPriority, "bohemian", "slovieni"),
            new CultureGroup(CulturalGroupMatchingMode.FirstOnlyPriority, "polish", "pommeranian"),

            new CultureGroup(CulturalGroupMatchingMode.EqualPriority, "hungarian", "szekely"),

            new CultureGroup(CulturalGroupMatchingMode.EqualPriority, "turkish", "turkmen", "oghuz", "pecheneg"),

            new CultureGroup(CulturalGroupMatchingMode.EqualPriority, "avar", "bolghar", "khazar"),

            new CultureGroup(CulturalGroupMatchingMode.EqualPriority, "afghan", "baloch", "qufs"),

            new CultureGroup(CulturalGroupMatchingMode.EqualPriority, "tuareg", "tagelmust", "sanhaja", "masmuda", "zanata"),

            new CultureGroup(CulturalGroupMatchingMode.EqualPriority, "persian", "tajik", "khwarezmi", "adhari", "khorasani"),
            new CultureGroup(CulturalGroupMatchingMode.EqualPriority, "sogdian", "daylamite", "khalaj")
        };

        readonly IDictionary<string, string> CultureLanguages = new Dictionary<string, string>
        {
            { "afar", "AA" },
            { "aragonese", "AN" },
            { "assamese", "AS" },
            { "avar", "AV" },
            { "basque", "EU" },
            { "bohemian", "CZ" },
            { "breton", "BR" },
            { "carantanian", "SL" },
            { "castillan", "ES" },
            { "catalan", "CA" },
            { "cornish", "KW" },
            { "croatian", "HR" },
            { "danish", "DA" },
            { "dutch", "DL" },
            { "finnish", "FI" },
            { "frankish", "FR" },
            { "frisian", "FY" },
            { "galiian", "GL" },
            { "german", "DE" },
            { "hausa", "HA" },
            { "hungarian", "HU" },
            { "icelandic", "IS" },
            { "irish", "GA" },
            { "italian", "IT" },
            { "kanuri", "KR" },
            { "lithuanian", "LT" },
            { "norwegian", "NO" },
            { "occitan", "OC" }, // i don't think it's mediaeval
            { "polish", "PL" },
            { "portuguese", "PT" },
            { "roman", "LA" },
            { "romanian", "RO" },
            { "sardinian", "SC" },
            { "scottish", "GD" },
            { "sindhi", "SD" },
            { "slovieni", "SK" },
            { "somali", "SO" },
            { "swedish", "SV" },
            { "turkish", "TR" },
            { "turkmen", "TK" },
            { "welsh", "CY" },
            { "wolof", "WO" },
            //{ "armenian", "HY" }, // non-latin
            //{ "bashkir", "BA" }, // non-latin
            //{ "bengali", "BN" }, // non-latin
            //{ "bosnian", "BS" }, // non-latin
            //{ "bulgarian", "BG" }, // non-latin
            //{ "georgian", "KA" }, // non-latin
            //{ "greek", "EL" }, // non-latin
            //{ "han", "ZH" }, // non-latin
            //{ "hindustani", "HI" }, // non-latin
            //{ "kannada", "KN" }, // non-latin
            //{ "kirghiz", "KY" }, // non-latin
            //{ "komi", "KV" }, // non-latin
            //{ "kurdish", "KU" }, // non-latin
            //{ "marathi", "MR" }, // non-latin
            //{ "mongolia", "MN" }, // non-latin
            //{ "nepali", "NE" }, // non-latin
            //{ "oriya", "OR" }, // non-latin
            //{ "panjabi", "PA" }, // non-latin
            //{ "persian", "FA" }, // non-latin
            //{ "russian", "RU" }, // non-latin
            //{ "sinhala", "SI" }, // non-latin
            //{ "tajik", "TG" }, // non-latin
            //{ "tamil", "TA" }, // non-latin
            //{ "telugu", "TE" }, // non-latin
            //{ "uyghur", "UG" }, // non-latin
        };
    }
}
