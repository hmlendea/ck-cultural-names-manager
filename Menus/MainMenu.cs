using System;
using System.Collections.Generic;
using System.Linq;

using NuciCLI;
using NuciCLI.Menus;

using CK2LandedTitlesManager.BusinessLogic;
using CK2LandedTitlesManager.BusinessLogic.Models;
using CK2LandedTitlesManager.Models;

namespace CK2LandedTitlesManager.Menus
{
    /// <summary>
    /// Main menu.
    /// </summary>
    public class MainMenu : Menu
    {
        LandedTitleManager landedTitleManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainMenu"/> class.
        /// </summary>
        public MainMenu()
        {
            landedTitleManager = new LandedTitleManager();

            Title = "CK2 Landed Titles Extractor";

            AddCommand(
                "load",
                "Load the input file (landed_titles.txt)",
                delegate { LoadFile(); });

            AddCommand(
                "save",
                "save the output file",
                delegate { SaveFile(); });

            AddCommand(
                "get",
                "Displays the landed title with the specified ID",
                delegate { Get(); });

            AddCommand(
                "remove-names",
                "Removes all of the dynamic names",
                delegate { RemoveNames(); });

            AddCommand(
                "remove-names-from-file",
                "Removes the dynamic names contained in the specified file",
                delegate { RemoveNamesFromFile(); });

            AddCommand(
                "count-dynamic-names",
                "Gets the number of dynamic names for each culture",
                delegate { CountDynamicNames(); });

            AddCommand(
                "get-cultural-names",
                "Gets the names of all titles that contain all specified cultures",
                delegate { GetNamesOfCultures(); });

            AddCommand(
                "get-overwritten-dynamic-names",
                "Gets the names that were ovewritten",
                delegate { GetOverwrittenDynamicNames(); });

            AddCommand(
                "integrity-check",
                "Checks if a landed titles structure is compatible as a mod for a master file",
                delegate { IntegrityCheck(); });

            AddCommand(
                "clean-file",
                "Cleans a landed titles file",
                delegate { CleanFile(); });
        }

        /// <summary>
        /// Displays the landed title with the specified ID.
        /// </summary>
        private void Get()
        {
            string id = NuciConsole.ReadLine("ID = ");

            LandedTitle landedTitle = landedTitleManager.Get(id);

            if (landedTitle == null)
            {
                NuciConsole.WriteLine($"Cannot find title {id}!");
                return;
            }

            if (landedTitle.DynamicNames.Count == 0)
            {
                NuciConsole.WriteLine($"There are no dynamic titles for {id}!");
                return;
            }

            foreach (var dynamicName in landedTitle.DynamicNames)
            {
                NuciConsole.WriteLine($"{landedTitle.Id}.{dynamicName.Key} = \"{dynamicName.Value}\"");
            }
        }

        private void RemoveNames()
        {
            landedTitleManager.RemoveDynamicNames();
        }

        private void RemoveNamesFromFile()
        {
            string fileName = NuciConsole.ReadLine("File containing the names to remove = ");

            landedTitleManager.RemoveDynamicNamesFromFile(fileName);

            IEnumerable<LandedTitle> landedTitles = landedTitleManager.GetAll();
            int titlesCount = landedTitleManager.GetAll().Count();

            NuciConsole.WriteLine($"OK");
        }

        private void CountDynamicNames()
        {
            IDictionary<string, int> dynamicNamesCount = landedTitleManager.GetDynamicNamesCount();

            foreach (string cultureId in dynamicNamesCount.Keys)
            {
                NuciConsole.WriteLine($"{cultureId.PadRight(20, ' ')}{dynamicNamesCount[cultureId]}");
            }

            NuciConsole.WriteLine();
            NuciConsole.WriteLine($"TOTAL {dynamicNamesCount.Sum(x => x.Value)}");
        }

        private void GetNamesOfCultures()
        {
            string cultures = NuciConsole.ReadLine("Cultures to search = ");
            List<string> cultureIds = cultures
                .Replace("\"", "")
                .Replace(",", "")
                .Trim()
                .Split(' ')
                .ToList();

            List<string> findings = landedTitleManager.GetNamesOfTitlesWithAllCultures(cultureIds);

            int titlesCount = findings.Count / cultureIds.Count;
            int perfectMatches = 0;
            
            foreach (string finding in findings)
            {
                NuciConsole.WriteLine(finding);

                if (finding.StartsWith('X'))
                {
                    perfectMatches += 1;
                }
            }

            perfectMatches = perfectMatches / cultureIds.Count;

            NuciConsole.WriteLine();
            NuciConsole.WriteLine("A grand total of :");
            NuciConsole.WriteLine($"   {titlesCount} titles");
            NuciConsole.WriteLine($"   {perfectMatches} perfect matches");
        }

