using System.Collections.Generic;
using System.Linq;

using CK2LandedTitlesExtractor.Entities;
using CK2LandedTitlesExtractor.Exceptions;

namespace CK2LandedTitlesExtractor.Repositories
{
    /// <summary>
    /// Memory Repository.
    /// </summary>
    public class Repository<T> where T : EntityBase
    {
        /// <summary>
        /// Gets or sets the entities.
        /// </summary>
        /// <value>The entities.</value>
        protected Dictionary<int, T> Entities { get; set; }

        /// <summary>
        /// Gets the number of entities.
        /// </summary>
        /// <value>The number of entities.</value>
        public int Size
        {
            get { return Entities.Count; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CK2LandedTitlesExtractor.Repositories.Repository"/> class.
        /// </summary>
        public Repository()
        {
            Entities = new Dictionary<int, T>();
        }

        /// <summary>
        /// Add the specified entity.
        /// </summary>
        /// <param name="entity">Entity.</param>
        public virtual void Add(T entity)
        {
            if (Contains(entity.Id))
                throw new DuplicateEntityException();

            Entities.Add(entity.Id, entity);
        }

        /// <summary>
        /// Get the entity with the specified identifier.
        /// </summary>
        /// <param name="id">Identifier.</param>
        public virtual T Get(int id)
        {
            if (!Contains(id))
                throw new EntityNotFoundException();

            return Entities[id];
        }

        /// <summary>
        /// Gets all entities.
        /// </summary>
        /// <returns>The all.</returns>
        public List<T> GetAll()
        {
            return Entities.Values.ToList();
        }

        /// <summary>
        /// Remove the specified entity.
        /// </summary>
        /// <param name="entity">Entity.</param>
        public virtual void Remove(T entity)
        {
            Remove(entity.Id);
        }

        /// <summary>
        /// Remove the entity with the specified identifier.
        /// </summary>
        /// <param name="id">Identifier.</param>
        public virtual void Remove(int id)
        {
            if (!Contains(id))
                throw new EntityNotFoundException();

            Entities.Remove(id);
        }

        /// <summary>
        /// Checks wether the specified entity is contained.
        /// </summary>
        /// <param name="entity">Entity.</param>
        public bool Contains(T entity)
        {
            return Entities.ContainsValue(entity);
        }

        /// <summary>
        /// Checks wether the specified entity is contained.
        /// </summary>
        /// <param name="id">Identifier.</param>
        public bool Contains(int id)
        {
            return Entities.ContainsKey(id);
        }

        /// <summary>
        /// Empties the repository.
        /// </summary>
        public void Clear()
        {
            Entities.Clear();
        }
    }
}
