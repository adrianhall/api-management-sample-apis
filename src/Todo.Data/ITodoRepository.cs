// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Todo.Data.Models;

namespace Todo.Data;

/// <summary>
/// The definition of the repository to store data.
/// </summary>
public interface ITodoRepository<T> where T : TodoBaseModel
{
    /// <summary>
    /// Retrieves an unexecuted <see cref="IQueryable{T}"/> of all entities in the repository.
    /// </summary>
    /// <returns>An unexecuted <see cref="IQueryable{T}"/>.</returns>
    IQueryable<T> AsQueryable();

    /// <summary>
    /// Deletes an entity from the repository.
    /// </summary>
    /// <param name="id">The ID of the entity to delete.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns><c>true</c> if the list was deleted, and <c>false</c> otherwise.</returns>
    Task<bool> DeleteEntityAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves an entity from the repository.
    /// </summary>
    /// <param name="id">The ID of the entity to retrieve</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>The entity, or <c>null</c> if the entity does not exist.</returns>
    Task<T?> GetEntityAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves an entity to the repository, either overwriting the data that is present, or creating
    /// a new entity.
    /// </summary>
    /// <param name="entity">The entity data to be saved.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>The saved entity.</returns>
    Task<T> SaveEntityAsync(T entity, CancellationToken cancellationToken = default);
}
