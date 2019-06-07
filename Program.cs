using CK2LandedTitlesManager.Menus;

using NuciCLI.Menus;

namespace CK2LandedTitlesManager
{
    public class Program
    {
        /// <summary>
        /// The entry point of the program, where the program control starts and ends.
        /// </summary>
        /// <param name="args">CLI arguments</param>
        public static void Main(string[] args)
        {
            MenuManager.Instance.AreStatisticsEnabled = true;
            MenuManager.Instance.Start<MainMenu>();
        }
    }
}
