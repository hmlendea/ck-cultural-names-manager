using System.Collections.Generic;
using System.Linq;

using CK2LandedTitlesManager.DataAccess.DataObjects;
using CK2LandedTitlesManager.Models;

namespace CK2LandedTitlesManager.Mapping
{
    /// <summary>
    /// LandedTitle mapping extensions for converting between entities and domain models.
    /// </summary>
    static class LandedTitleMappingExtensions
    {
        /// <summary>
        /// Converts the entity into a domain model.
        /// </summary>
        /// <returns>The domain model.</returns>
        /// <param name="landedTitleEntity">LandedTitle entity.</param>
        internal static LandedTitle ToDomainModel(this LandedTitleEntity landedTitleEntity)
        {
            LandedTitle landedTitle = new LandedTitle
            {
                Id = landedTitleEntity.Id,
                ParentId = landedTitleEntity.ParentId,
                Children = landedTitleEntity.Children.ToDomainModels().ToList(),
                DynamicNames = landedTitleEntity.DynamicNames.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            };

            return landedTitle;
        }

        /// <summary>
        /// Converts the domain model into an entity.
        /// </summary>
        /// <returns>The entity.</returns>
        /// <param name="landedTitle">LandedTitle.</param>
        internal static LandedTitleEntity ToEntity(this LandedTitle landedTitle)
        {
            LandedTitleEntity landedTitleEntity = new LandedTitleEntity
            {
                Id = landedTitle.Id,
                ParentId = landedTitle.ParentId,
                Children = landedTitle.Children.ToEntities().ToList(),
                DynamicNames = landedTitle.DynamicNames
            };

            return landedTitleEntity;
        }

        /// <summary>
        /// Converts the entities into domain models.
        /// </summary>
        /// <returns>The domain models.</returns>
        /// <param name="landedTitleEntities">LandedTitle entities.</param>
        internal static IEnumerable<LandedTitle> ToDomainModels(this IEnumerable<LandedTitleEntity> landedTitleEntities)
        {
            IEnumerable<LandedTitle> landedTitles = landedTitleEntities.Select(landedTitleEntity => landedTitleEntity.ToDomainModel());

            return landedTitles;
        }

        /// <summary>
        /// Converts the domain models into entities.
        /// </summary>
        /// <returns>The entities.</returns>
        /// <param name="landedTitles">LandedTitles.</param>
        internal static IEnumerable<LandedTitleEntity> ToEntities(this IEnumerable<LandedTitle> landedTitles)
        {
            IEnumerable<LandedTitleEntity> landedTitleEntities = landedTitles.Select(landedTitle => landedTitle.ToEntity());

            return landedTitleEntities;
        }
    }
}
