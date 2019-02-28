using System;
using System.Collections.Generic;
using System.Linq;

using NuciCLI;
using NuciCLI.Menus;

using CK2LandedTitlesManager.BusinessLogic;
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

            AreStatisticsEnabled = true;
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
                "remove-title",
                "Removes the specified title",
                delegate { RemoveTitle(); });

            AddCommand(
                "remove-unlocalised-titles",
                "Removes the titles that contain no dynamic names",
                delegate { RemoveUnlocalisedTitles(); });

            AddCommand(
                "get-suggestions",
                "Suggests names for cultures in the same group",
                delegate { GetSuggestions(); });

            AddCommand(
                "apply-suggestions",
                "Applies all suggestions",
                delegate { ApplySuggestions(); });

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

        private void RemoveTitle()
        {
            string titleId = NuciConsole.ReadLine("Title to remove = ");

            landedTitleManager.RemoveTitle(titleId);
        }

        private void RemoveUnlocalisedTitles()
        {
            landedTitleManager.RemoveUnlocalisedTitles();
        }

        private void ApplySuggestions()
        {
            //string fileName = Input("File = ");

            landedTitleManager.ApplySuggestions();//(fileName);
        }

        private void GetSuggestions()
        {
            IEnumerable<CulturalGroupSuggestion> suggestions = landedTitleManager.GetCulturalGroupSuggestions().Reverse();
            IEnumerable<string> titlesWithSuggestions = suggestions.Select(x => x.TitleId).Distinct();

            int titleColWidth = suggestions.Select(x => x.TitleId).Max(x => x.Length);

            if (suggestions.Count() == 0)
            {
                NuciConsole.WriteLine("There are no suggestions!");
            }
            else
            {
                foreach (string titleId in titlesWithSuggestions)
                {
                    IEnumerable<CulturalGroupSuggestion> suggestionsForTitle = suggestions
                        .Where(x => x.TitleId == titleId)
                        .OrderBy(x => x.TargetCultureId);

                    NuciConsole.Write("Suggestions for ");
                    NuciConsole.Write(titleId, Colour.Yellow);
                    NuciConsole.WriteLine(" :");

                    foreach (CulturalGroupSuggestion suggestion in suggestionsForTitle)
                    {
                        NuciConsole.WriteLine(
                            $"{suggestion.TargetCultureId} = \"{suggestion.SuggestedName}\" " +
                            $"# Historical? Copied from {GetCultureNameFromId(suggestion.SourceCultureId)}");
                    }
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
    }
}
