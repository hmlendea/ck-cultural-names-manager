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
            List<List<string>> alikeCultureLists = new List<List<string>>();

            alikeCultureLists.Add(new List<string> {
                "italian", "sardinian", "sicilian", "umbrian", "laziale", "neapolitan",
                "tuscan", "ligurian", "langobardisch", "venetian", "dalmatian" });
            alikeCultureLists.Add(new List<string> {
                "german", "thuringian", "swabian", "bavarian", "low_saxon", "franconian",
                "low_german", "low_frankish" });
            alikeCultureLists.Add(new List<string> {
                "turkish", "turkmen", "oghuz", "pecheneg" });
            alikeCultureLists.Add(new List<string> {
                "avar", "bolghar", "khazar" });
            alikeCultureLists.Add(new List<string> {
                "croatian", "serbian", "bosnian", "karantanci" });
            alikeCultureLists.Add(new List<string> {
                "bohemian", "moravian" });
            alikeCultureLists.Add(new List<string> {
                "maghreb_arabic", "levantine_arabic", "egyptian_arabic",
                "andalusian_arabic", "bedouin_arabic" });

            List<string> sasa = new List<string>();

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

                    foreach (string cultureId in alikeCultures)
                    {
                        if (!masterTitle.DynamicNames.ContainsKey(cultureId) &&
                            !title.DynamicNames.ContainsKey(cultureId))
                        {
                            AddReasonToViolations(
                                violations,
                                title.Id,
                                $"No localisation found for {cultureId}. Consider copying one from {foundTitleCultureId} as fallback.");
                            
                            if (foundTitleCultureId == "bohemian" ||
                                foundTitleCultureId == "moravian" ||
                                foundTitleCultureId == "german" ||
                                foundTitleCultureId == "turkish" ||
                                foundTitleCultureId == "italian" ||
                                foundTitleCultureId == "croatian" ||
                                foundTitleCultureId == "serbian" ||
                                foundTitleCultureId == "avar" ||
                                foundTitleCultureId == "bolghar" ||
                                foundTitleCultureId == "khazar" ||
                                foundTitleCultureId == "maghreb_arabic")
                            {
                                if (title.DynamicNames.ContainsKey(foundTitleCultureId))
                                {
                                    sasa.Add($"{title.Id} {cultureId} {foundTitleCultureId} {title.DynamicNames[foundTitleCultureId].Replace(" ", "_")}");
                                }
                                else if (masterTitle.DynamicNames.ContainsKey(foundTitleCultureId))
                                {
                                    sasa.Add($"{title.Id} {cultureId} {foundTitleCultureId} {masterTitle.DynamicNames[foundTitleCultureId].Replace(" ", "_")}");
                                }
                            }
                        }
                    }

                    System.IO.File.WriteAllLines("sasa.out.txt", sasa);
                }
            }

            return violations.Count == 0;
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
