// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.EntityFrameworkCore;
using Todo.Data;

namespace Todo.GraphQLApi.GraphQL;

[QueryType]
public static class Query
{
    [NodeResolver]
    public static Task<TodoItem?> GetTodoItemByIdAsync(TodoDbContext context, Guid id, CancellationToken cancellationToken = default)
        => context.TodoItems.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    [NodeResolver]
    public static Task<TodoList?> GetTodoListByIdAsync(TodoDbContext context, Guid id, CancellationToken cancellationToken = default)
        => context.TodoLists.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    [UsePaging]
    [UseFiltering]
    [UseSorting]
    public static IQueryable<TodoList> GetTodoLists(TodoDbContext context)
        => context.TodoLists.AsQueryable();
}
