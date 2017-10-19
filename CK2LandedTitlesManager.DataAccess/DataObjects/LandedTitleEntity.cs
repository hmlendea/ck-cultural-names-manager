using System.Collections.Generic;

namespace CK2LandedTitlesManager.DataAccess.DataObjects
{
    public class LandedTitleEntity
    {
        public string Id { get; set; }

        public string ParentId { get; set; }

        public IList<LandedTitleEntity> Children { get; set; }

        /// <summary>
        /// Gets or sets the female names.
        /// </summary>
        /// <value>The female names.</value>
        public IList<string> FemaleNames { get; set; }

        /// <summary>
        /// Gets or sets the male names.
        /// </summary>
        /// <value>The male banes.</value>
        public IList<string> MaleNames { get; set; }

        /// <summary>
        /// Gets or sets the holy sites.
        /// </summary>
        /// <value>The holy sites.</value>
        public IList<string> HolySites { get; set; }

        public IDictionary<string, string> DynamicNames { get; set; }

        /// <summary>
        /// Gets or sets the religious values.
        /// </summary>
        /// <value>The religious values.</value>
        public IDictionary<string, int> ReligiousValues { get; set; }

        public int[] PrimaryColour { get; set; }

        public int[] SecondaryColour { get; set; }

        public string ControlledReligionId { get;set; }

        public string CultureId { get; set; }

        public string GraphicalCulture { get; set; }

        public string MercenaryType { get; set; }

        public string ReligionId { get; set; }

        public string TitleFormOfAddress { get; set; }

        public string TitleLocalisationId { get; set; }

        public string TitleLocalisationFemaleId { get; set; }

        public string TitleLocalisationPrefixId { get; set; }

        public string TitleNameTierId { get; set; }

        public bool CreationRequiresCapital { get; set; }

        public bool TitleContainsCapital { get; set; }

        public bool HasPurpleBornHeirs { get; set; }

        public bool HasTopDeJureCapital { get; set; }

        public bool IsCaliphate { get; set; }

        public bool IsHolyOrder { get; set; }

        public bool IsIndependent { get; set; }

        public bool IsLandless { get; set; }

        public bool IsMercenaryGroup { get; set; }

        public bool IsPrimaryTitle { get; set; }

        public bool IsTribe { get; set; }

        public bool UseShortName { get; set; }
        
        public float StrengthGrowthPerCentury { get; set; }

        public int CapitalId { get; set; }

        public int Dignity { get; set; }

        public int MonthlyIncome { get; set; }

        public LandedTitleEntity()
        {
            Children = new List<LandedTitleEntity>();
            FemaleNames = new List<string>();
            MaleNames = new List<string>();
            HolySites = new List<string>();

            DynamicNames = new Dictionary<string, string>();
            ReligiousValues = new Dictionary<string, int>();

            PrimaryColour = new int[] { 0, 0, 0 };
            SecondaryColour = new int[] { 0, 0, 0 };
        }
    }
}
