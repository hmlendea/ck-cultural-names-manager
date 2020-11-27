using System.Collections.Generic;

using CKCulturalNamesManager.Models;

namespace CKCulturalNamesManager.DataAccess
{
    public interface IFileManager
    {
        IEnumerable<LandedTitle> Read(string fileName);

        void Write(IEnumerable<LandedTitle> landedTitles, string filePath, string game);
    }
}