using System;

namespace CK2LandedTitlesExtractor.Exceptions
{
    /// <summary>
    /// Repository exception.
    /// </summary>
    public class EntityNotFoundException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CK2LandedTitlesExtractor.Exceptions.EntityNotFoundException"/> class.
        /// </summary>
        public EntityNotFoundException()
            :base("The specified entity does not exist")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CK2LandedTitlesExtractor.Exceptions.EntityNotFoundException"/> class.
        /// </summary>
        /// <param name="message">Message.</param>
        public EntityNotFoundException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CK2LandedTitlesExtractor.Exceptions.EntityNotFoundException"/> class.
        /// </summary>
        /// <param name="message">Message.</param>
        /// <param name="innerException">Inner exception.</param>
        public EntityNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
