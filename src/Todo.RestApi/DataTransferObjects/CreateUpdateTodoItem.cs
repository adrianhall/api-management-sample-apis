// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Todo.Data.Models;

namespace Todo.RestApi.DataTransferObjects;

public record CreateUpdateTodoItem(
    string Name,
    TodoItemState State,
    DateTimeOffset? DueDate,
    DateTimeOffset? CompletedDate,
    string? Description = null
);
