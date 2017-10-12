using System.Collections.Generic;

namespace CK2LandedTitlesManager.DataAccess.DataObjects
{
    public class LandedTitleEntity
    {
        public string Id { get; set; }

        public string ParentId { get; set; }

        public IList<LandedTitleEntity> Children { get; set; }

        public IDictionary<string, string> DynamicNames { get; set; }

        /// <summary>
        /// Gets or sets the religious values.
        /// </summary>
        /// <value>The religious values.</value>
        public IDictionary<string, int> ReligiousValues { get; set; }

        public int[] PrimaryColour { get; set; }

        public int[] SecondaryColour { get; set; }

        public string CultureId { get; set; }

        public string GraphicalCulture { get; set; }

        public string MercenaryType { get; set; }

        public string ReligionId { get; set; }

        public string TitleFormOfAddress { get; set; }

        public string TitleLocalisationId { get; set; }

        public string TitleLocalisationFemaleId { get; set; }

        public bool HasPurpleBornHeirs { get; set; }

        public bool HasTopDeJureCapital { get; set; }

        public bool IsCaliphate { get; set; }

        public bool IsHolyOrder { get; set; }

        public bool IsLandless { get; set; }

        public bool IsMercenaryGroup { get; set; }

        public bool IsPrimaryTitle { get; set; }

        public bool IsTribe { get; set; }

        public bool UseShortName { get; set; }

        public int CapitalId { get; set; }

        public int Dignity { get; set; }

        public int MonthlyIncome { get; set; }

        public LandedTitleEntity()
        {
            DynamicNames = new Dictionary<string, string>();
            ReligiousValues = new Dictionary<string, int>();
            Children = new List<LandedTitleEntity>();
            PrimaryColour = new int[] { 0, 0, 0 };
            SecondaryColour = new int[] { 0, 0, 0 };
        }
    }
}
