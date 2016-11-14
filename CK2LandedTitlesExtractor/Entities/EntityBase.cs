using System.ComponentModel.DataAnnotations;

namespace CK2LandedTitlesExtractor.Entities
{
    /// <summary>
    /// Entity base.
    /// </summary>
    public abstract class EntityBase
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        [Key]
        public virtual int Id { get; set; }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to the current <see cref="CK2LandedTitlesExtractor.Entities.EntityBase"/>.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with the current <see cref="CK2LandedTitlesExtractor.Entities.EntityBase"/>.</param>
        /// <returns><c>true</c> if the specified <see cref="System.Object"/> is equal to the current
        /// <see cref="CK2LandedTitlesExtractor.Entities.EntityBase"/>; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            EntityBase other = obj as EntityBase;
            return Id == other?.Id;
        }

        /// <summary>
        /// Serves as a hash function for a <see cref="CK2LandedTitlesExtractor.Entities.EntityBase"/> object.
        /// </summary>
        /// <returns>A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a
        /// hash table.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the current <see cref="CK2LandedTitlesExtractor.Entities.EntityBase"/>.
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents the current <see cref="CK2LandedTitlesExtractor.Entities.EntityBase"/>.</returns>
        public override string ToString()
        {
            return string.Format("{0} #{1}", base.ToString(), Id);
        }
    }
}
