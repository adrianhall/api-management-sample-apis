// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

namespace Todo.Data.Models;

public enum TodoItemState
{
    Todo,
    InProgress,
    Done
}

/// <summary>
/// The model for the TodoItem, as stored in the database.
/// </summary>
public class TodoItem : TodoBaseModel
{
    /// <summary>
    /// Creates a new <see cref="TodoItem"/>.
    /// </summary>
    /// <param name="listId">The ID of the list that this item is in.</param>
    /// <param name="name">The name or title of the item.</param>
    public TodoItem(string listId, string name)
    {
        ListId = listId;
        Name = name;
    }

    /// <summary>
    /// The ID of the list that this item is in.
    /// </summary>
    public string ListId { get; set; }

    /// <summary>
    /// The name or title of the item.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// A description of the item.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The current state.
    /// </summary>
    public TodoItemState State { get; set; } = TodoItemState.Todo;

    /// <summary>
    /// The date that the item is due.
    /// </summary>
    public DateTimeOffset? DueDate { get; set; }

    /// <summary>
    /// The date that the item was completed.
    /// </summary>
    public DateTimeOffset? CompletedDate { get; set; }
}