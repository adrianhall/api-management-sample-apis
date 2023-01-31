// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using AutoMapper;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Web;
using Todo.Data;
using Todo.RestApi.DataTransferObjects;
using Todo.RestApi.Extensions;

namespace Todo.RestApi.Controllers;

[ApiController]
[Route("/todo/lists")]
public class ListsController : ControllerBase
{
    private readonly TodoDbContext dbContext;
    private readonly IMapper mapper;
    private readonly IHttpContextAccessor contextAccessor;

    public ListsController(TodoDbContext dbContext, IMapper mapper, IHttpContextAccessor contextAccessor)
    {
        this.dbContext = dbContext;
        this.mapper = mapper;
        this.contextAccessor = contextAccessor;
    }

    #region Helpers
    /// <summary>
    /// Gets this request, but without the $skip and $top parameters.
    /// </summary>
    /// <returns></returns>
    private string GetBaseUri()
    {
        var request = contextAccessor.HttpContext?.Request ?? Request;
        var baseUri = new UriBuilder(request.GetDisplayUrl());
        if (request.QueryString.HasValue)
        {
            var query = HttpUtility.ParseQueryString(request.QueryString.Value ?? string.Empty);
            query.Remove("$skip");
            query.Remove("$top");
            baseUri.Query = query.ToString();
        }
        return baseUri.ToString();
    }

    /// <summary>
    /// Gets a list by its ID.
    /// </summary>
    /// <param name="list_id">The ID of the list to retrieve.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>The list entity, or <c>null</c> if not found.</returns>
    private async ValueTask<TodoList?> GetListByIdAsync(string list_id, CancellationToken cancellationToken = default)
    {
        if (Guid.TryParse(list_id, out Guid list_guid))
        {
            return await dbContext.TodoLists.FirstOrDefaultAsync(m => m.Id == list_guid, cancellationToken);
        }
        return null;
    }

