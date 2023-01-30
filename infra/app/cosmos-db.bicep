param accountName string
param databaseName string
param containers array = []
param location string = resourceGroup().location
param tags object = {}
param principalIds array = []

module cosmosDatabase '../core/database/cosmos/cosmos-sql-db.bicep' = {
  name: 'cosmos-sql-db'
  params: {
    accountName: accountName
    location: location
    tags: tags
    databaseName: databaseName
    principalIds: principalIds
  }
}

module cosmosContainers '../core/database/cosmos/cosmos-sql-containers.bicep' = {
  name: 'cosmos-containers'
  params: {
    accountName: cosmosDatabase.outputs.accountName
    databaseName: cosmosDatabase.outputs.databaseName
    containers: containers
  }
}

output accountName string = cosmosDatabase.outputs.accountName
output databaseName string = cosmosDatabase.outputs.databaseName
output endpoint string = cosmosDatabase.outputs.endpoint
output roleDefinitionId string = cosmosDatabase.outputs.roleDefinitionId
