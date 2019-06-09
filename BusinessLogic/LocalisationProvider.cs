using System.Collections.Generic;

using NuciDAL.Repositories;
using NuciExtensions;

using CK2LandedTitlesManager.BusinessLogic.Models;

namespace CK2LandedTitlesManager.BusinessLogic
{
    public sealed class LocalisationProvider : ILocalisationProvider
    {
        readonly IDictionary<string, string> localisationCache;
        readonly IRepository<TitleLocalisation> localisationRepository;

        public LocalisationProvider(IRepository<TitleLocalisation> localisationRepository)
        {
            localisationCache = new Dictionary<string, string>();

            this.localisationRepository = localisationRepository;
        }

        public string GetLocalisation(string titleId)
        {
            TitleLocalisation localisation = localisationRepository.TryGet(titleId);

            if (localisation is null)
            {
                return GenerateLocalisationFromTitleId(titleId);
            }

            return localisation.Value;
        }

        string GenerateLocalisationFromTitleId(string titleId)
        {
            if (localisationCache.ContainsKey(titleId))
            {
                return localisationCache[titleId];
            }

            string localisation = titleId;
            
            if (titleId[1] == '_')
            {
                localisation = titleId.Substring(2);
            }

            localisation = localisation
                .Replace("_", " ")
                .ToTitleCase();

            localisationCache.Add(titleId, localisation);

            return localisation;
        }
    }
}
