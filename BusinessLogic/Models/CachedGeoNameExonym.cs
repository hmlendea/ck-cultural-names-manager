using NuciDAL.DataObjects;

namespace CK2LandedTitlesManager.BusinessLogic.Models
{
    public sealed class CachedGeoNameExonym : EntityBase
    {
        public string PlaceName { get; set; }

        public string LanguageId { get; set; }

        public string Exonym { get; set; }
    }
}
