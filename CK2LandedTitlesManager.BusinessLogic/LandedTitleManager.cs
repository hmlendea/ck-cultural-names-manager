﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using CK2LandedTitlesManager.BusinessLogic.Mapping;
using CK2LandedTitlesManager.DataAccess.IO;
using CK2LandedTitlesManager.Infrastructure.Extensions;
using CK2LandedTitlesManager.Models;

namespace CK2LandedTitlesManager.BusinessLogic
{
    public sealed class LandedTitleManager
    {
        List<LandedTitle> landedTitles;
        List<CultureGroup> cultureGroups;

        public LandedTitleManager()
        {
            landedTitles = new List<LandedTitle>();
            cultureGroups = BuildCultureGroups();
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
            landedTitles = MergeTitles(loadedTitles).ToList();
        }

        public void RemoveDynamicNamesFromFile(string fileName)
        {
            IEnumerable<LandedTitle> landedTitlesToRemove = LoadTitlesFromFile(fileName);

            MergeTitles(landedTitlesToRemove);
            RemoveDynamicNames(landedTitlesToRemove);
        }

        public void RemoveAllDynamicNames()
        {
            landedTitles.ForEach(title => title.DynamicNames.Clear());
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
                    foreach (string cultureId in cultureIds)
                    {
                        string finding = $"{title.Id}\t{cultureId.PadRight(cultureColumnWidth + 1, ' ')}{title.DynamicNames[cultureId]}";
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

        List<CultureGroup> BuildCultureGroups()
        {
            List<CultureGroup> cultureGroups = new List<CultureGroup>();

            cultureGroups.Add(new CultureGroup(
                CulturalGroupMatchingMode.FirstOnlyPriority,
                "italian", "dalmatian", "sardinian", "langobardisch", "laziale", "ligurian", "neapolitan", "sicilian", "tuscan", "umbrian", "venetian"));

            cultureGroups.Add(new CultureGroup(CulturalGroupMatchingMode.EqualPriority, "irish", "scottish", "welsh"));
            cultureGroups.Add(new CultureGroup(CulturalGroupMatchingMode.AscendingPriority, "scottish", "cumbric", "pictish"));
            cultureGroups.Add(new CultureGroup(CulturalGroupMatchingMode.AscendingPriority, "welsh", "breton", "cornish"));

            cultureGroups.Add(new CultureGroup(
                CulturalGroupMatchingMode.FirstOnlyPriority,
                "german", "bavarian", "franconian", "low_frankish", "low_german", "low_saxon", "swabian", "thuringian"));

            cultureGroups.Add(new CultureGroup(CulturalGroupMatchingMode.EqualPriority, "turkish", "oghuz", "pecheneg", "turkmen"));

            cultureGroups.Add(new CultureGroup(CulturalGroupMatchingMode.EqualPriority, "avar", "bolghar", "khazar"));

            cultureGroups.Add(new CultureGroup(CulturalGroupMatchingMode.AscendingPriority, "serbian", "croatian", "bosnian"));

            cultureGroups.Add(new CultureGroup(CulturalGroupMatchingMode.EqualPriority, "bohemian", "moravian"));

            cultureGroups.Add(new CultureGroup(CulturalGroupMatchingMode.FirstOnlyPriority, "polish", "pommeranian"));

            cultureGroups.Add(new CultureGroup(CulturalGroupMatchingMode.EqualPriority, "hungarian", "szekely"));

            cultureGroups.Add(new CultureGroup(CulturalGroupMatchingMode.EqualPriority, "frankish", "norman"));

            cultureGroups.Add(new CultureGroup(CulturalGroupMatchingMode.EqualPriority, "sogdian", "khalaj", "khwarezmi"));

            cultureGroups.Add(new CultureGroup(CulturalGroupMatchingMode.EqualPriority, "norse", "icelandic"));

            cultureGroups.Add(new CultureGroup(CulturalGroupMatchingMode.EqualPriority, "anglonorse", "norsegaelic"));

            cultureGroups.Add(new CultureGroup(CulturalGroupMatchingMode.FirstOnlyPriority, "greek", "crimean_gothic"));

            cultureGroups.Add(new CultureGroup(CulturalGroupMatchingMode.FirstOnlyPriority, "finnish", "komi", "lappish", "livonian", "ugricbaltic"));

            cultureGroups.Add(new CultureGroup(
                CulturalGroupMatchingMode.EqualPriority,
                "maghreb_arabic", "andalusian_arabic", "bedouin_arabic", "egyptian_arabic", "levantine_arabic", "hijazi", "yemeni"));

            return cultureGroups;
        }
    }
}
