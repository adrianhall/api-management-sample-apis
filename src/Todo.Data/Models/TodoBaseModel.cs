// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

namespace Todo.Data.Models;

public abstract class TodoBaseModel
{
    /// <summary>
    /// The ID of the item.
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// The date that the item was created.
    /// </summary>
    public DateTimeOffset CreatedDate { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// The date that the item was last updated.
    /// </summary>
    public DateTimeOffset? UpdatedDate { get; set; }
}