        private void GetOverwrittenDynamicNames()
        {
            string fileName = NuciConsole.ReadLine("Master file to check compatibility with = ");

            IEnumerable<OverwrittenDynamicName> overwrittenNames = landedTitleManager.GetOverwrittenDynamicNames(fileName);
            IEnumerable<string> titlesWithOverwrittenNames = overwrittenNames.Select(x => x.TitleId).Distinct().Reverse();

            int titleColWidth = overwrittenNames.Select(x => x.TitleId).Max(x => x.Length);

            if (overwrittenNames.Count() == 0)
            {
                NuciConsole.WriteLine("There are no overwritten names!");
            }

            foreach (string titleId in titlesWithOverwrittenNames)
            {
                IEnumerable<OverwrittenDynamicName> overwrittenNamesForTitle = overwrittenNames
                    .Where(x => x.TitleId == titleId)
                    .OrderBy(x => x.CultureId);
                
                string indentation = string.Empty.PadRight(TitleLevelIndentation[titleId[0]], ' ');

                NuciConsole.Write("Overwritten names for ");
                NuciConsole.Write(titleId, NuciConsoleColour.Yellow);
                NuciConsole.Write(" (");
                NuciConsole.Write(overwrittenNamesForTitle.First().Localisation, NuciConsoleColour.White);
                NuciConsole.WriteLine(") :");

                foreach (OverwrittenDynamicName overwrittenName in overwrittenNamesForTitle)
                {
                    NuciConsole.WriteLine(
                        $"{indentation}{overwrittenName.CultureId} = \"{overwrittenName.FinalName}\"" +
                        $" # Overwrites \"{overwrittenName.OriginalName}\"");
                }
            }
        }

        private void IntegrityCheck()
        {
            string fileName = NuciConsole.ReadLine("Master file to check compatibility with = ");

            bool isValid = landedTitleManager.CheckIntegrity(fileName);

            if (isValid)
            {
                NuciConsole.WriteLine("The structure passed the integrity check!");
            }
            else
            {
                NuciConsole.WriteLine("The structure FAILED the integrity check!!!");
            }
        }
        
        private void CleanFile()
        {
            string fileName = NuciConsole.ReadLine("File to clean = ");

            landedTitleManager.CleanFile(fileName);
        }

        /// <summary>
        /// Saves the landed titles to the specified file
        /// </summary>
        /// /// <param name="fileName">Path to the output landed_title file</param>
        private void SaveLandedTitles(string fileName)
        {
            landedTitleManager.SaveTitles(fileName);
        }

        /// <summary>
        /// Loads the landed_title file
        /// </summary>
        private void LoadFile()
        {
            string fileName = NuciConsole.ReadLine("Input file path = ");

            NuciConsole.Write($"Loading titles from \"{fileName}\"... ");

            landedTitleManager.LoadTitles(fileName);

            IEnumerable<LandedTitle> landedTitles = landedTitleManager.GetAll();
            int titlesCount = landedTitleManager.GetAll().Count();

            NuciConsole.WriteLine($"OK ({titlesCount} titles)");
        }

        /// <summary>
        /// Saves the titles and names to file
        /// </summary>
        private void SaveFile()
        {
            string fileName = NuciConsole.ReadLine("Output file path = ");

            NuciConsole.Write("Writing output... ");
            SaveLandedTitles(fileName);
            NuciConsole.WriteLine("OK");
        }

        private string GetCultureNameFromId(string cultureId)
        {
            string cultureName = char.ToUpper(cultureId[0]).ToString();

            for (int i = 1; i < cultureId.Length; i++)
            {
                if (cultureId[i] == '_')
                {
                    cultureName += char.ToUpper(cultureId[i + 1]);
                    i++;
                    continue;
                }

                cultureName += cultureId[i];
            }

            return cultureName;
        }

        readonly IDictionary<char, int> TitleLevelIndentation = new Dictionary<char, int>
        {
            { 'b', 20 },
            { 'c', 16 },
            { 'd', 12 },
            { 'k', 8 },
            { 'e', 4 }
        };
    }
}
