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
                "remove-names-except",
                "Removes all of the dynamic names except those of specified cultures",
                delegate { RemoveNamesExcept(); });

            AddCommand(
                "remove-names-from-file",
                "Removes the dynamic names contained in the specified file",
                delegate { RemoveNamesFromFile(); });

            AddCommand(
                "remove-redundant-names",
                "Removes the redundant names already contained in the specified file, or default names",
                delegate { RemoveRedundantNames(); });

            AddCommand(
                "remove-title",
                "Removes the specified title",
                delegate { RemoveTitle(); });

            AddCommand(
                "remove-unlocalised-titles",
                "Removes the titles that contain no dynamic names",
                delegate { RemoveUnlocalisedTitles(); });

            AddCommand(
                "get-suggestions-geonames",
                "Suggests names from GeoNames",
                delegate { GetGeoNamesSuggestions(); });

            AddCommand(
                "count-dynamic-names",
                "Gets the number of dynamic names for each culture",
                delegate { CountDynamicNames(); });

            AddCommand(
                "copy-names-from-culture",
                "Copies the names from a culture to another where they don't exist already",
                delegate { CopyNamesFromCulture(); });

            // TODO: Better command name and description, etc
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

        private void RemoveNamesExcept()
        {
            string cultures = NuciConsole.ReadLine("Cultures to keep = ");
            List<string> cultureIds = cultures
                .Replace("\"", "")
                .Replace(",", "")
                .Trim()
                .Split(' ')
                .ToList();

            landedTitleManager.RemoveDynamicNames(cultureIds);
        }

        private void RemoveNamesFromFile()
        {
            string fileName = NuciConsole.ReadLine("File containing the names to remove = ");

            landedTitleManager.RemoveDynamicNamesFromFile(fileName);

            IEnumerable<LandedTitle> landedTitles = landedTitleManager.GetAll();
            int titlesCount = landedTitleManager.GetAll().Count();

            NuciConsole.WriteLine($"OK");
        }

        private void RemoveRedundantNames()
        {
            string fileName = NuciConsole.ReadLine("Master file to check against = ");

            landedTitleManager.RemoveRedundantDynamicNames(fileName);
        }

        private void RemoveTitle()
        {
            string titleId = NuciConsole.ReadLine("Title to remove = ");

            landedTitleManager.RemoveTitle(titleId);
        }

        private void RemoveUnlocalisedTitles()
        {
            landedTitleManager.RemoveUnlocalisedTitles();
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

        private void GetGeoNamesSuggestions()
        {
            IEnumerable<GeoNamesSuggestion> geoNameSuggestions = landedTitleManager.GetGeoNamesSuggestion();
            IEnumerable<string> titlesWithGeoNameSuggestions = geoNameSuggestions.Select(x => x.TitleId).Distinct();

            int titleColWidth = geoNameSuggestions.Select(x => x.TitleId).Max(x => x.Length);

            if (geoNameSuggestions.Count() == 0)
            {
                NuciConsole.WriteLine("There are no suggestions!");
            }

            foreach (string titleId in titlesWithGeoNameSuggestions)
            {
                IEnumerable<GeoNamesSuggestion> suggestionsForTitle = geoNameSuggestions
                    .Where(x => x.TitleId == titleId)
                    .OrderBy(x => x.CultureId);
                
                string indentation = string.Empty.PadRight(TitleLevelIndentation[titleId[0]], ' ');

                NuciConsole.Write("Suggestions for ");
                NuciConsole.Write(titleId, NuciConsoleColour.Yellow);
                NuciConsole.Write(" (");
                NuciConsole.Write(suggestionsForTitle.First().Localisation, NuciConsoleColour.White);
                NuciConsole.WriteLine(") :");

                foreach (GeoNamesSuggestion suggestion in suggestionsForTitle)
                {
                    NuciConsole.WriteLine($"{indentation}{suggestion.CultureId} = \"{suggestion.SuggestedName}\"");
                }
            }
        }

        private void CopyNamesFromCulture()
        {
            string sourceCultureId = NuciConsole.ReadLine("Culture from which to copy = ");
            string targetCultureId = NuciConsole.ReadLine("Culture to which to copy to = ");

            landedTitleManager.CopyNamesFromCulture(sourceCultureId, targetCultureId);
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
