param serviceUri string
param path string = 'starwars-graphql'

param apiManagementServiceName string
param apiManagementLoggerName string = ''

var resolvers = [
  { 
    type: 'Query'
    field: 'characters' 
    policy: loadTextContent('../../src/ApiManagement/StarWarsGraphQLApi/resolvers/query-characters.xml') 
  }
  { 
    type: 'Query'
    field: 'films'
    policy: loadTextContent('../../src/ApiManagement/StarWarsGraphQLApi/resolvers/query-films.xml') 
  }
  { 
    type: 'Query'
    field: 'planets'
    policy: loadTextContent('../../src/ApiManagement/StarWarsGraphQLApi/resolvers/query-planets.xml') 
  }
  { 
    type: 'Query'
    field: 'species'
    policy: loadTextContent('../../src/ApiManagement/StarWarsGraphQLApi/resolvers/query-species.xml') 
  }
  { 
    type: 'Query'
    field: 'starships'
    policy: loadTextContent('../../src/ApiManagement/StarWarsGraphQLApi/resolvers/query-starships.xml') 
  }
  { 
    type: 'Query'
    field: 'vehicles'
    policy: loadTextContent('../../src/ApiManagement/StarWarsGraphQLApi/resolvers/query-vehicles.xml') 
  }
  { 
    type: 'Query'
    field: 'getCharacterById'
    policy: loadTextContent('../../src/ApiManagement/StarWarsGraphQLApi/resolvers/query-getCharacterById.xml') 
  }
  { 
    type: 'Query'
    field: 'getFilmById'
    policy: loadTextContent('../../src/ApiManagement/StarWarsGraphQLApi/resolvers/query-getFilmById.xml') 
  }
  { 
    type: 'Query'
    field: 'getPlanetById'
    policy: loadTextContent('../../src/ApiManagement/StarWarsGraphQLApi/resolvers/query-getPlanetById.xml') 
  }
  { 
    type: 'Query'
    field: 'getSpeciesById'
    policy: loadTextContent('../../src/ApiManagement/StarWarsGraphQLApi/resolvers/query-getSpeciesById.xml') 
  }
  { 
    type: 'Query'
    field: 'getStarshipById'
    policy: loadTextContent('../../src/ApiManagement/StarWarsGraphQLApi/resolvers/query-getStarshipById.xml') 
  }
  { 
    type: 'Query'
    field: 'getVehicleById'
    policy: loadTextContent('../../src/ApiManagement/StarWarsGraphQLApi/resolvers/query-getVehicleById.xml') 
  }
  { 
    type: 'Film'
    field: '*'
    policy: loadTextContent('../../src/ApiManagement/StarWarsGraphQLApi/resolvers/film-all.xml') 
  }
  { 
    type: 'Person'
    field: '*'
    policy: loadTextContent('../../src/ApiManagement/StarWarsGraphQLApi/resolvers/person-all.xml') 
  }
  { 
    type: 'Planet'
    field: '*'
    policy: loadTextContent('../../src/ApiManagement/StarWarsGraphQLApi/resolvers/planet-all.xml') 
  }
  { 
    type: 'Species'
    field: '*'
    policy: loadTextContent('../../src/ApiManagement/StarWarsGraphQLApi/resolvers/species-all.xml') 
  }
  { 
    type: 'Starship'
    field: '*'
    policy: loadTextContent('../../src/ApiManagement/StarWarsGraphQLApi/resolvers/starship-all.xml') 
  }
  { 
    type: 'Vehicle'
    field: '*'
    policy: loadTextContent('../../src/ApiManagement/StarWarsGraphQLApi/resolvers/vehicle-all.xml') 
  }
]

module graphqlApiDefinition '../core/gateway/synthetic-graphql-api.bicep' = {
  name: 'starwars-graphql-api-definition'
  params: {
    name: 'starwars-graphql'
    apimServiceName: apiManagementServiceName
    apimLoggerName: apiManagementLoggerName
    path: path
    policy: loadTextContent('../../src/ApiManagement/TodoGraphQLApi/policy.xml')
    schema: loadTextContent('../../src/ApiManagement/TodoGraphQLApi/schema.graphql')
    namedValues: [
      { name: 'starwarsapi', value: serviceUri }
    ]
    resolvers: resolvers
  }
}

output gatewayUri string = graphqlApiDefinition.outputs.serviceUrl
