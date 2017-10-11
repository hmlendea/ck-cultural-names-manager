using System.Collections.Generic;

using CK2LandedTitlesManager.Models;
using CK2LandedTitlesManager.Exceptions;
using CK2LandedTitlesManager.Utils.Extensions;

namespace CK2LandedTitlesManager.Repositories
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
        protected Dictionary<int, T> DataStore { get; set; }

        /// <summary>
        /// Gets the size.
        /// </summary>
        /// <value>The size.</value>
        public int Size
        {
            get
            {
                if (DataStore.IsNullOrEmpty())
                {
                    return 0;
                }

                return DataStore.Count;
            }
        }

        /// <summary>
        /// Indicates wether the repository is empty.
        /// </summary>
        /// <value>True if the repository is empty, false otherwise.</value>
        public bool Empty
        {
            get { return DataStore.IsNullOrEmpty(); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Repository"/> class.
        /// </summary>
        public Repository()
        {
            DataStore = new Dictionary<int, T>();
        }

        /// <summary>
        /// Add the specified entity.
        /// </summary>
        /// <param name="entity">Entity.</param>
        public virtual void Add(T entity)
        {
            if (Contains(entity.Id))
            {
                throw new DuplicateEntityException();
            }

            DataStore.Add(entity.Id, entity);
        }

        /// <summary>
        /// Get the entity with the specified identifier.
        /// </summary>
        /// <param name="id">Identifier.</param>
        public virtual T Get(int id)
        {
            if (!Contains(id))
            {
                throw new EntityNotFoundException();
            }

            return DataStore[id];
        }

        /// <summary>
        /// Gets all entities.
        /// </summary>
        /// <returns>The all.</returns>
        public IEnumerable<T> GetAll()
        {
            return DataStore.Values;
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
            {
                throw new EntityNotFoundException();
            }

            DataStore.Remove(id);
        }

        /// <summary>
        /// Checks wether the specified entity is contained.
        /// </summary>
        /// <param name="entity">Entity.</param>
        public bool Contains(T entity)
        {
            return DataStore.ContainsValue(entity);
        }

        /// <summary>
        /// Checks wether the specified entity is contained.
        /// </summary>
        /// <param name="id">Identifier.</param>
        public bool Contains(int id)
        {
            return DataStore.ContainsKey(id);
        }

        /// <summary>
        /// Empties the repository.
        /// </summary>
        public void Clear()
        {
            DataStore.Clear();
        }
    }
}
