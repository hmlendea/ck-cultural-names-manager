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
            return FindTitle(id, landedTitles);
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
        
            MergeDuplicates();
        }

        public void SaveTitles(string fileName)
        {
            LandedTitlesFile.WriteAllTitles(fileName, landedTitles.ToEntities());
        }

        private LandedTitle FindTitle(string id, IEnumerable<LandedTitle> landedTitlesChunk)
        {
            if (landedTitlesChunk.Any(x => x.Id == id))
            {
                return landedTitlesChunk.FirstOrDefault(x => x.Id == id);
            }

            foreach (LandedTitle landedTitle in landedTitlesChunk)
            {
                LandedTitle result = FindTitle(id, landedTitle.Children);

                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        private void MergeDuplicates()
        {
            landedTitles = MergeDuplicates(landedTitles).ToList();
        }

        private IEnumerable<LandedTitle> MergeDuplicates(IEnumerable<LandedTitle> landedTitlesChunk)
        {
            IEnumerable<LandedTitle> mergedDuplicates = landedTitlesChunk
                .GroupBy(o => o.Id)
                .Select(g => g.Skip(1)
                    .Aggregate(
                        g.First(),
                        (a, o) =>
                        {
                            a.Children = a.Children.Concat(o.Children).ToList();
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

            foreach (LandedTitle landedTitle in landedTitlesChunk)
            {
                landedTitle.Children = MergeDuplicates(landedTitle.Children).ToList();
            }

            return mergedDuplicates;
        }
    }
}
