using System;

using Pdoxcl2Sharp;

namespace CK2LandedTitlesManager.DataAccess.IO
{
    public sealed class LandedTitlesFile : IParadoxRead, IParadoxWrite
    {
        public void TokenCallback(ParadoxParser parser, string token)
        {
            throw new NotImplementedException();
        }
        
        public void Write(ParadoxStreamWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
