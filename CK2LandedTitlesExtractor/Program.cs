using CK2LandedTitlesExtractor.UI;

namespace CK2LandedTitlesExtractor
{
    /// <summary>
    /// The title name class
    /// </summary>
    public class TitleName
    {
        public string Culture { get; set; }
        public string Name { get; set; }
        public int TitleId { get; set; }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to the current <see cref="CK2LandedTitlesExtractor.TitleName"/>.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with the current <see cref="CK2LandedTitlesExtractor.TitleName"/>.</param>
        /// <returns><c>true</c> if the specified <see cref="System.Object"/> is equal to the current
        /// <see cref="CK2LandedTitlesExtractor.TitleName"/>; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            TitleName other = obj as TitleName;

            if (this.TitleId == other.TitleId &&
                this.Culture == other.Culture &&
                this.Name == other.Name)
                return true;

            return false;
        }
    }

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
