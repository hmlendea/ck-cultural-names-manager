using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using NuciExtensions;

using CKCulturalNamesManager.DataAccess.DataObjects;
using CKCulturalNamesManager.DataAccess.IO;
using CKCulturalNamesManager.BusinessLogic.Mapping;
using CKCulturalNamesManager.BusinessLogic.Models;
using CKCulturalNamesManager.Models;

namespace CKCulturalNamesManager.BusinessLogic
{
    public sealed class LandedTitleManager
    {
        List<LandedTitle> landedTitles;

        readonly INameValidator nameValidator;

        public LandedTitleManager()
        {
            landedTitles = new List<LandedTitle>();
            nameValidator = new NameValidator();
            
            EncodingProvider encodingProvider = CodePagesEncodingProvider.Instance;
            Encoding.RegisterProvider(encodingProvider);
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

        public void RemoveNamesFromFile(string fileName)
        {
            IEnumerable<LandedTitle> landedTitlesToRemove = LoadTitlesFromFile(fileName);
            RemoveNames(landedTitlesToRemove);
        }

        public void RemoveNames()
        {
            foreach (LandedTitle title in landedTitles)
            {
                title.Names.Clear();
            }
        }

        public bool CheckIntegrity(string fileName)
        {
            IEnumerable<LandedTitle> masterTitles = GetTitlesFromFile(fileName);

            Dictionary<string, string> violations = new Dictionary<string, string>();

            foreach (LandedTitle title in landedTitles)
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

                foreach (string cultureId in title.Names.Keys)
                {
                    if (!nameValidator.IsNameValid(title.Names[cultureId]))
                    {
                        AddReasonToViolations(violations, title.Id, $"Invalid name for {cultureId}");
                    }
                }
            }

            return violations.Count == 0;
        }

        public IDictionary<string, int> GetNamesCount()
        {
            IDictionary<string, int> namesCount = new Dictionary<string, int>();

            foreach (LandedTitle title in landedTitles)
            {
                foreach (string cultureId in title.Names.Keys)
                {
                    if (!namesCount.ContainsKey(cultureId))
                    {
                        namesCount.Add(cultureId, 0);
                    }

                    namesCount[cultureId] += 1;
                }
            }

            return namesCount
                .OrderBy(x => x.Value)
                .Reverse()
                .ToDictionary(x => x.Key, x => x.Value);
        }

        public IEnumerable<OverwrittenName> GetOverwrittenNames(string fileName)
        {
            IEnumerable<LandedTitle> masterTitles = GetTitlesFromFile(fileName);
            IList<OverwrittenName> overwrittenNames = new List<OverwrittenName>();

            foreach (LandedTitle title in landedTitles)
            {
                LandedTitle masterTitle = masterTitles.FirstOrDefault(x => x.Id == title.Id);

                if (masterTitle == null)
                {
                    continue;
                }

                foreach (string cultureId in title.Names.Keys)
                {
                    if (!masterTitle.Names.ContainsKey(cultureId) ||
                        masterTitle.Names[cultureId] == title.Names[cultureId])
                    {
                        continue;
                    }

                    OverwrittenName overwrittenName = new OverwrittenName();
                    overwrittenName.TitleId = title.Id;
                    overwrittenName.CultureId = cultureId;
                    overwrittenName.OriginalName = masterTitle.Names[cultureId];
                    overwrittenName.FinalName = title.Names[cultureId];

                    overwrittenNames.Add(overwrittenName);
                }
            }
            
            return overwrittenNames;
        }

        public void SaveTitlesCK2(string fileName)
        {
            LandedTitlesFile<CK2LandedTitleDefinition>.WriteAllTitles(fileName, landedTitles.ToEntities());
            string content = ReadWindows1252File(fileName);

            content = Regex.Replace(content, "\t", "    ");
            content = Regex.Replace(content, "= *(\r\n|\r|\n).*{", "={");
            content = Regex.Replace(content, "=", " = ");
            content = Regex.Replace(content, "\"(\r\n|\r|\n)( *[ekdcb]_)", "\"\n\n$2");

            WriteWindows1252File(fileName, content);

            List<LandedTitle> oldLandedTitles=  landedTitles.ToList();
            landedTitles = new List<LandedTitle>();
            landedTitles = LoadTitlesFromFile(fileName).ToList();
        }

        public void SaveTitlesCK3(string fileName)
        {
            LandedTitlesFile<CK3LandedTitleDefinition>.WriteAllTitles(fileName, landedTitles.ToEntities());
        }

        // TODO: Better name
        // TODO: Proper return type
        public List<string> GetNamesOfCultures(List<string> cultureIds)
        {
            List<string> findings = new List<string>();

            int cultureColumnWidth = cultureIds.Max(x => x.Length);

            foreach (LandedTitle title in landedTitles)
            {
                if (cultureIds.All(title.Names.ContainsKey))
                {
                    string prefix = " ";

                    List<string> uniqueNames = title.Names
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
                        string finding = $"{prefix} {title.Id}\t{cultureId.PadRight(cultureColumnWidth + 1, ' ')}{title.Names[cultureId]}";
                        findings.Add(finding);
                    }
                }
            }

            return findings;
        }

        string ReadWindows1252File(string filePath)
        {
            Encoding encoding = Encoding.GetEncoding("windows-1252");
            
            return File.ReadAllText(filePath, encoding);
        }

        void WriteWindows1252File(string filePath, string content)
        {
            Encoding encoding = Encoding.GetEncoding("windows-1252");
            byte[] contentBytes = encoding.GetBytes(content.ToCharArray());
            
            File.WriteAllBytes(filePath, contentBytes);
        }

        IEnumerable<LandedTitle> MergeTitles(IEnumerable<LandedTitle> landedTitles)
        {
            return landedTitles
                .GroupBy(o => o.Id)
                .Select(g => g.Skip(1)
                              .Aggregate(g.First(),
                                        (a, o) =>
                                        {
                                            a.Names = a.Names
                                                .Concat(o.Names)
                                                .GroupBy(e => e.Key)
                                                .ToDictionary(d => d.Key, d => d.First().Value);

                                            return a;
                                        }));
        }

        void RemoveNames(IEnumerable<LandedTitle> landedTitlesToRemove)
        {
            foreach (LandedTitle landedTitle in landedTitles)
            {
                LandedTitle landedTitleToRemove = landedTitlesToRemove.FirstOrDefault(x => x.Id == landedTitle.Id);

                if (landedTitleToRemove == null)
                {
                    continue;
                }

                foreach (string cultureId in landedTitle.Names.Keys)
                {
                    if (landedTitleToRemove.Names.ContainsKey(cultureId))
                    {
                        landedTitle.Names.Remove(cultureId);
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
            IEnumerable<LandedTitle> titles = GetTitlesFromFile(fileName);
            titles = MergeTitles(titles);

            return titles;
        }

        IEnumerable<LandedTitle> GetTitlesFromFile(string fileName)
        {
            string content = File.ReadAllText(fileName);

            IEnumerable<LandedTitleEntity> masterTitleEntities;

            if (content.Contains("cultural_names"))
            {
                masterTitleEntities = LandedTitlesFile<CK3LandedTitleDefinition>.ReadAllTitles(fileName);
            }
            else
            {
                masterTitleEntities = LandedTitlesFile<CK2LandedTitleDefinition>.ReadAllTitles(fileName);
            }

            return masterTitleEntities.ToDomainModels();
        }
    }
}
