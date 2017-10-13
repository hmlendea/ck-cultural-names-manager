using System;
using System.Collections.Generic;
using System.Linq;

using CK2LandedTitlesManager.BusinessLogic.Mapping;
using CK2LandedTitlesManager.DataAccess.Repositories;
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
            LandedTitleRepository repository = new LandedTitleRepository(fileName);
            landedTitles = repository.GetAll().ToDomainModels().ToList();
        
            RemoveDuplicatedTitles(landedTitles);
        }

        public void SaveTitles(string fileName)
        {
            throw new NotImplementedException();
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

        private void RemoveDuplicatedTitles(IEnumerable<LandedTitle> landedTitlesChunk)
        {
            landedTitlesChunk = landedTitlesChunk
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

                            return a;
                        }));

            foreach (LandedTitle landedTitle in landedTitlesChunk)
            {
                RemoveDuplicatedTitles(landedTitle.Children);
            }
        }
    }
}
