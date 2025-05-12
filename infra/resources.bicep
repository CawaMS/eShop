param name string
param location string
param resourceToken string
param tags object
param openAiSku object = {
  name:'S0'
}

var embeddingModelName = 'text-embedding-ada-002'
var embeddingDeploymentCapacity = 30

var prefix = '${name}-${resourceToken}'
//added for Redis Cache
var cacheServerName = '${prefix}-redisCache'
var databaseSubnetName = 'database-subnet'
var databaseDnsZoneName = 'privatelink${environment().suffixes.sqlServerHostname}'
var databasePrivateEndpointName = 'database-privateEndpoint'
var databasePvtEndpointDnsGroupName = 'sqlDnsGroup'
var webappSubnetName = 'webapp-subnet'
//added for Redis Cache
var cacheSubnetName = 'cache-subnet'
//added for Redis Cache
var cachePrivateEndpointName = 'cache-privateEndpoint'
//added for Redis Cache
var cachePvtEndpointDnsGroupName = 'cacheDnsGroup'
var abbrs = loadJsonContent('./abbreviations.json')
var redisPort = 10000

resource virtualNetwork 'Microsoft.Network/virtualNetworks@2019-11-01' = {
  name: '${prefix}-vnet'
  location: location
  tags: tags
  properties: {
    addressSpace: {
      addressPrefixes: [
        '10.0.0.0/16'
      ]
    }
    subnets: [
      {
        name: databaseSubnetName
        properties:{
          addressPrefix: '10.0.0.0/24'
          privateEndpointNetworkPolicies: 'Disabled'
        }
      }
      {
        name: webappSubnetName
        properties: {
          addressPrefix: '10.0.1.0/24'
          delegations: [
            {
              name: '${prefix}-subnet-delegation-web'
              properties: {
                serviceName: 'Microsoft.Web/serverFarms'
              }
            }
          ]
        }
      }
      {
        name: cacheSubnetName
        properties:{
          addressPrefix: '10.0.2.0/24'
        }
      }
    ]
  }
  resource databaseSubnet 'subnets' existing = {
    name: databaseSubnetName
  }
  resource webappSubnet 'subnets' existing = {
    name: webappSubnetName
  }
  //added for Redis Cache
  resource cacheSubnet 'subnets' existing = {
    name: cacheSubnetName
  }
}

resource privateDnsZoneDatabase 'Microsoft.Network/privateDnsZones@2020-06-01' = {
  name: databaseDnsZoneName
  location: 'global'
  tags: tags
  dependsOn:[
    virtualNetwork
  ]
}

resource privateDnsZoneLinkDatabase 'Microsoft.Network/privateDnsZones/virtualNetworkLinks@2020-06-01' = {
  parent: privateDnsZoneDatabase
  name: '${databaseDnsZoneName}-link'
  location: 'global'
  properties:{
    registrationEnabled:false
    virtualNetwork:{
      id: virtualNetwork.id
    }
  }
}

resource databasePrivateEndpoint 'Microsoft.Network/privateEndpoints@2023-05-01' = {
  name: databasePrivateEndpointName
  location: location
  properties:{
    subnet: {
      id: virtualNetwork::databaseSubnet.id
    }
    privateLinkServiceConnections:[
      {
        name: databasePrivateEndpointName
        properties:{
          privateLinkServiceId: sqlserver.id
          groupIds:[
            'sqlServer'
          ]
        }
      }
    ]
  }
  resource databasePvtEndpointDnsGroup 'privateDnsZoneGroups' = {
    name: databasePvtEndpointDnsGroupName
    properties:{
      privateDnsZoneConfigs:[
        {
          name: 'database-config'
          properties:{
            privateDnsZoneId: privateDnsZoneDatabase.id
          }
        }
      ]
    }
  }
}


// added for Redis Cache
resource privateDnsZoneCache 'Microsoft.Network/privateDnsZones@2020-06-01' = {
  name: 'privatelink.redis.azure.net'
  location: 'global'
  tags: tags
  dependsOn:[
    virtualNetwork
  ]
}

 //added for Redis Cache
resource privateDnsZoneLinkCache 'Microsoft.Network/privateDnsZones/virtualNetworkLinks@2020-06-01' = {
 parent: privateDnsZoneCache
 name: 'privatelink.redis.azure.net-applink'
 location: 'global'
 properties: {
   registrationEnabled: false
   virtualNetwork: {
     id: virtualNetwork.id
   }
 }
}


