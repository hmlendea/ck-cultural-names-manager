namespace CK2LandedTitlesExtractor.Entities
{
    /// <summary>
    /// The title entity
    /// </summary>
    public class Title : EntityBase
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the de jure title identifier.
        /// </summary>
        /// <value>The de jure title identifier.</value>
        public int DeJureTitleId { get; set; }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to the current <see cref="CK2LandedTitlesExtractor.Title"/>.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with the current <see cref="CK2LandedTitlesExtractor.Title"/>.</param>
        /// <returns><c>true</c> if the specified <see cref="System.Object"/> is equal to the current
        /// <see cref="CK2LandedTitlesExtractor.Title"/>; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            if (obj.ToString() == this.Text)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Serves as a hash function for a <see cref="CK2LandedTitlesExtractor.Entities.Title"/> object.
        /// </summary>
        /// <returns>A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a
        /// hash table.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the current <see cref="CK2LandedTitlesExtractor.Entities.Title"/>.
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents the current <see cref="CK2LandedTitlesExtractor.Entities.Title"/>.</returns>
        public override string ToString()
        {
            return Text;
        }
    }
}
