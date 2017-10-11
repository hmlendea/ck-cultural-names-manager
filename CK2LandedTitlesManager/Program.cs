using CK2LandedTitlesManager.UI;

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
            MainMenu mainMenu = new MainMenu();
            mainMenu.Run();
        }
    }
}
