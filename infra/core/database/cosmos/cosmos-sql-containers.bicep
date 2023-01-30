param accountName string
param databaseName string
param containers array

resource cosmosAccount 'Microsoft.DocumentDB/databaseAccounts@2022-08-15' existing = {
  name: accountName
}

resource cosmosDatabase 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2022-08-15' existing = {
  name: databaseName
  parent: cosmosAccount
}

resource cosmosContainer 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2022-08-15' = [for item in containers: {
  name: item.name
  parent: cosmosDatabase
  properties: {
    resource: {
      id: item.id
      partitionKey: {
        paths: [ item.partitionKey ]
      }
    }
    options: {
    }
  }
}]
