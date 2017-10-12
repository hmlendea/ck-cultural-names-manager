using System.Collections.Generic;
using System.Drawing;
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
                DynamicNames = landedTitleEntity.DynamicNames.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                PrimaryColour = Color.FromArgb(255, landedTitleEntity.PrimaryColour[0], landedTitleEntity.PrimaryColour[1], landedTitleEntity.PrimaryColour[2]),
                SecondaryColour = Color.FromArgb(255, landedTitleEntity.SecondaryColour[0], landedTitleEntity.SecondaryColour[1], landedTitleEntity.SecondaryColour[2]),
                CultureId = landedTitleEntity.CultureId,
                GraphicalCulture = landedTitleEntity.GraphicalCulture,
                MercenaryType = landedTitleEntity.MercenaryType,
                ReligionId = landedTitleEntity.ReligionId,
                TitleFormOfAddress = landedTitleEntity.TitleFormOfAddress,
                TitleLocalisationId = landedTitleEntity.TitleLocalisationId,
                TitleLocalisationFemaleId = landedTitleEntity.TitleLocalisationFemaleId,
                HasPurpleBornHeirs = landedTitleEntity.HasPurpleBornHeirs,
                HasTopDeJureCapital = landedTitleEntity.HasTopDeJureCapital,
                IsCaliphate = landedTitleEntity.IsCaliphate,
                IsHolyOrder = landedTitleEntity.IsHolyOrder,
                IsLandless = landedTitleEntity.IsLandless,
                IsMercenaryGroup = landedTitleEntity.IsMercenaryGroup,
                IsPrimaryTitle = landedTitleEntity.IsPrimaryTitle,
                IsTribe = landedTitleEntity.IsTribe,
                UseShortName = landedTitleEntity.UseShortName,
                CapitalId = landedTitleEntity.CapitalId,
                Dignity = landedTitleEntity.Dignity,
                MonthlyIncome = landedTitleEntity.MonthlyIncome
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
                DynamicNames = landedTitle.DynamicNames,
                PrimaryColour = new int[] { landedTitle.PrimaryColour.R, landedTitle.PrimaryColour.G, landedTitle.PrimaryColour.B },
                SecondaryColour = new int[] { landedTitle.SecondaryColour.R, landedTitle.SecondaryColour.G, landedTitle.SecondaryColour.B },
                CultureId = landedTitle.CultureId,
                GraphicalCulture = landedTitle.GraphicalCulture,
                MercenaryType = landedTitle.MercenaryType,
                ReligionId = landedTitle.ReligionId,
                TitleFormOfAddress = landedTitle.TitleFormOfAddress,
                TitleLocalisationId = landedTitle.TitleLocalisationId,
                TitleLocalisationFemaleId = landedTitle.TitleLocalisationFemaleId,
                HasPurpleBornHeirs = landedTitle.HasPurpleBornHeirs,
                HasTopDeJureCapital = landedTitle.HasTopDeJureCapital,
                IsCaliphate = landedTitle.IsCaliphate,
                IsHolyOrder = landedTitle.IsHolyOrder,
                IsLandless = landedTitle.IsLandless,
                IsMercenaryGroup = landedTitle.IsMercenaryGroup,
                IsPrimaryTitle = landedTitle.IsPrimaryTitle,
                IsTribe = landedTitle.IsTribe,
                UseShortName = landedTitle.UseShortName,
                CapitalId = landedTitle.CapitalId,
                Dignity = landedTitle.Dignity,
                MonthlyIncome = landedTitle.MonthlyIncome
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
