using System;
using System.Collections.Generic;
using System.Linq;

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
            landedTitles = LoadTitlesFromFile(fileName).ToList();
            landedTitles = MergeTitles(landedTitles).ToList();
        }

        public void RemoveDynamicNamesFromFile(string fileName)
        {
            IEnumerable<LandedTitle> landedTitlesToRemove = LoadTitlesFromFile(fileName);

            MergeTitles(landedTitlesToRemove);
            RemoveDynamicNames(landedTitlesToRemove);
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

        public IEnumerable<CulturalGroupSuggestion> GetCulturalGroupSuggestions(string fileName)
        {
            IEnumerable<LandedTitle> masterTitles = LoadTitlesFromFile(fileName);
            IEnumerable<LandedTitle> mergedTitles = MergeTitles(masterTitles);

            List<CulturalGroupSuggestion> suggestions = new List<CulturalGroupSuggestion>();

            foreach (LandedTitle title in mergedTitles)
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

                    foreach (string cultureId in cultureGroup.CultureIds.Where(x => !title.DynamicNames.ContainsKey(x)))
                    {
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

        public void SaveTitles(string fileName)
        {
            LandedTitlesFile.WriteAllTitles(fileName, landedTitles.ToEntities());
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
                "andalusian_arabic", "bedouin_arabic", "egyptian_arabic", "levantine_arabic", "maghreb_arabic", "hijazi", "yemeni"));

            return cultureGroups;
        }
    }
}
