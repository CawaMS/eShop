param name string
param location string
param resourceToken string
param tags object
@secure()
@description('the SQL database user and app password')
param databasePassword string

var prefix = '${name}-${resourceToken}'
//added for Redis Cache
var cacheServerName = '${prefix}-redisCache'
param sqlAdmin string = 'sqlAdmin'
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
  name: 'privatelink.redisenterprise.cache.azure.net'
  location: 'global'
  tags: tags
  dependsOn:[
    virtualNetwork
  ]
}

 //added for Redis Cache
resource privateDnsZoneLinkCache 'Microsoft.Network/privateDnsZones/virtualNetworkLinks@2020-06-01' = {
 parent: privateDnsZoneCache
 name: 'privatelink.redisenterprise.cache.azure.net-applink'
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
          name: 'privatelink-redisenterprise-cache-azure-net'
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
      linuxFxVersion: 'DOTNETCORE|8.0'
    }
    httpsOnly: true
  }
  identity: {
    type: 'SystemAssigned'
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
        value:'Server=tcp:${sqlserver.properties.fullyQualifiedDomainName},1433;Initial Catalog=${sqlserver::database.name};Persist Security Info=False;User ID=${sqlAdmin};Password=${databasePassword};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;'
        type:'SQLAzure'
      }
      DEFAULTCONNECTION:{
        value: 'Server=tcp:${sqlserver.properties.fullyQualifiedDomainName},1433;Initial Catalog=${sqlserver::database.name};Persist Security Info=False;User ID=${sqlAdmin};Password=${databasePassword};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;'
        type:'SQLAzure'
      }
      ESHOPREDISCONNECTION:{
        value:'${redisCache.properties.hostName}:10000,password=${redisdatabase.listKeys().primaryKey},ssl=True,abortConnect=False'
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

module applicationInsightsResources './core/monitor/applicationinsights.bicep' = {
  name: 'applicationinsights-resources'
  params: {
    name: '${prefix}-appinsights'
    dashboardName:'${prefix}-dashboard'
    location: location
    tags: tags
    logAnalyticsWorkspaceId: logAnalyticsWorkspace.id
  }
  
}

resource sqlserver 'Microsoft.Sql/servers@2023-02-01-preview' = {
  location: location
  name:'${prefix}-sqlserver'
  tags:tags
  properties:{
    administratorLogin:sqlAdmin
    administratorLoginPassword:databasePassword
    publicNetworkAccess:'Disabled'
  }
  resource database 'databases' = {
    name: 'database'
    location: location
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
resource redisCache 'Microsoft.Cache/redisEnterprise@2024-02-01' = {
  location:location
  name:cacheServerName
  sku:{
    capacity:2
    name:'Enterprise_E10'
  }
}     

resource redisdatabase 'Microsoft.Cache/redisEnterprise/databases@2024-02-01' = {
  name: 'default'
  parent: redisCache
  properties: {
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

output WEB_URI string = 'https://${web.properties.defaultHostName}'
output APPLICATIONINSIGHTS_CONNECTION_STRING string = applicationInsightsResources.outputs.connectionString
