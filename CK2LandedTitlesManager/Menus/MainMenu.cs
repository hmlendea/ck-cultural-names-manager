using System;
using System.Collections.Generic;
using System.Linq;

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
            int titlesCount = landedTitles.Sum(x => x.TotalChildren);

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
    }
}
