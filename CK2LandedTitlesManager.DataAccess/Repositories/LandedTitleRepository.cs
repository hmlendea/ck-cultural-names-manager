using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using CK2LandedTitlesManager.DataAccess.DataObjects;
using CK2LandedTitlesManager.DataAccess.IO;

using Pdoxcl2Sharp;

namespace CK2LandedTitlesManager.DataAccess.Repositories
{
    public sealed class LandedTitleRepository : IRepository<string, LandedTitleEntity>
    {
        readonly string fileName;

        /// <summary>
        /// Initializes a new instance of the <see cref="LandedTitleRepository"/> class.
        /// </summary>
        /// <param name="fileName">File name.</param>
        public LandedTitleRepository(string fileName)
        {
            this.fileName = fileName;
        }

        public void Add(LandedTitleEntity landedTitle)
        {
            throw new NotImplementedException();
        }

        public LandedTitleEntity Get(string id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<LandedTitleEntity> GetAll()
        {
            LandedTitlesFile landedTitlesFile;
            using (FileStream fs = new FileStream(fileName, FileMode.Open))
            {
                landedTitlesFile = ParadoxParser.Parse(fs, new LandedTitlesFile());
            }
            
            return landedTitlesFile.LandedTitles.Select(x => (LandedTitleEntity)x);
        }

        public void Remove(string id)
        {
            throw new NotImplementedException();
        }
    }
}
