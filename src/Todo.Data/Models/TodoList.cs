// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

namespace Todo.Data.Models;

/// <summary>
/// The model for the TodoList, as stored in the database.
/// </summary>
public class TodoList : TodoBaseModel
{
    /// <summary>
    /// Create a new <see cref="TodoList"/>
    /// </summary>
    /// <param name="name">The name of the list.</param>
    public TodoList(string name)
    {
        Name = name;
    }

    /// <summary>
    /// The name of the list.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// A description of the list.
    /// </summary>
    public string? Description { get; set; }
}
