param serviceUri string
param path string = 'todo-syngql'

param apiManagementServiceName string
param apiManagementLoggerName string = ''

var resolvers = [
  {
    name: 'query-todolists', type: 'Query', field: 'todolists'
    policy: loadTextContent('../../src/ApiManagement/TodoSynGQLApi/resolvers/query-todolists.xml')
  }
  {
    name: 'query-todolistbyid', type: 'Query', field: 'todolistbyid'
    policy: loadTextContent('../../src/ApiManagement/TodoSynGQLApi/resolvers/query-todolistbyid.xml')
  }
]

module graphqlApiDefinition '../core/gateway/synthetic-graphql-api.bicep' = {
  name: 'todo-syngql-api-definition'
  params: {
    name: 'todo-syngql'
    apimServiceName: apiManagementServiceName
    apimLoggerName: apiManagementLoggerName
    path: path
    policy: loadTextContent('../../src/ApiManagement/TodoGraphQLApi/policy.xml')
    schema: loadTextContent('../../src/ApiManagement/TodoGraphQLApi/schema.graphql')
    namedValues: [
      { name: 'todoapi', value: serviceUri }
    ]
    resolvers: resolvers
  }
}

output gatewayUri string = graphqlApiDefinition.outputs.serviceUrl
