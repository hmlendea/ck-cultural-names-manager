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
                FemaleNames = landedTitleEntity.FemaleNames,
                MaleNames = landedTitleEntity.MaleNames,
                HolySites = landedTitleEntity.HolySites,
                DynamicNames = landedTitleEntity.DynamicNames.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                PrimaryColour = GetColorFromIntArray(landedTitleEntity.PrimaryColour),
                SecondaryColour = GetColorFromIntArray(landedTitleEntity.SecondaryColour),
                ControlledReligionId = landedTitleEntity.ControlledReligionId,
                CultureId = landedTitleEntity.CultureId,
                GraphicalCulture = landedTitleEntity.GraphicalCulture,
                MercenaryType = landedTitleEntity.MercenaryType,
                ReligionId = landedTitleEntity.ReligionId,
                TitleFormOfAddress = landedTitleEntity.TitleFormOfAddress,
                TitleLocalisationId = landedTitleEntity.TitleLocalisationId,
                TitleLocalisationFemaleId = landedTitleEntity.TitleLocalisationFemaleId,
                TitleLocalisationPrefixId = landedTitleEntity.TitleLocalisationPrefixId,
                TitleNameTierId = landedTitleEntity.TitleNameTierId,
                AllowsAssimilation = landedTitleEntity.AllowsAssimilation,
                CreationRequiresCapital = landedTitleEntity.CreationRequiresCapital,
                TitleContainsCapital = landedTitleEntity.TitleContainsCapital,
                HasPurpleBornHeirs = landedTitleEntity.HasPurpleBornHeirs,
                HasTopDeJureCapital = landedTitleEntity.HasTopDeJureCapital,
                IsCaliphate = landedTitleEntity.IsCaliphate,
                IsHolyOrder = landedTitleEntity.IsHolyOrder,
                IsIndependent = landedTitleEntity.IsIndependent,
                IsLandless = landedTitleEntity.IsLandless,
                IsMercenaryGroup = landedTitleEntity.IsMercenaryGroup,
                IsPirate = landedTitleEntity.IsPirate,
                IsPrimaryTitle = landedTitleEntity.IsPrimaryTitle,
                IsTribe = landedTitleEntity.IsTribe,
                UseDynastyTitleNames = landedTitleEntity.UseDynastyTitleNames,
                UseShortName = landedTitleEntity.UseShortName,
                StrengthGrowthPerCentury = landedTitleEntity.StrengthGrowthPerCentury,
                CapitalId = landedTitleEntity.CapitalId,
                Dignity = landedTitleEntity.Dignity,
                MonthlyIncome = landedTitleEntity.MonthlyIncome
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
                FemaleNames = landedTitle.FemaleNames,
                MaleNames = landedTitle.MaleNames,
                HolySites = landedTitle.HolySites,
                DynamicNames = landedTitle.DynamicNames,
                PrimaryColour = new int[] { landedTitle.PrimaryColour.R, landedTitle.PrimaryColour.G, landedTitle.PrimaryColour.B },
                SecondaryColour = new int[] { landedTitle.SecondaryColour.R, landedTitle.SecondaryColour.G, landedTitle.SecondaryColour.B },
                CultureId = landedTitle.CultureId,
                ControlledReligionId = landedTitle.ControlledReligionId,
                GraphicalCulture = landedTitle.GraphicalCulture,
                MercenaryType = landedTitle.MercenaryType,
                ReligionId = landedTitle.ReligionId,
                TitleFormOfAddress = landedTitle.TitleFormOfAddress,
                TitleLocalisationId = landedTitle.TitleLocalisationId,
                TitleLocalisationFemaleId = landedTitle.TitleLocalisationFemaleId,
                TitleLocalisationPrefixId = landedTitle.TitleLocalisationPrefixId,
                TitleNameTierId = landedTitle.TitleNameTierId,
                AllowsAssimilation = landedTitle.AllowsAssimilation,
                CreationRequiresCapital = landedTitle.CreationRequiresCapital,
                TitleContainsCapital = landedTitle.TitleContainsCapital,
                HasPurpleBornHeirs = landedTitle.HasPurpleBornHeirs,
                HasTopDeJureCapital = landedTitle.HasTopDeJureCapital,
                IsCaliphate = landedTitle.IsCaliphate,
                IsHolyOrder = landedTitle.IsHolyOrder,
                IsIndependent = landedTitle.IsIndependent,
                IsLandless = landedTitle.IsLandless,
                IsMercenaryGroup = landedTitle.IsMercenaryGroup,
                IsPirate = landedTitle.IsPirate,
                IsPrimaryTitle = landedTitle.IsPrimaryTitle,
                IsTribe = landedTitle.IsTribe,
                UseDynastyTitleNames = landedTitle.UseDynastyTitleNames,
                UseShortName = landedTitle.UseShortName,
                StrengthGrowthPerCentury = landedTitle.StrengthGrowthPerCentury,
                CapitalId = landedTitle.CapitalId,
                Dignity = landedTitle.Dignity,
                MonthlyIncome = landedTitle.MonthlyIncome
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
