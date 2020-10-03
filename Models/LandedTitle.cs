using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CK2LandedTitlesManager.Models
{
    /// <summary>
    /// The title entity.
    /// </summary>
    public class LandedTitle
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        [Key]
        public string Id { get; set; }

        public string ParentId { get; set; }

        public IList<LandedTitle> Children { get; set; }

        /// <summary>
        /// Gets or sets the names.
        /// </summary>
        /// <value>The cultural names.</value>
        public IDictionary<string, string> Names { get; set; }

        public LandedTitle()
        {
            Children = new List<LandedTitle>();
            Names = new Dictionary<string, string>();
        }

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to the current <see cref="LandedTitle"/>.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare with the current <see cref="LandedTitle"/>.</param>
        /// <returns><c>true</c> if the specified <see cref="object"/> is equal to the current
        /// <see cref="LandedTitle"/>; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            if (obj.ToString() == ToString())
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
            return Id;
        }
    }
}
