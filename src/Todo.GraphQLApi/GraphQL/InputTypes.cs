// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Todo.Data;

namespace Todo.GraphQLApi.GraphQL;

/// <summary>
/// The input type for saving a TodoList.
/// </summary>
public class SaveTodoListInput
{
    [ID]
    public Guid? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

/// <summary>
/// The input type for saving a TodoItem.
/// </summary>
public class SaveTodoItemInput
{
    [ID]
    public Guid? Id { get; set; }
    public Guid ListId { get; set; } = Guid.Empty;
    public string Name { get; set; } = string.Empty;
    public TodoItemState State { get; set; } = TodoItemState.Todo;
    public string? Description { get; set; }
    public DateTimeOffset? DueDate { get; set; }
    public DateTimeOffset? CompletedDate { get; set; }
}

