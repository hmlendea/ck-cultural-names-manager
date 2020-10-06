using System.Collections.Generic;
using System.IO;
using System.Linq;

using Pdoxcl2Sharp;

using CKCulturalNamesManager.DataAccess.DataObjects;

namespace CKCulturalNamesManager.DataAccess.IO
{
    public sealed class LandedTitlesFile<TLandedTitleDefinition> : IParadoxRead, IParadoxWrite where TLandedTitleDefinition : CKLandedTitleDefinition, new()
    {
        public IList<TLandedTitleDefinition> LandedTitles { get; set; }

        public LandedTitlesFile()
        {
            LandedTitles = new List<TLandedTitleDefinition>();
        }

        public void TokenCallback(ParadoxParser parser, string token)
        {
            TLandedTitleDefinition landedTitle = new TLandedTitleDefinition();
            landedTitle.LandedTitleEntity.Id = token;

            if (token.StartsWith('@') ||
                token.StartsWith("ï»¿"))
            {
                parser.ReadString();
            }
            else
            {
                LandedTitles.Add(parser.Parse(landedTitle));
            }
        }
        
        public void Write(ParadoxStreamWriter writer)
        {
            foreach (TLandedTitleDefinition landedTitle in LandedTitles)
            {
                writer.Write(landedTitle.LandedTitleEntity.Id, landedTitle);
            }
        }

        public static IEnumerable<LandedTitleEntity> ReadAllTitles(string fileName)
        {
            LandedTitlesFile<TLandedTitleDefinition> landedTitlesFile;

            using (FileStream fs = new FileStream(fileName, FileMode.Open))
            {
                landedTitlesFile = ParadoxParser.Parse(fs, new LandedTitlesFile<TLandedTitleDefinition>());
            }
            
            return landedTitlesFile.LandedTitles.Select(x => x.LandedTitleEntity);
        }

        public static void WriteAllTitles(string fileName, IEnumerable<LandedTitleEntity> landedTitles)
        {
            LandedTitlesFile<TLandedTitleDefinition> landedTitlesFile = new LandedTitlesFile<TLandedTitleDefinition>
            {
                LandedTitles = landedTitles.Select(x => new TLandedTitleDefinition { LandedTitleEntity = x }).ToList()
            };

            using (FileStream fs = new FileStream(fileName, FileMode.Create))
            using (ParadoxSaver saver = new ParadoxSaver(fs))
            {
                landedTitlesFile.Write(saver);
            }
        }
    }
}
