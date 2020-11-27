using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

using CKCulturalNamesManager.DataAccess.DataObjects;
using CKCulturalNamesManager.DataAccess.IO;
using CKCulturalNamesManager.BusinessLogic.Mapping;
using CKCulturalNamesManager.Models;

namespace CKCulturalNamesManager.DataAccess
{
    public sealed class FileManager : IFileManager
    {
        public IEnumerable<LandedTitle> Read(string fileName)
        {
            string filePath = NormaliseFilePath(fileName);
            string content = File.ReadAllText(filePath);

            IEnumerable<LandedTitleEntity> masterTitleEntities;

            if (content.Contains("cultural_names"))
            {
                masterTitleEntities = LandedTitlesFile<CK3LandedTitleDefinition>.ReadAllTitles(filePath);
            }
            else
            {
                masterTitleEntities = LandedTitlesFile<CK2LandedTitleDefinition>.ReadAllTitles(filePath);
            }

            return masterTitleEntities.ToDomainModels();
        }

        public void Write(IEnumerable<LandedTitle> landedTitles, string filePath, string game)
        {
            switch (game.ToLowerInvariant())
            {
                case "ck2":
                    WriteCK2(landedTitles, filePath);
                    break;

                case "ck3":
                    WriteCK3(landedTitles, filePath);
                    break;
                    
                default:
                    throw new ArgumentException("Invalid game");
            }
        }

        void WriteCK2(IEnumerable<LandedTitle> landedTitles, string fileName)
        {
            string filePath = NormaliseFilePath(fileName);
            LandedTitlesFile<CK2LandedTitleDefinition>.WriteAllTitles(filePath, landedTitles.ToEntities());
            string content = ReadWindows1252File(filePath);

            content = Regex.Replace(content, "\t", "    ");
            content = Regex.Replace(content, "= *(\r\n|\r|\n).*{", "={");
            content = Regex.Replace(content, "=", " = ");
            content = Regex.Replace(content, "\"(\r\n|\r|\n)( *[ekdcb]_)", "\"\n\n$2");

            WriteWindows1252File(filePath, content);
        }

        void WriteCK3(IEnumerable<LandedTitle> landedTitles, string fileName)
        {
            string filePath = NormaliseFilePath(fileName);
            LandedTitlesFile<CK3LandedTitleDefinition>.WriteAllTitles(filePath, landedTitles.ToEntities());
        }

        string ReadWindows1252File(string filePath)
        {
            Encoding encoding = Encoding.GetEncoding("windows-1252");
            
            return File.ReadAllText(filePath, encoding);
        }

        void WriteWindows1252File(string filePath, string content)
        {
            Encoding encoding = Encoding.GetEncoding("windows-1252");
            byte[] contentBytes = encoding.GetBytes(content.ToCharArray());
            
            File.WriteAllBytes(filePath, contentBytes);
        }

        string NormaliseFilePath(string filePath)
        {
            if (filePath.EndsWith(".txt"))
            {
                return filePath;
            }

            return $"{filePath}.txt";
        }
    }
}