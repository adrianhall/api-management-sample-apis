param serviceUri string
param path string = 'starwars-graphql'

param apiManagementServiceName string
param apiManagementLoggerName string = ''

var resolvers = [
  { 
    name: 'query-characters', type: 'Query', field: 'characters' 
    policy: loadTextContent('../../src/ApiManagement/StarWarsGraphQLApi/resolvers/query-characters.xml') 
  }
  { 
    name: 'query-films', type: 'Query', field: 'films'
    policy: loadTextContent('../../src/ApiManagement/StarWarsGraphQLApi/resolvers/query-films.xml') 
  }
  { 
    name: 'query-planets', type: 'Query', field: 'planets'
    policy: loadTextContent('../../src/ApiManagement/StarWarsGraphQLApi/resolvers/query-planets.xml') 
  }
  { 
    name: 'query-species', type: 'Query', field: 'species'
    policy: loadTextContent('../../src/ApiManagement/StarWarsGraphQLApi/resolvers/query-species.xml') 
  }
  { 
    name: 'query-starships', type: 'Query', field: 'starships'
    policy: loadTextContent('../../src/ApiManagement/StarWarsGraphQLApi/resolvers/query-starships.xml') 
  }
  { 
    name: 'query-vehicles', type: 'Query', field: 'vehicles'
    policy: loadTextContent('../../src/ApiManagement/StarWarsGraphQLApi/resolvers/query-vehicles.xml') 
  }
  { 
    name: 'query-getcharacterbyid', type: 'Query', field: 'getCharacterById'
    policy: loadTextContent('../../src/ApiManagement/StarWarsGraphQLApi/resolvers/query-getCharacterById.xml') 
  }
  { 
    name: 'query-getfilmbyid', type: 'Query', field: 'getFilmById'
    policy: loadTextContent('../../src/ApiManagement/StarWarsGraphQLApi/resolvers/query-getFilmById.xml') 
  }
  { 
    name: 'query-getplanetbyid', type: 'Query', field: 'getPlanetById'
    policy: loadTextContent('../../src/ApiManagement/StarWarsGraphQLApi/resolvers/query-getPlanetById.xml') 
  }
  { 
    name: 'query-getspeciesbyid', type: 'Query', field: 'getSpeciesById'
    policy: loadTextContent('../../src/ApiManagement/StarWarsGraphQLApi/resolvers/query-getSpeciesById.xml') 
  }
  { 
    name: 'query-getstarshipbyid', type: 'Query', field: 'getStarshipById'
    policy: loadTextContent('../../src/ApiManagement/StarWarsGraphQLApi/resolvers/query-getStarshipById.xml') 
  }
  { 
    name: 'query-getvehiclebyid', type: 'Query', field: 'getVehicleById'
    policy: loadTextContent('../../src/ApiManagement/StarWarsGraphQLApi/resolvers/query-getVehicleById.xml') 
  }
  { 
    name: 'film-all', type: 'Film', field: '*'
    policy: loadTextContent('../../src/ApiManagement/StarWarsGraphQLApi/resolvers/film-all.xml') 
  }
  { 
    name: 'person-all', type: 'Person', field: '*'
    policy: loadTextContent('../../src/ApiManagement/StarWarsGraphQLApi/resolvers/person-all.xml') 
  }
  { 
    name: 'planet-all', type: 'Planet', field: '*'
    policy: loadTextContent('../../src/ApiManagement/StarWarsGraphQLApi/resolvers/planet-all.xml') 
  }
  { 
    name: 'species-all', type: 'Species', field: '*'
    policy: loadTextContent('../../src/ApiManagement/StarWarsGraphQLApi/resolvers/species-all.xml') 
  }
  { 
    name: 'starship-all', type: 'Starship', field: '*'
    policy: loadTextContent('../../src/ApiManagement/StarWarsGraphQLApi/resolvers/starship-all.xml') 
  }
  { 
    name: 'vehicle-all', type: 'Vehicle', field: '*'
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
