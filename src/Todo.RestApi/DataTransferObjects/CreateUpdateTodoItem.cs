// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

namespace Todo.RestApi.DataTransferObjects;

public record CreateUpdateTodoItem(
    string Name,
    string? State,
    DateTimeOffset? DueDate,
    DateTimeOffset? CompletedDate,
    string? Description = null
);
