// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Todo.Data.Models;

namespace Todo.Data.Repositories;

public class InMemoryRepository<T> : ITodoRepository<T> where T : TodoBaseModel
{
    /// <summary>
    /// Internal storage for the contents of the repository.
    /// </summary>
    private readonly Dictionary<string, T> contents = new();

    #region ITodoRepository<T>
    /// <summary>
    /// Retrieves an unexecuted <see cref="IQueryable{T}"/> of all entities in the repository.
    /// </summary>
    /// <returns>An unexecuted <see cref="IQueryable{T}"/>.</returns>
    public IQueryable<T> AsQueryable()
        => contents.Values.AsQueryable();

    /// <summary>
    /// Deletes an entity from the repository.
    /// </summary>
    /// <param name="id">The ID of the entity to delete.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns><c>true</c> if the list was deleted, and <c>false</c> otherwise.</returns>
    public Task<bool> DeleteEntityAsync(string id, CancellationToken cancellationToken = default)
    {
        lock (contents)
        {
            if (contents.ContainsKey(id))
            {
                contents.Remove(id);
                return Task.FromResult(true);
            }
        }
        return Task.FromResult(false);
    }

    /// <summary>
    /// Retrieves an entity from the repository.
    /// </summary>
    /// <param name="id">The ID of the entity to retrieve</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>The entity, or <c>null</c> if the entity does not exist.</returns>
    public Task<T?> GetEntityAsync(string id, CancellationToken cancellationToken = default)
        => Task.FromResult(contents.ContainsKey(id) ? contents[id] : null);

    /// <summary>
    /// Saves an entity to the repository, either overwriting the data that is present, or creating
    /// a new entity.
    /// </summary>
    /// <param name="entity">The entity data to be saved.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>The saved entity.</returns>
    public Task<T> SaveEntityAsync(T entity, CancellationToken cancellationToken = default)
    {
        lock (contents)
        {
            if (string.IsNullOrWhiteSpace(entity.Id))
            {
                entity.Id = Guid.NewGuid().ToString("N");
            }
            var existingEntity = contents.ContainsKey(entity.Id) ? contents[entity.Id] : null;
            entity.CreatedDate = existingEntity?.CreatedDate ?? DateTimeOffset.UtcNow;
            entity.UpdatedDate = DateTimeOffset.UtcNow;
            contents[entity.Id] = entity;
        }
        return Task.FromResult(contents[entity.Id]);
    }
    #endregion
}
