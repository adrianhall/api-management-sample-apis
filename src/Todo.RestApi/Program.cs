// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Azure.Identity;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Azure.Cosmos;
using System.Text.Json;
using System.Text.Json.Serialization;
using Todo.Data;
using Todo.Data.Models;
using Todo.Data.Repositories;
using Todo.RestApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

/******************************************************************************************
**
** Add services to the container
*/

/*
** CORS
*/
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin();
        policy.AllowAnyHeader();
        policy.AllowAnyMethod();
    });
});

/*
** Cosmos Client
*/
var cosmosEndpoint = builder.Configuration["AZURE_COSMOS_ENDPOINT"];
if (!string.IsNullOrEmpty(cosmosEndpoint))
{
    var credential = new ChainedTokenCredential(new AzureDeveloperCliCredential(), new DefaultAzureCredential());
    builder.Services.AddSingleton(_ => new CosmosClient(builder.Configuration["AZURE_COSMOS_ENDPOINT"], credential, new CosmosClientOptions()
    {
        EnableContentResponseOnWrite = true,
        SerializerOptions = new CosmosSerializationOptions
        {
            PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
        }
    }));
}


/*
** Repositories.
*/
if (!string.IsNullOrEmpty(cosmosEndpoint))
{
    builder.Services.AddSingleton<ITodoRepository<TodoItem>, CosmosRepository<TodoItem>>();
    builder.Services.AddSingleton<ITodoRepository<TodoList>, CosmosRepository<TodoList>>();
}
else
{
    builder.Services.AddSingleton<ITodoRepository<TodoItem>, InMemoryRepository<TodoItem>>();
    builder.Services.AddSingleton<ITodoRepository<TodoList>, InMemoryRepository<TodoList>>();
}

/*
** Controllers.
*/
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.WriteIndented = true;
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, true));
});

/*
** Swagger / OpenAPI.
*/
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

/******************************************************************************************
**
** Configure the HTTP Pipeline.
*/
var app = builder.Build();

/*
** CORS
*/
app.UseCors();

/*
** Add Swagger support.
*/
app.UseSwagger();
app.UseSwaggerUI();

/*
** Controllers.
*/
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

/************************************************************************************************
**
** Run the application.
*/
app.Run();
