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
            landedTitles = LandedTitlesFile
                .ReadAllTitles(fileName)
                .ToDomainModels()
                .ToList();

            landedTitles = MergeTitles(landedTitles).ToList();
        }

        public void RemoveDynamicNamesFromFile(string fileName)
        {
            List<LandedTitle> landedTitlesToRemove = LandedTitlesFile
                .ReadAllTitles(fileName)
                .ToDomainModels()
                .ToList();

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
            List<LandedTitle> masterTitles = LandedTitlesFile
                .ReadAllTitles(fileName)
                .ToDomainModels()
                .ToList();

            List<CulturalGroupSuggestion> suggestions = new List<CulturalGroupSuggestion>();
            List<List<string>> alikeCultureLists = new List<List<string>>();

            alikeCultureLists.Add(new List<string> { "italian", "dalmatian", "sardinian", "langobardisch", "laziale", "ligurian",
                                                     "neapolitan", "sicilian", "tuscan", "umbrian", "venetian" });

            alikeCultureLists.Add(new List<string> { "irish", "scottish", "welsh" });
            alikeCultureLists.Add(new List<string> { "scottish", "irish", "welsh" });
            alikeCultureLists.Add(new List<string> { "welsh", "irish", "scottish" });
            alikeCultureLists.Add(new List<string> { "welsh", "breton", "cornish" });
            alikeCultureLists.Add(new List<string> { "scottish", "cumbric", "pictish" });

            alikeCultureLists.Add(new List<string> { "german", "bavarian", "franconian", "low_frankish", "low_german", "low_saxon", "swabian", "thuringian" });

            alikeCultureLists.Add(new List<string> { "turkish", "oghuz", "pecheneg", "turkmen" });

            alikeCultureLists.Add(new List<string> { "avar", "bolghar", "khazar" });
            alikeCultureLists.Add(new List<string> { "bolghar", "avar", "khazar" });
            alikeCultureLists.Add(new List<string> { "khazar", "avar", "bolghar" });

            alikeCultureLists.Add(new List<string> { "serbian", "bosnian", "croatian" }); //, "carantanian" });

            alikeCultureLists.Add(new List<string> { "bohemian", "moravian" });
            alikeCultureLists.Add(new List<string> { "moravian", "bohemian" });
            
            alikeCultureLists.Add(new List<string> { "polish", "pommeranian" });
            
            alikeCultureLists.Add(new List<string> { "hungarian", "szekely" });
            alikeCultureLists.Add(new List<string> { "szekely", "hungarian" });

            alikeCultureLists.Add(new List<string> { "frankish", "norman" });
            alikeCultureLists.Add(new List<string> { "norman", "frankish" });
            
            alikeCultureLists.Add(new List<string> { "sogdian", "khalaj", "khwarezmi" });
            alikeCultureLists.Add(new List<string> { "khwarezmi", "khalaj", "sogdian" });
            alikeCultureLists.Add(new List<string> { "khalaj", "khwarezmi", "sogdian" });

            alikeCultureLists.Add(new List<string> { "norse", "icelandic", });
            alikeCultureLists.Add(new List<string> { "anglonorse", "norsegaelic" });
            alikeCultureLists.Add(new List<string> { "norsegaelic", "anglonorse" });

            alikeCultureLists.Add(new List<string> { "greek", "crimean_gothic" });

            alikeCultureLists.Add(new List<string> { "finnish", "komi", "lappish", "livonian", "ugricbaltic" });

            alikeCultureLists.Add(new List<string> { "maghreb_arabic", "andalusian_arabic", "bedouin_arabic", "egyptian_arabic", "levantine_arabic" });
            alikeCultureLists.Add(new List<string> { "maghreb_arabic", "hijazi", "yemeni" });
            alikeCultureLists.Add(new List<string> { "hijazi", "maghreb_arabic" });
            alikeCultureLists.Add(new List<string> { "yemeni", "maghreb_arabic" });
            alikeCultureLists.Add(new List<string> { "hijazi", "yemeni" });
            alikeCultureLists.Add(new List<string> { "yemeni", "hijazi" });
            
            bool safeAlikeCultures = true;

            foreach(LandedTitle title in landedTitles)
            {
                LandedTitle masterTitle = masterTitles.FirstOrDefault(x => x.Id == title.Id);

                foreach (List<string> alikeCultures in alikeCultureLists)
                {
                    string foundTitleCultureId = alikeCultures.FirstOrDefault(x => title.DynamicNames.ContainsKey(x));
                    string foundMasterTitleCultureId = alikeCultures.FirstOrDefault(x => masterTitle.DynamicNames.ContainsKey(x));
                    
                    if (string.IsNullOrEmpty(foundTitleCultureId))
                    {
                        if (string.IsNullOrEmpty(foundMasterTitleCultureId))
                        {
                            continue;
                        }

                        foundTitleCultureId = foundMasterTitleCultureId;
                    }

                    if (safeAlikeCultures && foundTitleCultureId != alikeCultures.First())
                    {
                        continue;
                    }

                    foreach (string cultureId in alikeCultures)
                    {
                        if (masterTitle.DynamicNames.ContainsKey(cultureId) ||
                            title.DynamicNames.ContainsKey(cultureId))
                        {
                            continue;
                        }

                        string name = string.Empty;

                        if (title.DynamicNames.ContainsKey(foundTitleCultureId))
                        {
                            name = title.DynamicNames[foundTitleCultureId];
                        }
                        else
                        {
                            name = masterTitle.DynamicNames[foundTitleCultureId];
                        }

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
    }
}
