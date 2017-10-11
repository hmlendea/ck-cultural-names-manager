namespace CK2LandedTitlesManager.Models
{
    /// <summary>
    /// The title name entity
    /// </summary>
    public class Name : EntityBase
    {
        /// <summary>
        /// Gets or sets the culture.
        /// </summary>
        /// <value>The culture.</value>
        public string Culture { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the title identifier.
        /// </summary>
        /// <value>The title identifier.</value>
        public int TitleId { get; set; }

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to the current <see cref="Name"/>.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare with the current <see cref="Name"/>.</param>
        /// <returns><c>true</c> if the specified <see cref="object"/> is equal to the current
        /// <see cref="Name"/>; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            Name other = obj as Name;

            if (other != null &&
                TitleId == other.TitleId &&
                Culture == other.Culture &&
                Text == other.Text)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Serves as a hash function for a <see cref="Name"/> object.
        /// </summary>
        /// <returns>A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a
        /// hash table.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}