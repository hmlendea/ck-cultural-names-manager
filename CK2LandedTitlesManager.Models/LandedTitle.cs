namespace CK2LandedTitlesManager.Models
{
    /// <summary>
    /// The title entity
    /// </summary>
    public class LandedTitle : EntityBase
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
        /// Determines whether the specified <see cref="object"/> is equal to the current <see cref="LandedTitle"/>.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare with the current <see cref="LandedTitle"/>.</param>
        /// <returns><c>true</c> if the specified <see cref="object"/> is equal to the current
        /// <see cref="LandedTitle"/>; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            if (obj.ToString() == Text)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Serves as a hash function for a <see cref="LandedTitle"/> object.
        /// </summary>
        /// <returns>A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a
        /// hash table.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents the current <see cref="LandedTitle"/>.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents the current <see cref="LandedTitle"/>.</returns>
        public override string ToString()
        {
            return Text;
        }
    }
}
