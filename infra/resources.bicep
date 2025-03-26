param name string
param location string
param resourceToken string
param tags object
param userLogin string
param userObjectId string
// param tenantId string

var prefix = '${name}-${resourceToken}'
var abbrs = loadJsonContent('./abbreviations.json')

resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2024-11-30' = {
  name: 'mi-${resourceToken}'
  location: location
  tags: tags
}

resource web 'Microsoft.Web/sites@2022-03-01' = {
  name: '${prefix}-app-service'
  location: location
  tags: union(tags, { 'azd-service-name': 'web' })
  kind: 'app,linux'
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      alwaysOn: true
      linuxFxVersion: 'DOTNETCORE|9.0'
    }
    httpsOnly: true
  }
  identity: {
    type: 'SystemAssigned, UserAssigned'
    userAssignedIdentities: {
      '${managedIdentity.id}': {}
    }
  }
  
  resource appSettings 'config' = {
    name: 'appsettings'
    properties: {
      //"ENABLE_ORYX_BUILD" : "false", "SCM_DO_BUILD_DURING_DEPLOYMENT" : "false",
      SCM_DO_BUILD_DURING_DEPLOYMENT: 'false'
      ENABLE_ORYX_BUILD: 'false'
    }
  }

  resource connectionstrings 'config' = {
    name:'connectionstrings'
    properties:{
      ESHOPCONTEXT:{
        value: 'Server=tcp:${sqlserver.properties.fullyQualifiedDomainName},1433;Database=${database.name};Authentication=Active Directory Managed Identity;'
        type:'SQLAzure'
      }
      DEFAULTCONNECTION:{
        value: 'Server=tcp:${sqlserver.properties.fullyQualifiedDomainName},1433;Database=${database.name};Authentication=Active Directory Managed Identity;'
        type:'SQLAzure'
      }
    }
  }

  resource logs 'config' = {
    name: 'logs'
    properties: {
      applicationLogs: {
        fileSystem: {
          level: 'Verbose'
        }
      }
      detailedErrorMessages: {
        enabled: true
      }
      failedRequestsTracing: {
        enabled: true
      }
      httpLogs: {
        fileSystem: {
          enabled: true
          retentionInDays: 1
          retentionInMb: 35
        }
      }
    }
  }

}

resource appServicePlan 'Microsoft.Web/serverfarms@2021-03-01' = {
  name: '${prefix}-service-plan'
  location: location
  tags: tags
  sku: {
    name: 'S1'
  }
  properties: {
    reserved: true
  }
}

resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2020-03-01-preview' = {
  name: '${prefix}-workspace'
  location: location
  tags: tags
  properties: any({
    retentionInDays: 30
    features: {
      searchVersion: 1
    }
    sku: {
      name: 'PerGB2018'
    }
  })
}

// module applicationInsightsResources './core/monitor/applicationinsights.bicep' = {
//   name: 'applicationinsights-resources'
//   params: {
//     name: '${prefix}-appinsights'
//     dashboardName:'${prefix}-dashboard'
//     location: location
//     tags: tags
//     logAnalyticsWorkspaceId: logAnalyticsWorkspace.id
//   }
  
// }

resource sqlserver 'Microsoft.Sql/servers@2021-11-01' = {
  location: location
  name:'${prefix}-sqlserver'
  tags:tags
  properties:{
    publicNetworkAccess:'Enabled'
    administrators: {
      administratorType: 'ActiveDirectory'
      azureADOnlyAuthentication: true
      principalType: 'User'
      login: web.name
      sid: web.identity.principalId
      tenantId: subscription().tenantId
    }
  }
  // resource sqlserverADOnlyAuth 'azureADOnlyAuthentications' = {
  //   name: 'Default'
  //   properties: {
  //     azureADOnlyAuthentication: true
  //   }
  // }
  // resource entraadmin 'administrators' = {
  //   name: 'ActiveDirectory'
  //   properties: {
  //     administratorType: 'ActiveDirectory'
  //     principalType: 'Application'
  //     login: userLogin
  //     sid: userObjectId
  //     tenantId: tenantId
  //   }
  // }
}

resource database 'Microsoft.Sql/servers/databases@2022-02-01-preview' = {
  parent:sqlserver
  name: 'database'
  location: location
}

// Grant the Web App's Managed Identity access to the SQL Database
// resource sqlAdminRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
//   name: guid(web.name, 'sql-admin-role-assignment')
//   properties: {
//     roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'db0b5fdb-5a0c-4c9a-8b6d-5e1bcb7d1b4c') // SQL DB Contributor role
//     principalId: web.identity.principalId
//   }
//   scope: database
// } 

resource sqlDatabaseRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(web.name, 'Contributor')
  scope: database
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'b24988ac-6180-42a0-ab88-20f7382dd24c')
    principalId: web.identity.principalId
  }
}

  resource sqlserverfirewall 'Microsoft.Sql/servers/firewallRules@2024-05-01-preview' = {
    name: '${sqlserver.name}-AllowAllWindowsAzureIps'
    parent: sqlserver
    properties:{
      startIpAddress:'0.0.0.0'
      endIpAddress:'0.0.0.0'
    }
  }

module keyvault 'core/security/keyvault.bicep' = {
  name:'${abbrs.keyVaultVaults}${resourceToken}'
  params:{
    location: location
    name:'${abbrs.keyVaultVaults}${resourceToken}'
  }
}  


output WEB_URI string = 'https://${web.properties.defaultHostName}'
//output APPLICATIONINSIGHTS_CONNECTION_STRING string = applicationInsightsResources.outputs.connectionString
