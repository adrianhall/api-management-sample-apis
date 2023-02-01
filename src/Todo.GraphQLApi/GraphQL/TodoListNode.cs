// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Todo.Data;

namespace Todo.GraphQLApi.GraphQL;

[ExtendObjectType<TodoList>]
public class TodoListNode
{
    [UsePaging]
    [UseFiltering]
    [UseSorting]
    public static IQueryable<TodoItem> GetItems(TodoDbContext context, [Parent] TodoList list, TodoItemState? state)
    {
        return state != null
            ? context.TodoItems.Where(i => i.ListId == list.Id && i.State == state)
            : context.TodoItems.Where(i => i.ListId == list.Id);
    }
}
