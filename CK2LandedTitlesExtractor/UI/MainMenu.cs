using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using CK2LandedTitlesExtractor.Entities;
using CK2LandedTitlesExtractor.Repositories;
using CK2LandedTitlesExtractor.Utils.Extensions;

namespace CK2LandedTitlesExtractor.UI
{
    /// <summary>
    /// Main menu.
    /// </summary>
    public class MainMenu : Menu
    {
        static TitleRepository titleRepository;
        static NameRepository nameRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="CK2LandedTitlesExtractor.UI.Mainmenu"/> class.
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

            titleRepository = new TitleRepository();
            nameRepository = new NameRepository();
        }

        /// <summary>
        /// Outputs the title names
        /// </summary>
        private void DisplayLandedTitles()
        {
            foreach (Name name in nameRepository.GetAll())
            {
                Title title = titleRepository.Get(name.TitleId);

                Console.WriteLine(
                    "{0} = {{ {1} = \"{2}\" }}",
                    title.Text.PadRight(23, ' '),
                    name.Culture,
                    name.Text);
            }
        }

        /// <summary>
        /// Saves the landed titles to the specified file
        /// </summary>
        /// /// <param name="fileName">Path to the output landed_title file</param>
        private void SaveLandedTitles(string fileName)
        {
            if (File.Exists(fileName))
                File.Delete(fileName);

            StreamWriter sw = new StreamWriter(File.OpenWrite(fileName));

            foreach (Title title in titleRepository.GetAll())
            {
                List<Name> names = nameRepository.GetAll()
                                                 .Where(x => x.TitleId == title.Id)
                                                 .OrderBy(x => x.Culture)
                                                 .ToList();

                if (names.Count == 0)
                    continue;

                sw.WriteLine("{0} = {{", title);

                foreach (Name name in names)
                    sw.WriteLine("  {0} = \"{1}\"", name.Culture, name.Text);

                sw.WriteLine("}");
            }

            sw.Dispose();
        }

        /// <summary>
        /// Cleans the titles and names
        /// </summary>
        private void CleanTitlesAndNames()
        {
            foreach (Title title in titleRepository.GetAll())
            {
                List<Title> titlesDupes = titleRepository.GetAllByText(title.Text)
                                                     .Where(x => x.Id != title.Id)
                                                     .ToList();

                foreach (Title titleDupe in titlesDupes)
                {
                    foreach (Name name in nameRepository.GetAllByTitleId(titleDupe.Id))
                        name.TitleId = title.Id;

                    titleRepository.Remove(titleDupe);
                }
            }

            foreach (Name name in nameRepository.GetAll())
            {
                List<Name> namesDupes = nameRepository.GetAllByTitleId(name.TitleId)
                                                      .Where(x => x.Id != name.Id &&
                                                                  x.Culture == name.Culture &&
                                                                  x.Text == name.Text)
                                                      .ToList();
                
                foreach (Name nameDupe in namesDupes)
                {
                    nameRepository.Remove(nameDupe);
                }
            }
        }

        /// <summary>
        /// Loads the landed_title file
        /// </summary>
        private void LoadFile()
        {
            string fileName = Input("Input file path (absolute) = ");
            List<string> lines = File.ReadAllLines(Path.GetFullPath(fileName)).ToList();

            Console.Write("Loading titles... ");
            titleRepository.Load(fileName);
            Console.WriteLine("OK ({0} titles)", titleRepository.Size);

            Console.Write("Loading names... ");
            nameRepository.Load(fileName);
            Console.WriteLine("OK ({0} names)", nameRepository.Size);

            Console.Write("Linking names with titles... ");
            LinkNamesWithTitles();
            Console.WriteLine("OK");

            Console.Write("Cleaning titles and names... ");
            CleanTitlesAndNames();
            Console.WriteLine("OK ");

            Console.WriteLine($"{titleRepository.Size} title; {nameRepository.Size} names");
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
            foreach (Name name in nameRepository.GetAll())
            {
                for (int titleKey = name.Id; titleKey > 0; titleKey--)
                    if (titleRepository.Contains(titleKey))
                    {
                        name.TitleId = titleKey;
                        break;
                    }
            }
        }
    }
}
