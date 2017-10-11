using System.ComponentModel.DataAnnotations;

namespace CK2LandedTitlesManager.Models
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
        /// Determines whether the specified <see cref="object"/> is equal to the current <see cref="EntityBase"/>.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare with the current <see cref="EntityBase"/>.</param>
        /// <returns><c>true</c> if the specified <see cref="object"/> is equal to the current
        /// <see cref="EntityBase"/>; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            EntityBase other = obj as EntityBase;
            return Id == other?.Id;
        }

        /// <summary>
        /// Serves as a hash function for a <see cref="EntityBase"/> object.
        /// </summary>
        /// <returns>A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a
        /// hash table.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents the current <see cref="EntityBase"/>.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents the current <see cref="EntityBase"/>.</returns>
        public override string ToString()
        {
            return string.Format("{0} #{1}", base.ToString(), Id);
        }
    }
}