    /// <summary>
    /// Gets an item by its ID
    /// </summary>
    /// <param name="list_id">The ID of the list that contains the item.</param>
    /// <param name="item_id">The ID of the item to retrieve.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>The item entity, or <c>null</c> if not found.</returns>
    private async ValueTask<TodoItem?> GetListItemByIdAsync(string list_id, string item_id, CancellationToken cancellationToken = default)
    {
        if (Guid.TryParse(list_id, out Guid list_guid)  && Guid.TryParse(item_id, out Guid item_guid))
        {
            var item = await dbContext.TodoItems.FirstOrDefaultAsync(m => m.Id == item_guid, cancellationToken);
            return item?.ListId == list_guid ? item : null;
        }
        return null;
    }
    #endregion

    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(200, Type = typeof(Page<TodoListDto>))]
    public async Task<ActionResult<Page<TodoList>>> GetListsAsync(
        [FromQuery(Name = "$skip")] int? skip = null, 
        [FromQuery(Name = "$top")] int? batchSize = null,
        CancellationToken cancellationToken = default)
    {
        if (!Utils.ValidateSkipTop(skip, batchSize))
        {
            return BadRequest();
        }
        int totalCount = await dbContext.TodoLists.CountAsync(cancellationToken);
        int skipValue = skip ?? 0;
        var items = await dbContext.TodoLists
            .OrderBy(item => item.CreatedDate)
            .Skip(skipValue).Take(Utils.GetBatchSize(batchSize))
            .ToListAsync(cancellationToken);
        bool hasMoreitems = skipValue + items.Count < totalCount;
        var page = new Page<TodoListDto>
        {
            Items = mapper.Map<List<TodoListDto>>(items),
            HasMoreItems = hasMoreitems,
            NextLink = !hasMoreitems ? null : new Uri($"{GetBaseUri()}?$skip={skipValue + items.Count}&$top={Utils.GetBatchSize(batchSize)}")
        };
        return Ok(page);
    } 

    [HttpPost]
    [Produces("application/json")]
    [ProducesResponseType(201, Type = typeof(TodoListDto))]
    public async Task<ActionResult> CreateListAsync(
        [FromBody] CreateUpdateTodoList list, 
        CancellationToken cancellationToken = default)
    {
        var newlist = new TodoList(list.Name) 
        { 
            Description = list.Description,
            UpdatedDate = DateTimeOffset.UtcNow
        };
        var entity = dbContext.TodoLists.Add(newlist);
        await dbContext.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetListAsync), new { list_id = entity.Entity.Id }, mapper.Map<TodoListDto>(entity.Entity));
    }

    [HttpGet("{list_id}")]
    [Produces("application/json")]
    [ProducesResponseType(200, Type = typeof(TodoListDto))]
    [ProducesResponseType(404)]
    [ActionName(nameof(GetListAsync))]
    public async Task<ActionResult> GetListAsync(
        [FromRoute] string list_id, 
        CancellationToken cancellationToken = default)
    {
        var list = await GetListByIdAsync(list_id, cancellationToken);
        return list == null ? NotFound() : Ok(mapper.Map<TodoListDto>(list));
    }

    [HttpPut("{list_id}")]
    [Produces("application/json")]
    [ProducesResponseType(200, Type = typeof(TodoListDto))]
    [ProducesResponseType(404)]
    public async Task<ActionResult> ReplaceListAsync(
        [FromRoute] string list_id, 
        [FromBody] CreateUpdateTodoList list, 
        CancellationToken cancellationToken = default)
    {
        var existinglist = await GetListByIdAsync(list_id, cancellationToken);
        if (existinglist == null)
        {
            return NotFound();
        }

        existinglist.Name = list.Name;
        existinglist.Description = list.Description;
        existinglist.UpdatedDate = DateTimeOffset.UtcNow;

        var entity = dbContext.TodoLists.Update(existinglist);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(mapper.Map<TodoListDto>(entity.Entity));
    }

    [HttpDelete("{list_id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<ActionResult> DeleteListAsync(
        [FromRoute] string list_id, 
        CancellationToken cancellationToken = default)
    {
        var existinglist = await GetListByIdAsync(list_id, cancellationToken);
        if (existinglist == null)
        {
            return NotFound();
        }
        dbContext.TodoLists.Remove(existinglist);
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpGet("{list_id}/items")]
    [Produces("application/json")]
    [ProducesResponseType(200, Type = typeof(Page<TodoItemDto>))]
    [ProducesResponseType(404)]
    public async Task<ActionResult<Page<TodoItemDto>>> GetListItemsAsync(
        [FromRoute] string list_id, 
        [FromQuery(Name = "$skip")] int? skip = null, 
        [FromQuery(Name = "$top")] int? batchSize = null, 
        CancellationToken cancellationToken = default)
    {
        if (!Utils.ValidateSkipTop(skip, batchSize))
        {
            return BadRequest();
        }

        var list = await GetListByIdAsync(list_id, cancellationToken);
        if (list == null)
        {
            return NotFound();
        }

        int totalCount = await dbContext.TodoItems.CountAsync(cancellationToken);
        int skipValue = skip ?? 0;
        var items = await dbContext.TodoItems
            .OrderBy(item => item.CreatedDate)
            .Skip(skipValue).Take(Utils.GetBatchSize(batchSize))
            .ToListAsync(cancellationToken);
        bool hasMoreItems = skipValue + items.Count < totalCount;
        var page = new Page<TodoItemDto>
        {
            Items = mapper.Map<List<TodoItemDto>>(items),
            HasMoreItems = hasMoreItems,
            NextLink = !hasMoreItems ? null : new Uri($"{GetBaseUri()}?$skip={skipValue + items.Count}&$top={Utils.GetBatchSize(batchSize)}")
        };
        return Ok(page);
    }

    [HttpPost("{list_id}/items")]
    [Produces("application/json")]
    [ProducesResponseType(201, Type = typeof(TodoItemDto))]
    [ProducesResponseType(404)]
    public async Task<ActionResult<TodoItemDto>> CreateListItemAsync(
        [FromRoute] string list_id, 
        [FromBody] CreateUpdateTodoItem item, 
        CancellationToken cancellationToken = default)
    {
        var list = await GetListByIdAsync(list_id, cancellationToken);
        if (list == null)
        {
            return NotFound();
        }

        var newitem = new TodoItem(list.Id!.Value, item.Name) 
        { 
            Description = item.Description,
            DueDate = item.DueDate,
            CompletedDate = item.CompletedDate,
            UpdatedDate = DateTimeOffset.UtcNow
        };
        if (string.IsNullOrEmpty(item.State))
        {
            newitem.State = TodoItemState.Todo;
        }
        else if (Utils.ParseState(item.State, out TodoItemState state))
        {
            newitem.State = state;
        }
        else
        {
            return BadRequest();
        }

        list.UpdatedDate = DateTimeOffset.UtcNow;
        dbContext.TodoLists.Update(list);

        var entity = dbContext.TodoItems.Add(newitem);
        await dbContext.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetListItemAsync), new { list_id = entity.Entity.List!.Id, item_id = entity.Entity.Id }, mapper.Map<TodoItemDto>(entity.Entity));
    }

    [HttpGet("{list_id}/items/{item_id}")]
    [Produces("application/json")]
    [ProducesResponseType(200, Type = typeof(TodoItemDto))]
    [ProducesResponseType(404)]
    [ActionName(nameof(GetListItemAsync))]
    public async Task<ActionResult<TodoItemDto>> GetListItemAsync(
        [FromRoute] string list_id, 
        [FromRoute] string item_id, 
        CancellationToken cancellationToken = default)
    {
        var item = await GetListItemByIdAsync(list_id, item_id, cancellationToken);
        return (item == null) ? NotFound() : Ok(mapper.Map<TodoItemDto>(item));
    }

    [HttpPut("{list_id}/items/{item_id}")]
    [Produces("application/json")]
    [ProducesResponseType(200, Type = typeof(TodoItemDto))]
    [ProducesResponseType(404)]
    public async Task<ActionResult<TodoItemDto>> UpdateListItemAsync(
        [FromRoute] string list_id, 
        [FromRoute] string item_id, 
        [FromBody] CreateUpdateTodoItem item, 
        CancellationToken cancellationToken = default)
    {
        var list = await GetListByIdAsync(list_id, cancellationToken);
        if (list == null)
        {
            return NotFound();
        }

        var existingitem = await GetListItemByIdAsync(list_id, item_id, cancellationToken);
        if (existingitem == null)
        {
            return NotFound();
        }

        existingitem.Name = item.Name;
        existingitem.Description = item.Description;
        existingitem.CompletedDate = item.CompletedDate;
        existingitem.DueDate = item.DueDate;
        existingitem.UpdatedDate = DateTimeOffset.UtcNow;
        if (!string.IsNullOrEmpty(item.State) && Utils.ParseState(item.State, out TodoItemState state))
        {
            existingitem.State = state;
        }
        else
        {
            return BadRequest();
        }

        list.UpdatedDate = DateTimeOffset.UtcNow;
        dbContext.TodoLists.Update(list);

        var entity = dbContext.TodoItems.Update(existingitem);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(mapper.Map<TodoItemDto>(entity.Entity));
    }

    [HttpDelete("{list_id}/items/{item_id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<ActionResult> DeleteListItemAsync(
        [FromRoute] string list_id, 
        [FromRoute] string item_id, 
        CancellationToken cancellationToken = default)
    {
        var list = await GetListByIdAsync(list_id, cancellationToken);
        if (list == null)
        {
            return NotFound();
        }

        var existingitem = await GetListItemByIdAsync(list_id, item_id, cancellationToken);
        if (existingitem == null)
        {
            return NotFound();
        }
        
        list.UpdatedDate = DateTimeOffset.UtcNow;
        dbContext.TodoLists.Update(list);

        dbContext.TodoItems.Remove(existingitem);
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpGet("{list_id}/state/{state}")]
    [Produces("application/json")]
    [ProducesResponseType(200, Type = typeof(Page<TodoItemDto>))]
    [ProducesResponseType(404)]
    public async Task<ActionResult<Page<TodoItemDto>>> GetListItemsByStateAsync(
        [FromRoute] string list_id, 
        [FromRoute] string state, 
        [FromQuery(Name = "$skip")] int? skip = null, 
        [FromQuery(Name = "$top")] int? batchSize = null, 
        CancellationToken cancellationToken = default)
    {
        if (!Utils.ParseState(state, out TodoItemState parsedState))
        {
            return BadRequest();
        }

        var list = await GetListByIdAsync(list_id, cancellationToken);
        if (list == null)
        {
            return NotFound();
        }

        int totalCount = await dbContext.TodoItems.CountAsync(cancellationToken);
        int skipValue = skip ?? 0;
        var items = await dbContext.TodoItems
            .Where(item => item.State == parsedState)
            .OrderBy(item => item.CreatedDate)
            .Skip(skipValue).Take(Utils.GetBatchSize(batchSize))
            .ToListAsync(cancellationToken);
        bool hasMoreitems = skipValue + items.Count < totalCount;
        var page = new Page<TodoItemDto>
        {
            Items = mapper.Map<List<TodoItemDto>>(items),
            HasMoreItems = hasMoreitems,
            NextLink = !hasMoreitems ? null : new Uri($"{GetBaseUri()}?$skip={skipValue + items.Count}&$top={Utils.GetBatchSize(batchSize)}")
        };
        return Ok(page);
    }
}
