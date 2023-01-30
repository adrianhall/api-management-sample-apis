param accountName string
param roleDefinitionId string
param principalId string = ''

resource cosmosRole 'Microsoft.DocumentDB/databaseAccounts/sqlRoleAssignments@2022-05-15' = {
  parent: cosmosAccount
  name: guid(roleDefinitionId, principalId, cosmosAccount.id)
  properties: {
    principalId: principalId
    roleDefinitionId: roleDefinitionId
    scope: cosmosAccount.id
  }
}

resource cosmosAccount 'Microsoft.DocumentDB/databaseAccounts@2022-08-15' existing = {
  name: accountName
}