resource cachePrivateEndpoint 'Microsoft.Network/privateEndpoints@2023-05-01' = {
  name: cachePrivateEndpointName
  location: location
  properties: {
    subnet: {
      id: virtualNetwork::cacheSubnet.id
    }
    privateLinkServiceConnections: [
      {
        name: cachePrivateEndpointName
        properties: {
          privateLinkServiceId: redisCache.id
          groupIds: [
            'redisEnterprise'
          ]
        }
      }
    ]
  }
  resource cachePvtEndpointDnsGroup 'privateDnsZoneGroups' = {
    name: cachePvtEndpointDnsGroupName
    properties: {
      privateDnsZoneConfigs: [
        {
          name: 'privatelink-redis-azure-net'
          properties: {
            privateDnsZoneId: privateDnsZoneCache.id
          }
        }
      ]
    }
  }
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
    //type: 'UserAssigned'
    //userAssignedIdentities: {
    //  '${managedIdentity.id}': {}
    //}
    type: 'SystemAssigned'
  }
  
  resource appSettings 'config' = {
    name: 'appsettings'
    properties: {
      //"ENABLE_ORYX_BUILD" : "false", "SCM_DO_BUILD_DURING_DEPLOYMENT" : "false",
      SCM_DO_BUILD_DURING_DEPLOYMENT: 'false'
      ENABLE_ORYX_BUILD: 'false'
      aoaiConnection: cognitiveAccount.properties.endpoint
      aoaiKey: cognitiveAccount.listKeys().key1
      textEmbeddingsDeploymentName: textembeddingdeployment.name
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
      ESHOPREDISCONNECTION:{
        //value:'${redisCache.properties.hostName}:10000,password=${redisdatabase.listKeys().primaryKey},ssl=True,abortConnect=False'
        value:'${redisCache.properties.hostName}:10000,ssl=true'
        type: 'Custom'
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

  resource webappVnetConfig 'networkConfig' = {
    name: 'virtualNetwork'
    properties: {
      subnetResourceId: virtualNetwork::webappSubnet.id
    }
  }

  dependsOn: [virtualNetwork]

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

resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: 'mi-${resourceToken}'
  location: location
  tags: tags
}

resource sqlserver 'Microsoft.Sql/servers@2022-08-01-preview' = {
  location: location
  name:'${prefix}-sqlserver'
  tags:tags
  properties:{
    publicNetworkAccess:'Disabled'
    administrators: {
      administratorType: 'ActiveDirectory'
      //login:managedIdentity.name
      //sid: managedIdentity.properties.principalId
      login: web.name
      sid:web.identity.principalId
      tenantId: subscription().tenantId
      azureADOnlyAuthentication: true
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
    // principalId: managedIdentity.properties.principalId
    principalId: web.identity.principalId
  }
}

module keyvault 'core/security/keyvault.bicep' = {
  name:'${abbrs.keyVaultVaults}${resourceToken}'
  params:{
    location: location
    name:'${abbrs.keyVaultVaults}${resourceToken}'
  }
}

//added for Redis Cache
resource redisCache 'Microsoft.Cache/redisEnterprise@2025-04-01' = {
  location:location
  name:cacheServerName
  sku:{
    // capacity:2
    name:'Balanced_B5'
  }
}     

resource redisdatabase 'Microsoft.Cache/redisEnterprise/databases@2025-04-01' = {
  name: 'default'
  parent: redisCache
  properties: {
    accessKeysAuthentication: 'Enabled'
    evictionPolicy:'NoEviction'
    clusteringPolicy: 'EnterpriseCluster'
    modules: [
      {
        name: 'RediSearch'
      }
      {
        name: 'RedisJSON'
      }
    ]
    port: redisPort
  }
}

resource redisAccessPolicyAssignmentName 'Microsoft.Cache/redisEnterprise/databases/accessPolicyAssignments@2025-04-01' = {
  name: take('cachecontributor${uniqueString(resourceGroup().id)}', 24)
  parent: redisdatabase
  properties: {
    accessPolicyName: 'default'
    user: {
      objectId: web.identity.principalId
      }
    }
  }

//azure open ai resource
resource cognitiveAccount 'Microsoft.CognitiveServices/accounts@2023-05-01' = {
  name: '${name}-csaccount'
  location: 'australiaeast'
  tags: tags
  kind: 'OpenAI'
  properties: {
    customSubDomainName: '${name}-csaccount'
    publicNetworkAccess: 'Enabled'
  }
  sku: openAiSku
}

//ada text embedding service
resource textembeddingdeployment 'Microsoft.CognitiveServices/accounts/deployments@2023-05-01' = {
  name:'${name}-textembedding'
  parent: cognitiveAccount
  properties:{
    model: {
      format: 'OpenAI'
      name: embeddingModelName
      version: '2'
    }
  }
  sku: {
    name: 'Standard'
    capacity: embeddingDeploymentCapacity
  }
}

output WEB_URI string = 'https://${web.properties.defaultHostName}'
//output APPLICATIONINSIGHTS_CONNECTION_STRING string = applicationInsightsResources.outputs.connectionString
