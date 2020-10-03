using System.Collections.Generic;

namespace CKCulturalNamesManager.DataAccess.DataObjects
{
    public class LandedTitleEntity
    {
        public string Id { get; set; }

        public string ParentId { get; set; }

        public IList<LandedTitleEntity> Children { get; set; }

        public IDictionary<string, string> Names { get; set; }

        public LandedTitleEntity()
        {
            Children = new List<LandedTitleEntity>();
            Names = new Dictionary<string, string>();
        }
    }
}
