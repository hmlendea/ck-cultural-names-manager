using System;
using System.Collections.Generic;
using System.Linq;

using CK2LandedTitlesManager.BusinessLogic.Mapping;
using CK2LandedTitlesManager.DataAccess.IO;
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

             MergeTitles(landedTitles);
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

        public void SaveTitles(string fileName)
        {
            LandedTitlesFile.WriteAllTitles(fileName, landedTitles.ToEntities());
        }

        void MergeTitles(IEnumerable<LandedTitle> landedTitles)
        {
            landedTitles = landedTitles
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
                                        })).ToList();
        }

        void RemoveDynamicNames(IEnumerable<LandedTitle> titlesToRemove)
        {
            landedTitles = landedTitles
                .Concat(titlesToRemove)
                .GroupBy(o => o.Id)
                .Select(g => g.Skip(1)
                              .Aggregate(g.First(),
                                         (a, o) =>
                                         {
                                             a.DynamicNames = a.DynamicNames
                                                 .Where(kvp => !o.DynamicNames.Keys.Contains(kvp.Key))
                                                 .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                                             return a;
                                         })).ToList();
        }
    }
}
