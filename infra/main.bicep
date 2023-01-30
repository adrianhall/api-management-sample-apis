// ---------------------------------------------------------------------------------------------
//
//  Infrastructure for spinning up on API services for testing GraphQL on API Management
//  
//  Copyright (C) 2023 Microsoft, Inc. All Rights Reserved
//  Licensed under the MIT License
//
// ---------------------------------------------------------------------------------------------
targetScope = 'subscription'

// ---------------------------------------------------------------------------------------------
//  Parameters - these are handled by the Azure Developer CLI
// ---------------------------------------------------------------------------------------------
@minLength(1)
@maxLength(64)
@description('Name of the the environment which is used to generate a short unique hash used in all resources.')
param environmentName string

@minLength(1)
@description('Primary location for all resources')
param location string

// ---------------------------------------------------------------------------------------------
//  Optional Parameters
//    Each resource has an optional override for the default azd resource naming conventions.
//    Update the main.parameters.json file to specify them:
//
//    "webServiceName": {
//      "value": "my-web-service"
//    }
// ---------------------------------------------------------------------------------------------

// Supporting services
param applicationInsightsDashboardName string = ''
param applicationInsightsName string = ''
param appServicePlanName string = ''
param cosmosAccountName string = ''
param cosmosDatabaseName string = ''
param logAnalyticsName string = ''
param resourceGroupName string = ''

// Underlying API Service Names
param starwarsRestServiceName string = ''
param todoRestServiceName string = ''

// API Management instance
param apiManagementServiceName string = ''

// ---------------------------------------------------------------------------------------------
//  Variables
//    These should not need to be touched.
// ---------------------------------------------------------------------------------------------
var abbrs = loadJsonContent('./abbreviations.json')
var resourceToken = toLower(uniqueString(subscription().id, environmentName, location))
var tags = { 'azd-env-name': environmentName }

var todoAppContainers = [
  { name: 'TodoList', id: 'TodoList', partitionKey: '/id' }
  { name: 'TodoItem', id: 'TodoItem', partitionKey: '/id' }
]

// ---------------------------------------------------------------------------------------------
//  RESOURCE GROUP
// ---------------------------------------------------------------------------------------------
resource rg 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: !empty(resourceGroupName) ? resourceGroupName : '${abbrs.resourcesResourceGroups}${environmentName}'
  location: location
  tags: tags
}

// ---------------------------------------------------------------------------------------------
//  MONITORING (Azure Monitor, Application Insights)
// ---------------------------------------------------------------------------------------------
module monitoring './core/monitor/monitoring.bicep' = {
  name: 'monitoring'
  scope: rg
  params: {
    location: location
    tags: tags
    logAnalyticsName: !empty(logAnalyticsName) ? logAnalyticsName : '${abbrs.operationalInsightsWorkspaces}${resourceToken}'
    applicationInsightsName: !empty(applicationInsightsName) ? applicationInsightsName : '${abbrs.insightsComponents}${resourceToken}'
    applicationInsightsDashboardName: !empty(applicationInsightsDashboardName) ? applicationInsightsDashboardName : '${abbrs.portalDashboards}${resourceToken}'
  }
}

// ---------------------------------------------------------------------------------------------
//  Database (Cosmos DB)
// ---------------------------------------------------------------------------------------------
module cosmos './app/cosmos-db.bicep' = {
  name: 'cosmos-sql'
  scope: rg
  params: {
    accountName: !empty(cosmosAccountName) ? cosmosAccountName : '${abbrs.documentDBDatabaseAccounts}${resourceToken}'
    location: location
    tags: tags
    databaseName: !empty(cosmosDatabaseName) ? cosmosDatabaseName : 'apisamples'
    // Define the containers for a specific API in a variables, then use concat to create the superset.
    // We use union so that duplicate values are ignored.
    containers: union([], todoAppContainers)
  }
}

