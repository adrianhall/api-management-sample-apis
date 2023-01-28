param name string
param location string
param tags object = {}

param applicationInsightsName string = ''
param appServicePlanId string
param apiManagementServiceName string = ''
param apiManagementLoggerName string = ''

module apiService '../core/host/appservice.bicep' = {
  name: name
  params: {
    name: name
    location: location
    tags: tags
    appCommandLine: ''
    applicationInsightsName: applicationInsightsName
    appServicePlanId: appServicePlanId
    appSettings: {}
    runtimeName: 'dotnetcore'
    runtimeVersion: '6.0'
    scmDoBuildDuringDeployment: false
  }
}

module restApiDefinition '../core/gateway/rest-api.bicep' = if (!empty(apiManagementServiceName)) {
  name: 'starwars-rest-api-definition'
  params: {
    name: 'starwars-rest'
    apimServiceName: apiManagementServiceName
    apimLoggerName: apiManagementLoggerName
    path: 'starwars-rest'
    serviceUrl: apiService.outputs.uri
    policy: loadTextContent('../../src/ApiManagement/StarWarsRestApi/policy.xml')
    definition: loadTextContent('../../src/ApiManagement/StarWarsRestApi/swagger.json')
  }
}

output serviceUri string = apiService.outputs.uri
output gatewayUri string = restApiDefinition.outputs.serviceUrl