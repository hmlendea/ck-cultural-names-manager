using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using CK2LandedTitlesManager.DataAccess.DataObjects;
using CK2LandedTitlesManager.Models;

namespace CK2LandedTitlesManager.BusinessLogic.Mapping
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
                //Children = landedTitleEntity.Children.ToDomainModels().ToList(),
                Names = landedTitleEntity.Names.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            };

            return landedTitle;
        }

        internal static IEnumerable<LandedTitle> ToDomainModelsRecursively(this LandedTitleEntity landedTitleEntity)
        {
            List<LandedTitle> landedTitles = new List<LandedTitle>();

            landedTitles.Add(landedTitleEntity.ToDomainModel());

            foreach(LandedTitleEntity child in landedTitleEntity.Children)
            {
                landedTitles.AddRange(child.ToDomainModelsRecursively());
            }

            return landedTitles;
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
                //Children = landedTitle.Children.ToEntities().ToList(),
                Names = landedTitle.Names,
            };

            return landedTitleEntity;
        }

        internal static LandedTitleEntity ToEntity(this IEnumerable<LandedTitle> landedTitles)
        {
            LandedTitleEntity landedTitleEntity = landedTitles.FirstOrDefault(x => landedTitles.All(y => y.Id != x.ParentId)).ToEntity();

            AddChildrenToEntityRecursively(landedTitleEntity, landedTitles);

            return landedTitleEntity;
        }

        private static void AddChildrenToEntityRecursively(LandedTitleEntity landedTitleEntity, IEnumerable<LandedTitle> landedTitles)
        {
            foreach(LandedTitle landedTitle in landedTitles.Where(x => x.ParentId == landedTitleEntity.Id))
            {
                LandedTitleEntity child = landedTitle.ToEntity();
                landedTitleEntity.Children.Add(child);

                AddChildrenToEntityRecursively(child, landedTitles);
            }
        }

        /// <summary>
        /// Converts the entities into domain models.
        /// </summary>
        /// <returns>The domain models.</returns>
        /// <param name="landedTitleEntities">LandedTitle entities.</param>
        internal static IEnumerable<LandedTitle> ToDomainModels(this IEnumerable<LandedTitleEntity> landedTitleEntities)
        {
            List<LandedTitle> landedTitles = new List<LandedTitle>();

            foreach(LandedTitleEntity landedTitleEntity in landedTitleEntities)
            {
                IEnumerable<LandedTitle> landedTitlesChildren = landedTitleEntity.ToDomainModelsRecursively();

                landedTitles.AddRange(landedTitlesChildren);
            }
            
            return landedTitles;
        }

        /// <summary>
        /// Converts the domain models into entities.
        /// </summary>
        /// <returns>The entities.</returns>
        /// <param name="landedTitles">LandedTitles.</param>
        internal static IEnumerable<LandedTitleEntity> ToEntities(this IEnumerable<LandedTitle> landedTitles)
        {
            IEnumerable<LandedTitle> roots = landedTitles.Where(x => landedTitles.All(y => y.Id != x.ParentId));
            List<LandedTitleEntity> landedTitleEntities = new List<LandedTitleEntity>();

            foreach(LandedTitle root in roots)
            {
                LandedTitleEntity landedTitleEntity = root.ToEntity();

                AddChildrenToEntityRecursively(landedTitleEntity, landedTitles);

                landedTitleEntities.Add(landedTitleEntity);
            }

            return landedTitleEntities;
        }

        private static Color GetColorFromIntArray(int[] rgb)
        {
            int r = Math.Min(Math.Max(0, rgb[0]), 255);
            int g = Math.Min(Math.Max(0, rgb[1]), 255);
            int b = Math.Min(Math.Max(0, rgb[2]), 255);

            return Color.FromArgb(255, r, g, b);
        }
    }
}
