using System.Collections.Generic;
using System.Linq;

using Pdoxcl2Sharp;
using NuciExtensions;

namespace CKCulturalNamesManager.DataAccess.IO
{
    public sealed class CK3CulturalNamesDefinition : IParadoxRead, IParadoxWrite
    {
        public IDictionary<string, string> Names { get; set; }

        public CK3CulturalNamesDefinition()
        {
            Names = new Dictionary<string, string>();
        }
        
        public void TokenCallback(ParadoxParser parser, string token)
        {
            string stringValue = parser.ReadString();
            Names.AddOrUpdate(token, stringValue);
        }
        
        public void Write(ParadoxStreamWriter writer)
        {
            List<KeyValuePair<string, string>> sortedNames = Names.ToList().OrderBy(x => x.Key).ToList();

            foreach(var name in sortedNames)
            {
                writer.WriteLine(name.Key, name.Value, ValueWrite.Quoted);
            }
        }
    }
}
