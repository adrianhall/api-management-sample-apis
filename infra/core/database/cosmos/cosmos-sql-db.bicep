param accountName string
param databaseName string
param principalIds array = []
param location string = resourceGroup().location
param tags object = {}

resource cosmosAccount 'Microsoft.DocumentDB/databaseAccounts@2022-08-15' = {
  name: accountName
  kind: 'GlobalDocumentDB'
  location: location
  tags: tags
  properties: {
    consistencyPolicy: { 
      defaultConsistencyLevel: 'Session' 
    }
    locations: [
      {
        locationName: location
      }
    ]
    databaseAccountOfferType: 'Standard'
  }
}

resource cosmosDatabase 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2022-08-15' = {
  name: databaseName
  parent: cosmosAccount
  properties: {
    resource: {
      id: databaseName
    }
  }
}

resource roleDefinition 'Microsoft.DocumentDB/databaseAccounts/sqlRoleDefinitions@2022-08-15' = {
  name: guid(cosmosAccount.id, cosmosAccount.name, 'sql-role')
  parent: cosmosAccount
  properties: {
    assignableScopes: [
      cosmosAccount.id
    ]
    permissions: [
      {
        dataActions: [
          'Microsoft.DocumentDB/databaseAccounts/readMetadata'
          'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers/items/*'
          'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers/*'  
        ]
        notDataActions: []
      }
    ]
    roleName: 'Reader Writer'
    type: 'CustomRole'
  }
}

@batchSize(1)
module userRole 'cosmos-sql-role-assign.bicep' = [for principalId in principalIds: if (!empty(principalId)) {
  name: 'cosmos-sql-user-role-${uniqueString(principalId)}'
  params: {
    accountName: accountName
    roleDefinitionId: roleDefinition.id
    principalId: principalId
  }
  dependsOn: [
    cosmosDatabase
  ]
}]

output accountId string = cosmosAccount.id
output accountName string = cosmosAccount.name
output databaseName string = databaseName
output endpoint string = cosmosAccount.properties.documentEndpoint
output roleDefinitionId string = roleDefinition.id
