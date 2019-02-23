using System;
using System.Collections.Generic;
using System.Linq;

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
                "Removes the dynamic names contained in the specified file",
                delegate { RemoveNames(); });

            AddCommand(
                "get-suggestions",
                "Suggests names for cultures in the same group",
                delegate { GetSuggestions(); });

            AddCommand(
                "integrity-check",
                "Checks if a landed titles structure is compatible as a mod for a master file",
                delegate { IntegrityCheck(); });
        }

        /// <summary>
        /// Displays the landed title with the specified ID.
        /// </summary>
        private void Get()
        {
            string id = Input("ID = ");

            LandedTitle landedTitle = landedTitleManager.Get(id);

            if (landedTitle == null)
            {
                Console.WriteLine($"Cannot find title {id}!");
                return;
            }

            if (landedTitle.DynamicNames.Count == 0)
            {
                Console.WriteLine($"There are no dynamic titles for {id}!");
                return;
            }

            foreach (var dynamicName in landedTitle.DynamicNames)
            {
                Console.WriteLine($"{landedTitle.Id}.{dynamicName.Key} = \"{dynamicName.Value}\"");
            }
        }

        private void RemoveNames()
        {
            string fileName = Input("File containing the names to remove (absolute) = ");

            landedTitleManager.RemoveDynamicNamesFromFile(fileName);

            IEnumerable<LandedTitle> landedTitles = landedTitleManager.GetAll();
            int titlesCount = landedTitleManager.GetAll().Count();

            Console.WriteLine($"OK");
        }

        private void GetSuggestions()
        {
            IEnumerable<CulturalGroupSuggestion> suggestions = landedTitleManager.GetCulturalGroupSuggestions();

            int titleColWidth = suggestions.Select(x => x.TitleId).Max(x => x.Length);

            if (suggestions.Count() == 0)
            {
                Console.WriteLine("There are no suggestions!");
            }
            else
            {
                foreach (CulturalGroupSuggestion suggestion in suggestions)
                {
                    //Console.WriteLine($"{suggestion.TitleId}\t{suggestion.SourceCultureId}\t=> {suggestion.TargetCultureId}\t({suggestion.SuggestedName})");
                    Console.WriteLine(
                        $"{(suggestion.TitleId + " ").PadRight(titleColWidth, '-')} " + 
                        $"{suggestion.TargetCultureId} = \"{suggestion.SuggestedName}\" " +
                        $"# Historical? Copied from {GetCultureNameFromId(suggestion.SourceCultureId)}");
                }
            }
        }

        private void IntegrityCheck()
        {
            string fileName = Input("Master file to check compatibility with (absolute) = ");

            bool isValid = landedTitleManager.CheckIntegrity(fileName);

            if (isValid)
            {
                Console.WriteLine("The structure passed the integrity check!");
            }
            else
            {
                Console.WriteLine("The structure FAILED the integrity check!!!");
            }
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
            string fileName = Input("Input file path (absolute) = ");

            Console.Write($"Loading titles from \"{fileName}\"... ");

            landedTitleManager.LoadTitles(fileName);

            IEnumerable<LandedTitle> landedTitles = landedTitleManager.GetAll();
            int titlesCount = landedTitleManager.GetAll().Count();

            Console.WriteLine($"OK ({titlesCount} titles)");
        }

        /// <summary>
        /// Saves the titles and names to file
        /// </summary>
        private void SaveFile()
        {
            string fileName = Input("Output file path (absolute) = ");

            Console.Write("Writing output... ");
            SaveLandedTitles(fileName);
            Console.WriteLine("OK");
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
