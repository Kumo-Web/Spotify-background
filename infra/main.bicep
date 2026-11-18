
param location string = 'uksouth'
param cosmosAccountName string = 'cosmo-spotify'

resource cosmos 'Microsoft.DocumentDB/databaseAccounts@2026-03-15' = {
  name: cosmosAccountName
  location: location

  
  kind: 'GlobalDocumentDB'

  properties: {
    databaseAccountOfferType: 'Standard'
    enableFreeTier: true
    capacity: {
      totalThroughputLimit: 1000
    }
    locations: [
      {
        locationName: location
        failoverPriority: 0 // 0 = the primary (and only) region
        isZoneRedundant: false // zone redundancy costs extra
      }
    ]
    enableMultipleWriteLocations: false
    enableAutomaticFailover: true
    minimalTlsVersion: 'Tls12'
    consistencyPolicy: {
      defaultConsistencyLevel: 'Session'
    }
    backupPolicy: {
      type: 'Periodic'
      periodicModeProperties: {
        backupIntervalInMinutes: 240 // a backup every 4 hours
        backupRetentionIntervalInHours: 8 // keep the last 8 hours (two backups, the free amount)
        backupStorageRedundancy: 'Local' // copies stay in UK South
      }
    }
  }
}
resource database 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2026-03-15' = {
  parent: cosmos
  name: 'spotify' // must equal Cosmos:DatabaseName in appsettings

  properties: {
    resource: {
      id: 'spotify'
    }
    options: {
      throughput: 1000
    }
  }
}

resource container 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2026-03-15' = {
  parent: database
  name: 'app' // must equal Cosmos:ContainerName in appsettings

  properties: {
    resource: {
      id: 'app' // must be the same as `name`
      partitionKey: {
        paths: [
          '/spotifyUserId'
        ]
        kind: 'Hash'
      }

      // -1 means "TTL is switched on, but nothing expires unless the document says so".
      // OAuthState documents set their own ttl (a few minutes), so abandoned logins clean
      // themselves up. Leave this out and Cosmos ignores the per-document ttl entirely.
      defaultTtl: -1
    }
  }
}
