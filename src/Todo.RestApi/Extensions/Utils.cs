// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.EntityFrameworkCore;
using Todo.Data;
using Todo.RestApi.DataTransferObjects;

namespace Todo.RestApi.Extensions;

public static class Utils
{
    private const int DefaultBatchSize = 20;
    private const int MaxBatchSize = 100;

    /// <summary>
    /// Gets the appropriate batch size.
    /// </summary>
    /// <param name="batchSize">The customer provided batch size.</param>
    /// <returns>The actual batch size to use.</returns>
    public static int GetBatchSize(int? batchSize)
    {
        if (batchSize == null || batchSize < 1)
        {
            return DefaultBatchSize;
        }
        else if (batchSize < MaxBatchSize)
        {
            return (int)batchSize;
        }
        else
        {
            return MaxBatchSize;
        }
    }

    /// <summary>
    /// Determine if skip and top are valid.
    /// </summary>
    /// <param name="skip"></param>
    /// <param name="batchSize"></param>
    /// <returns></returns>
    public static bool ValidateSkipTop(int? skip, int? batchSize)
    {
        if (skip.HasValue && skip.Value < 0)
        {
            return false;
        }
        if (batchSize.HasValue && (batchSize.Value < 1 || batchSize > MaxBatchSize))
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// Returns a paged response.
    /// </summary>
    /// <typeparam name="T">The type of item in the response.</typeparam>
    /// <param name="queryable">The queryable to obtain the list of items.</param>
    /// <param name="skip">The $skip value.</param>
    /// <param name="batchSize">The $top value.</param>
    /// <param name="baseUri">The base URI of the request.</param>
    /// <returns></returns>
    public static Page<T> PagedResponse<T>(DbSet<T> dbset, int? skip, int? batchSize, string baseUri) where T : TodoBaseModel
    {
        int totalCount = dbset.AsQueryable().Count();
        int skipValue = skip ?? 0;
        var items = dbset.AsQueryable().OrderBy(item => item.CreatedDate).Skip(skipValue).Take(GetBatchSize(batchSize)).ToList();
        bool hasMoreitems = skipValue + items.Count < totalCount;
        Uri? nextLink = !hasMoreitems ? null : new Uri($"{baseUri}?$skip={skipValue + items.Count}&$top={GetBatchSize(batchSize)}");
        return new Page<T>
        {
            Items = items,
            HasMoreItems = hasMoreitems,
            NextLink = nextLink
        };
    }

    /// <summary>
    /// Converts a State string into a TodoItemState.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="state"></param>
    /// <returns></returns>
    public static bool ParseState(string input, out TodoItemState state)
    {
        state = TodoItemState.Todo;
        if (input.Equals("todo", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        if (input.Equals("inprogress", StringComparison.OrdinalIgnoreCase))
        {
            state = TodoItemState.InProgress;
            return true;
        }
        if (input.Equals("in_progress", StringComparison.OrdinalIgnoreCase))
        {
            state = TodoItemState.InProgress;
            return true;
        }
        if (input.Equals("done", StringComparison.OrdinalIgnoreCase))
        {
            state = TodoItemState.Done;
            return true;
        }
        return false;
    }
}
