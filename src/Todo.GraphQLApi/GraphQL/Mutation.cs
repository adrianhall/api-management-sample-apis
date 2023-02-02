// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using HotChocolate.Resolvers;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using Todo.Data;

namespace Todo.GraphQLApi.GraphQL;

[MutationType]
public static class Mutation
{
    [UseMutationConvention(PayloadFieldName = "success")]
    public static async Task<bool> DeleteTodoItemAsync(TodoDbContext context, [ID] Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await context.TodoItems.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity != null)
        {
            context.TodoItems.Remove(entity);
            await context.SaveChangesAsync(cancellationToken);
            return true;
        }
        return false;
    }

    [UseMutationConvention(PayloadFieldName = "success")]
    public static async Task<bool> DeleteTodoListAsync(TodoDbContext context, [ID] Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await context.TodoLists.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity != null)
        {
            context.TodoLists.Remove(entity);
            await context.SaveChangesAsync(cancellationToken);
            return true;
        }
        return false;
    }

    [Error(typeof(ListDoesNotExistException))]
    public static async Task<TodoItem> SaveTodoItemAsync(TodoDbContext context, SaveTodoItemInput input, CancellationToken cancellationToken = default)
    {
        TodoItem? existingitem = input.Id != null ? await context.TodoItems.SingleOrDefaultAsync(x => x.Id == input.Id, cancellationToken) : null;
        TodoList? existinglist = input.ListId != Guid.Empty ? await context.TodoLists.SingleOrDefaultAsync(x => x.Id == input.ListId, cancellationToken) : null;
        if (existinglist == null)
        {
            throw new ListDoesNotExistException($"List with ID {input.ListId} does not exist.");
        }

        if (existingitem == null)
        {
            TodoItem newitem = new(input.ListId, input.Name)
            {
                Id = input.Id,
                Name = input.Name,
                Description = input.Description,
                DueDate = input.DueDate,
                CompletedDate = input.CompletedDate,
                UpdatedDate = DateTimeOffset.UtcNow
            };
            var newEntity = context.TodoItems.Add(newitem);
            await context.SaveChangesAsync(cancellationToken);
            return newEntity.Entity;
        }

        existingitem.Name = input.Name;
        existingitem.ListId = input.ListId;
        existingitem.Description = input.Description;
        existingitem.DueDate = input.DueDate;
        existingitem.CompletedDate = input.CompletedDate;
        existingitem.UpdatedDate = DateTimeOffset.UtcNow;
        var replacedEntity = context.TodoItems.Update(existingitem);
        await context.SaveChangesAsync(cancellationToken);
        return replacedEntity.Entity;
    }

    public static async Task<TodoList> SaveTodoListAsync(TodoDbContext context, SaveTodoListInput input, CancellationToken cancellationToken = default)
    {
        TodoList? existinglist = input.Id != null ? await context.TodoLists.SingleOrDefaultAsync(x => x.Id == input.Id, cancellationToken) : null;
        if (existinglist == null)
        {
            TodoList newlist = new(input.Name)
            {
                Id = input.Id,
                Description = input.Description,
                UpdatedDate = DateTimeOffset.UtcNow
            };
            var newEntity = context.TodoLists.Add(newlist);
            await context.SaveChangesAsync(cancellationToken);
            return newEntity.Entity;
        }

        existinglist.Name = input.Name;
        existinglist.Description = input.Description;
        existinglist.UpdatedDate = DateTimeOffset.UtcNow;
        var replacedEntity = context.TodoLists.Update(existinglist);
        await context.SaveChangesAsync(cancellationToken);
        return replacedEntity.Entity;
    }
}
