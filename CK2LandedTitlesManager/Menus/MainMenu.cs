using System;
using System.Collections.Generic;
using System.Linq;

using CK2LandedTitlesManager.DataAccess.Repositories;
using CK2LandedTitlesManager.Mapping;
using CK2LandedTitlesManager.Models;

namespace CK2LandedTitlesManager.Menus
{
    /// <summary>
    /// Main menu.
    /// </summary>
    public class MainMenu : Menu
    {
        List<LandedTitle> landedTitles;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainMenu"/> class.
        /// </summary>
        public MainMenu()
        {
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
                "print",
                "Display the landed titles",
                delegate { DisplayLandedTitles(); });
        }

        /// <summary>
        /// Outputs the title names
        /// </summary>
        private void DisplayLandedTitles()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Saves the landed titles to the specified file
        /// </summary>
        /// /// <param name="fileName">Path to the output landed_title file</param>
        private void SaveLandedTitles(string fileName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Cleans the titles and names
        /// </summary>
        private void CleanTitlesAndNames()
        {
            Console.Write("Cleaning titles and names... ");

            throw new NotImplementedException();

            Console.WriteLine("OK ");
        }

        private void LoadTitles(string fileName)
        {
            Console.Write($"Loading titles from \"{fileName}\"... ");

            LandedTitleRepository repository = new LandedTitleRepository(fileName);
            landedTitles = repository.GetAll().ToDomainModels().ToList();

            int titlesCount = landedTitles.Sum(x => x.TotalChildren);
            Console.WriteLine($"OK ({titlesCount} titles)");
        }

        /// <summary>
        /// Loads the landed_title file
        /// </summary>
        private void LoadFile()
        {
            string fileName = Input("Input file path (absolute) = ");

            LoadTitles(fileName);
            CleanTitlesAndNames();
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

        /// <summary>
        /// Links names with their respective titles
        /// </summary>
        private void LinkNamesWithTitles()
        {
            throw new NotImplementedException();
        }
    }
}
