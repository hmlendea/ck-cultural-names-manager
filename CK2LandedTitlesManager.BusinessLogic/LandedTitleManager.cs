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
    }
}
