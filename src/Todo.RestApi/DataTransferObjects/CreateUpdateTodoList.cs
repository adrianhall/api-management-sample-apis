// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

namespace Todo.RestApi.DataTransferObjects;

public record CreateUpdateTodoList(
    string Name,
    string? Description = null
);