using System.Collections.Generic;

namespace CK2LandedTitlesManager.DataAccess.DataObjects
{
    public class LandedTitleEntity
    {
        public string Id { get; set; }

        public string ParentId { get; set; }

        public IList<LandedTitleEntity> Children { get; set; }

        public IDictionary<string, string> DynamicNames { get; set; }

        public LandedTitleEntity()
        {
            DynamicNames = new Dictionary<string, string>();
            Children = new List<LandedTitleEntity>();
        }
    }
}
