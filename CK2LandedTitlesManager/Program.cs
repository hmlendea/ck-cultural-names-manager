using System.IO;

using CK2LandedTitlesManager.DataAccess.IO;
using CK2LandedTitlesManager.Menus;

using Pdoxcl2Sharp;

namespace CK2LandedTitlesManager
{
    public class Program
    {
        /// <summary>
        /// The entry point of the program, where the program control starts and ends.
        /// </summary>
        /// /// <param name="args">CLI arguments</param>
        public static void Main(string[] args)
        {
            Test();
            return;

            MainMenu mainMenu = new MainMenu();
            mainMenu.Run();
        }

        static void Test()
        {
            LandedTitlesFile landedTitlesFile;
            using (FileStream fs = new FileStream("test.txt", FileMode.Open))
            {
                landedTitlesFile = ParadoxParser.Parse(fs, new LandedTitlesFile());
            }

            // Save the information into a new file
            using (FileStream fs = new FileStream("test.new.txt", FileMode.Create))
            using (ParadoxSaver saver = new ParadoxSaver(fs))
            {
                landedTitlesFile.Write(saver);
            }
        }
    }
}