// ---------------------------------------------------------------------------------------------
//  API Services (App Services)
// ---------------------------------------------------------------------------------------------
module appServicePlan './core/host/appserviceplan.bicep' = {
  name: 'appserviceplan'
  scope: rg
  params: {
    name: !empty(appServicePlanName) ? appServicePlanName : '${abbrs.webServerFarms}${resourceToken}'
    location: location
    tags: tags
    sku: {
      name: 'B1'
    }
  }
}

// ---------------------------------------------------------------------------------------------
//  API Management Service
// ---------------------------------------------------------------------------------------------
module apiManagement './core/gateway/api-management.bicep' = {
  name: 'api-management'
  scope: rg
  params: {
    name: !empty(apiManagementServiceName) ? apiManagementServiceName : '${abbrs.apiManagementService}${resourceToken}'
    location: location
    tags: tags
    applicationInsightsName: monitoring.outputs.applicationInsightsName
    sku: 'Developer'
  }
}

// ---------------------------------------------------------------------------------------------
//  API: Star Wars REST
// ---------------------------------------------------------------------------------------------
module starWarsRestApiService './app/starwars-rest-api.bicep' = {
  name: 'starwars-rest-api-service'
  scope: rg
  params: {
    name: !empty(starwarsRestServiceName) ? starwarsRestServiceName : 'starwars-rest-${resourceToken}'
    location: location
    tags: union(tags, { 'azd-service-name': 'starwars-rest' })
    applicationInsightsName: monitoring.outputs.applicationInsightsName
    appServicePlanId: appServicePlan.outputs.id
    apiManagementServiceName: apiManagement.outputs.serviceName
    apiManagementLoggerName: apiManagement.outputs.loggerName
  }
}

// ---------------------------------------------------------------------------------------------
//  API: Star Wars REST
// ---------------------------------------------------------------------------------------------
module todoRestApiService './app/todo-rest-api.bicep' = {
  name: 'todo-rest-api-service'
  scope: rg
  params: {
    name: !empty(todoRestServiceName) ? todoRestServiceName : 'todo-rest-${resourceToken}'
    location: location
    tags: union(tags, { 'azd-service-name': 'todo-rest' })
    applicationInsightsName: monitoring.outputs.applicationInsightsName
    appSettings: {
      AZURE_COSMOS_DATABASE_NAME: cosmos.outputs.databaseName
      AZURE_COSMOS_ENDPOINT: cosmos.outputs.endpoint
    }
    appServicePlanId: appServicePlan.outputs.id
    apiManagementServiceName: apiManagement.outputs.serviceName
    apiManagementLoggerName: apiManagement.outputs.loggerName
  }
}

module todoRestApiCosmosSqlRole './core/database/cosmos/cosmos-sql-role-assign.bicep' = {
  name: 'todo-rest-api-service-cosmos-access'
  scope: rg
  params: {
    accountName: cosmos.outputs.accountName
    roleDefinitionId: cosmos.outputs.roleDefinitionId
    principalId: todoRestApiService.outputs.servicePrincipalId
  }
}

// ---------------------------------------------------------------------------------------------
//  OUTPUTS
//
//  These are used by Azure Developer CLI to configure deployed applications.
//
// ---------------------------------------------------------------------------------------------
output APPLICATIONINSIGHTS_CONNECTION_STRING string = monitoring.outputs.applicationInsightsConnectionString
output API_MANAGEMENT_SERVICE_URI string = apiManagement.outputs.uri
output AZURE_LOCATION string = location
output AZURE_TENANT_ID string = tenant().tenantId
output COSMOS_DATABASE_NAME string = cosmos.outputs.databaseName
output COSMOS_ENDPOINT string = cosmos.outputs.endpoint
output STARWARS_REST_GATEWAY_URI string = starWarsRestApiService.outputs.gatewayUri
output TODO_REST_GATEWAY_URI string = todoRestApiService.outputs.gatewayUri
