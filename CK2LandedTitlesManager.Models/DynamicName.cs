namespace CK2LandedTitlesManager.Models
{
    /// <summary>
    /// The title name entity
    /// </summary>
    public class DynamicName : EntityBase
    {
        /// <summary>
        /// Gets or sets the culture identifier.
        /// </summary>
        /// <value>The culture identifier.</value>
        public string CultureId { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the landed title identifier.
        /// </summary>
        /// <value>The landed title identifier.</value>
        public int LandedTitleId { get; set; }

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to the current <see cref="DynamicName"/>.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare with the current <see cref="DynamicName"/>.</param>
        /// <returns><c>true</c> if the specified <see cref="object"/> is equal to the current
        /// <see cref="LandedTitle"/>; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            DynamicName other = obj as DynamicName;

            if (other != null &&
                LandedTitleId == other.LandedTitleId &&
                CultureId == other.CultureId &&
                Name == other.Name)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Serves as a hash function for a <see cref="DynamicName"/> object.
        /// </summary>
        /// <returns>A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a
        /// hash table.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}