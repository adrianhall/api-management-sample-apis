// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Serialization.HybridRow.Layouts;
using System.Web;
using Todo.Data;
using Todo.Data.Models;
using Todo.RestApi.DataTransferObjects;
using Todo.RestApi.Extensions;

namespace Todo.RestApi.Controllers;

[ApiController]
[Route("/todo/lists")]
public class ListsController : ControllerBase
{
    private readonly ITodoRepository<TodoItem> itemsRepository;
    private readonly ITodoRepository<TodoList> listsRepository;
    private readonly IHttpContextAccessor contextAccessor;

    public ListsController(ITodoRepository<TodoList> listsRepository, ITodoRepository<TodoItem> itemsRepository, IHttpContextAccessor contextAccessor)
    {
        this.itemsRepository = itemsRepository;
        this.listsRepository = listsRepository;
        this.contextAccessor = contextAccessor;
    }

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

    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(200, Type = typeof(Page<TodoList>))]
    public ActionResult<Page<TodoList>> GetLists([FromQuery(Name = "$skip")] int? skip = null, [FromQuery(Name = "$top")] int? batchSize = null)
        => Ok(Utils.PagedResponse(listsRepository.AsQueryable(), skip, batchSize, GetBaseUri()));

    [HttpPost]
    [Produces("application/json")]
    [ProducesResponseType(201, Type = typeof(TodoList))]
    public async Task<ActionResult> CreateListAsync([FromBody] CreateUpdateTodoList list, CancellationToken cancellationToken = default)
    {
        var newlist = new TodoList(list.Name) { Description = list.Description };
        var savedlist = await listsRepository.SaveEntityAsync(newlist, cancellationToken);
        return CreatedAtAction(nameof(GetListAsync), new { id = savedlist.Id }, savedlist);
    }

    [HttpGet("{id}")]
    [Produces("application/json")]
    [ProducesResponseType(200, Type = typeof(TodoList))]
    [ProducesResponseType(404)]
    [ActionName(nameof(GetListAsync))]
    public async Task<ActionResult> GetListAsync([FromRoute] string id, CancellationToken cancellationToken = default)
    {
        var list = await listsRepository.GetEntityAsync(id, cancellationToken);
        return list == null ? NotFound() : Ok(list);
    }

    [HttpPut("{id}")]
    [Produces("application/json")]
    [ProducesResponseType(200, Type = typeof(TodoList))]
    [ProducesResponseType(404)]
    public async Task<ActionResult> ReplaceListAsync([FromRoute] string id, [FromBody] CreateUpdateTodoList list, CancellationToken cancellationToken = default)
    {
        var existinglist = await listsRepository.GetEntityAsync(id, cancellationToken);
        if (existinglist == null)
        {
            return NotFound();
        }

        existinglist.Name = list.Name;
        existinglist.Description = list.Description;

        var savedlist = await listsRepository.SaveEntityAsync(existinglist, cancellationToken);
        return Ok(savedlist);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<ActionResult> DeleteListAsync([FromRoute] string id, CancellationToken cancellationToken = default)
    {
        var existinglist = await listsRepository.GetEntityAsync(id, cancellationToken);
        if (existinglist == null)
        {
            return NotFound();
        }

        // Step 1 - find all items that are in this list, and delete them.
        var itemsInList = itemsRepository.AsQueryable().Where(item => item.ListId == existinglist.Id).ToList();
        foreach (var item in itemsInList)
        {
            await itemsRepository.DeleteEntityAsync(item.Id!, cancellationToken);
        }

        // Step 2 - delete the list.
        await listsRepository.DeleteEntityAsync(id, cancellationToken);

        // Return 204 No Content
        return NoContent();
    }

    [HttpGet("{id}/items")]
    [Produces("application/json")]
    [ProducesResponseType(200, Type = typeof(Page<TodoItem>))]
    [ProducesResponseType(404)]
    public async Task<ActionResult<Page<TodoItem>>> GetListItemsAsync([FromRoute] string id, [FromQuery(Name = "$skip")] int? skip = null, [FromQuery(Name = "$top")] int? batchSize = null, CancellationToken cancellationToken = default)
    {
        var list = await listsRepository.GetEntityAsync(id, cancellationToken);
        if (list == null)
        {
            return NotFound();
        }
        return Ok(Utils.PagedResponse<TodoItem>(itemsRepository.AsQueryable().Where(item => item.ListId == id), skip, batchSize, GetBaseUri()));
    }

    [HttpPost("{id}/items")]
    [Produces("application/json")]
    [ProducesResponseType(201, Type = typeof(TodoItem))]
    [ProducesResponseType(404)]
    public async Task<ActionResult<TodoItem>> CreateListItemAsync([FromRoute] string id, [FromBody] CreateUpdateTodoItem item, CancellationToken cancellationToken = default)
    {
        var list = await listsRepository.GetEntityAsync(id, cancellationToken);
        if (list == null)
        {
            return NotFound();
        }

        var newitem = new TodoItem(id, item.Name) { Description = item.Description };
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

        var saveditem = await itemsRepository.SaveEntityAsync(newitem, cancellationToken);
        return CreatedAtAction(nameof(GetListItemAsync), new { id, item_id = saveditem.Id }, saveditem);
    }

    [HttpGet("{id}/items/{item_id}")]
    [Produces("application/json")]
    [ProducesResponseType(200, Type = typeof(TodoItem))]
    [ProducesResponseType(404)]
    [ActionName(nameof(GetListItemAsync))]
    public async Task<ActionResult<TodoItem>> GetListItemAsync([FromRoute] string id, [FromRoute] string item_id, CancellationToken cancellationToken = default)
    {
        var list = await listsRepository.GetEntityAsync(id, cancellationToken);
        if (list == null)
        {
            return NotFound();
        }
        var item = await itemsRepository.GetEntityAsync(item_id, cancellationToken);
        return (item == null || item.ListId != id) ? NotFound() : Ok(item);
    }

    [HttpPut("{id}/items/{item_id}")]
    [Produces("application/json")]
    [ProducesResponseType(200, Type = typeof(TodoItem))]
    [ProducesResponseType(404)]
    public async Task<ActionResult<TodoItem>> UpdateListItemAsync([FromRoute] string id, [FromRoute] string item_id, [FromBody] CreateUpdateTodoItem item, CancellationToken cancellationToken = default)
    {
        var existingitem = await itemsRepository.GetEntityAsync(item_id, cancellationToken);
        if (existingitem == null || existingitem.ListId != id)
        {
            return NotFound();
        }

        existingitem.Name = item.Name;
        existingitem.Description = item.Description;
        existingitem.CompletedDate = item.CompletedDate;
        existingitem.DueDate = item.DueDate;
        if (!string.IsNullOrEmpty(item.State) && Utils.ParseState(item.State, out TodoItemState state))
        {
            existingitem.State = state;
        }
        else
        {
            return BadRequest();
        }

        var saveditem = await itemsRepository.SaveEntityAsync(existingitem, cancellationToken);
        return Ok(saveditem);
    }

    [HttpDelete("{id}/items/{item_id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<ActionResult> DeleteListItemAsync([FromRoute] string id, [FromRoute] string item_id, CancellationToken cancellationToken = default)
    {
        var existingitem = await itemsRepository.GetEntityAsync(item_id, cancellationToken);
        if (existingitem == null || existingitem.ListId != id)
        {
            return NotFound();
        }

        await itemsRepository.DeleteEntityAsync(item_id, cancellationToken);
        return NoContent();
    }

    [HttpGet("{id}/state/{state}")]
    [Produces("application/json")]
    [ProducesResponseType(200, Type = typeof(Page<TodoItem>))]
    [ProducesResponseType(404)]
    public async Task<ActionResult<Page<TodoItem>>> GetListItemsByStateAsync([FromRoute] string id, [FromRoute] string state, [FromQuery(Name = "$skip")] int? skip = null, [FromQuery(Name = "$top")] int? batchSize = null, CancellationToken cancellationToken = default)
    {
        if (!Utils.ParseState(state, out TodoItemState parsedState))
        {
            return BadRequest();
        }
        var list = await listsRepository.GetEntityAsync(id, cancellationToken);
        if (list == null)
        {
            return NotFound();
        }
        return Ok(Utils.PagedResponse<TodoItem>(itemsRepository.AsQueryable().Where(item => item.ListId == id && item.State == parsedState), skip, batchSize, GetBaseUri()));
    }
}
