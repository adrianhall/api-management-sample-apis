

using HotChocolate.Types.Pagination;
using Todo.Data;

namespace Todo.GraphQLApi.GraphQL;

public static class GraphQLExtensions
{
    /// <summary>
    /// Adds the appropriate GraphQL Services to the collection.
    /// </summary>
    /// <param name="services"></param>
    public static void AddGraphQLService(this IServiceCollection services)
    {
        var pagingOptions = new PagingOptions { MaxPageSize = 100, DefaultPageSize = 50 };

        services.AddGraphQLServer()
            .RegisterDbContext<TodoDbContext>(DbContextKind.Resolver)
            .AddTypes()
            .AddMutationConventions()
            .AddGlobalObjectIdentification()
            .SetPagingOptions(pagingOptions)
            .AddFiltering()
            .AddSorting();

    }
}
