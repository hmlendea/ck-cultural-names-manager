using System.Collections.Generic;

namespace CK2LandedTitlesManager.Models
{
    public sealed class CultureGroup
    {
        public CulturalGroupMatchingMode MatchingMode { get; }

        public IList<string> CultureIds { get; }

        public CultureGroup(CulturalGroupMatchingMode matchingMode, IList<string> cultureIds)
        {
            MatchingMode = matchingMode;
            CultureIds = cultureIds;
        }

        public CultureGroup(CulturalGroupMatchingMode matchingMode, params string[] cultureIds)
        {
            MatchingMode = matchingMode;
            CultureIds = cultureIds;
        }
    }
}
