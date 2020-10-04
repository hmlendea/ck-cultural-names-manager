using Pdoxcl2Sharp;
using NuciExtensions;

using CKCulturalNamesManager.DataAccess.DataObjects;

namespace CKCulturalNamesManager.DataAccess.IO
{
    public abstract class CKLandedTitleDefinition : IParadoxRead, IParadoxWrite
    {
        public LandedTitleEntity LandedTitleEntity { get; set; }

        public CKLandedTitleDefinition()
        {
            LandedTitleEntity = new LandedTitleEntity();
        }
        
        public abstract void TokenCallback(ParadoxParser parser, string token);
        
        public abstract void Write(ParadoxStreamWriter writer);
    }
}
