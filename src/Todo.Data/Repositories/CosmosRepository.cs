// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Azure.Cosmos;
using System.Net;
using Todo.Data.Models;

using CosmosContainer = Microsoft.Azure.Cosmos.Container;
using CosmosDatabase = Microsoft.Azure.Cosmos.Database;

namespace Todo.Data.Repositories;

public class CosmosRepository<T> : ITodoRepository<T> where T : TodoBaseModel
{
    private readonly CosmosContainer container;
    private readonly ItemRequestOptions saveOptions = new()
    {
        EnableContentResponseOnWrite = true
    };

    /// <summary>
    /// Creates a new <see cref="CosmosRepository"/> using a <see cref="CosmosContainer"/> 
    /// with the same name was the type.
    /// </summary>
    /// <param name="database">The <see cref="CosmosDatabase"/> to use.</param>
    public CosmosRepository(CosmosDatabase database)
    {
        container = database.GetContainer(typeof(T).Name);
    }

    /// <summary>
    /// Creates a new <see cref="CosmosRepository"/> using a <see cref="CosmosContainer"/> 
    /// with the specified name.
    /// </summary>
    /// <param name="database">The <see cref="CosmosDatabase"/> to use.</param>
    public CosmosRepository(CosmosDatabase database, string containerName)
    {
        container = database.GetContainer(containerName);
    }

    #region ITodoRepository<T>
    /// <summary>
    /// Retrieves an unexecuted <see cref="IQueryable{T}"/> of all entities in the repository.
    /// </summary>
    /// <returns>An unexecuted <see cref="IQueryable{T}"/>.</returns>
    public IQueryable<T> AsQueryable()
        => container.GetItemLinqQueryable<T>();

    /// <summary>
    /// Deletes an entity from the repository.
    /// </summary>
    /// <param name="id">The ID of the entity to delete.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns><c>true</c> if the list was deleted, and <c>false</c> otherwise.</returns>
    public async Task<bool> DeleteEntityAsync(string id, CancellationToken cancellationToken = default)
    {
        var response = await container.DeleteItemAsync<T>(id, GetPartitionKey(id), cancellationToken: cancellationToken).ConfigureAwait(false);
        return IsSuccessful(response?.StatusCode);
    }

    /// <summary>
    /// Retrieves an entity from the repository.
    /// </summary>
    /// <param name="id">The ID of the entity to retrieve</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>The entity, or <c>null</c> if the entity does not exist.</returns>
    public async Task<T?> GetEntityAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await container.ReadItemAsync<T>(id, GetPartitionKey(id), cancellationToken: cancellationToken).ConfigureAwait(false);
            return response?.Resource;
        }
        catch (CosmosException e) when (e.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    /// <summary>
    /// Saves an entity to the repository, either overwriting the data that is present, or creating
    /// a new entity.
    /// </summary>
    /// <param name="entity">The entity data to be saved.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>The saved entity.</returns>
    public async Task<T> SaveEntityAsync(T entity, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(entity.Id))
        {
            entity.Id = Guid.NewGuid().ToString("N");
        }
        var existingEntity = await GetEntityAsync(entity.Id, cancellationToken).ConfigureAwait(false);
        entity.CreatedDate = existingEntity?.CreatedDate ?? DateTimeOffset.UtcNow;
        entity.UpdatedDate = DateTimeOffset.UtcNow;
        var response = await container.UpsertItemAsync(entity, GetPartitionKey(entity.Id), saveOptions, cancellationToken).ConfigureAwait(false);
        return response?.Resource ?? throw new RepositoryException($"Content from UpsertItemAsync for ID {entity.Id} was not returned");
    }
    #endregion

    /// <summary>
    /// Converts an ID into a partition key.
    /// </summary>
    /// <param name="id">The ID being presented.</param>
    /// <returns>The <see cref="PartitionKey"/> for the ID.</returns>
    private static PartitionKey GetPartitionKey(string id)
        => new(id);

    /// <summary>
    /// Determines if a request was actually successful.
    /// </summary>
    /// <param name="statusCode">The <see cref="HttpStatusCode"/> that was returned.</param>
    /// <returns><c>true</c> if the operation was successful, and <c>false</c> otherwise.</returns>
    private static bool IsSuccessful(HttpStatusCode? statusCode)
        => statusCode != null && (int)statusCode >= 200 && (int)statusCode <= 299;
}
