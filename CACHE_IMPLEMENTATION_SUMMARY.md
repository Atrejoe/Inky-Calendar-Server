# Cache Configuration Implementation Summary

## Overview

The caching system in InkyCal has been made configurable to support both in-memory and Redis-based distributed caching. This is particularly useful for Kubernetes deployments with multiple replicas.

## Changes Made

### 1. Cache Abstraction Layer

Created two cache service interfaces:

- **`IImageCacheService`** (`InkyCal.Utils\Caching\IImageCacheService.cs`): For caching binary data (images, PDFs)
- **`IStringCacheService`** (`InkyCal.Utils\Caching\IStringCacheService.cs`): For caching string data (calendar content)

### 2. Cache Implementations

#### Memory Cache Implementations:
- **`MemoryCacheService`** (`InkyCal.Utils\Caching\MemoryCacheService.cs`): In-memory image cache using `Microsoft.Extensions.Caching.Memory`
- **`MemoryStringCacheService`** (`InkyCal.Utils\Caching\MemoryStringCacheService.cs`): In-memory string cache

#### Redis Cache Implementations:
- **`RedisCacheService`** (`InkyCal.Utils\Caching\RedisCacheService.cs`): Redis-based image cache using `StackExchange.Redis`
- **`RedisStringCacheService`** (`InkyCal.Utils\Caching\RedisStringCacheService.cs`): Redis-based string cache

### 3. Updated Components

The following components were updated to use the cache abstraction:

- **`IPanelRendererExtensions`** (`InkyCal.Utils\IPanelRenderer.cs`): Panel image rendering cache
- **`DownloadCache`** (`InkyCal.Utils\DownloadCache.cs`): Downloaded content cache
- **`ICalExtensions`** (`InkyCal.Utils\Calendar\iCalExtensions.cs`): Calendar content cache
- **`PdfRenderer`** (`InkyCal.Utils\PdfRenderer.cs`): PDF conversion cache
- **`PdfRendererHelper`** (`InkyCal.Utils\PdfRendererHelper.cs`): Helper for initializing PDF renderer cache

### 4. Configuration Support

#### Config Class (`InkyCal.Server.Config\Config.cs`)
Added properties:
- `CacheType`: Specifies cache type using `CacheType` enum (Memory or Redis)
- `RedisCacheConnectionString`: Redis connection string
- `MemoryCacheSizeLimit`: Memory cache size limit in bytes

Added enum:
- `CacheType` enum with values: `Memory` (default) and `Redis`

#### Application Settings (`InkyCal.Server\appsettings.json`)
Added configuration section:
```json
"Cache": {
  "Type": "Memory",
  "Memory": {
    "SizeLimit": 524288000
  },
  "Redis": {
    "ConnectionString": ""
  }
}
```

Note: `Type` can be set to "Memory" or "Redis" (case-insensitive) and will be parsed to the `CacheType` enum.

#### Startup Configuration (`InkyCal.Server\Startup.cs`)
- Registers cache services based on configuration
- Initializes static cache services for backward compatibility
- Handles fallback to memory cache if Redis is unavailable

### 5. Kubernetes Resources

Created the following Kubernetes resources for Redis deployment:

- **`k8s\redis-configmap.yaml`**: Redis configuration (memory limits, eviction policy)
- **`k8s\redis-deployment.yaml`**: Redis deployment with persistent storage
- **`k8s\redis-service.yaml`**: ClusterIP service for Redis
- **`k8s\redis-pvc.yaml`**: PersistentVolumeClaim for Redis data
- **`k8s\REDIS_CACHE.md`**: Comprehensive deployment and troubleshooting guide

#### Configuration Templates Updated:
- **`k8s\config.env.template`**: Added `Cache__Type` and `Cache__Memory__SizeLimit`
- **`k8s\secrets.env.template`**: Added `redis-connection-string`
- **`k8s\web-deployment.yaml`**: Added environment variables for cache configuration

### 6. Package Dependencies

Added `StackExchange.Redis` version 2.8.29 to `InkyCal.Utils.csproj`.

## Usage

### Using Memory Cache (Default)

In `appsettings.json`:
```json
"Cache": {
  "Type": "Memory",
  "Memory": {
    "SizeLimit": 524288000
  }
}
```

### Using Redis Cache

In `appsettings.json`:
```json
"Cache": {
  "Type": "Redis",
  "Redis": {
    "ConnectionString": "redis-host:6379"
  }
}
```

For Kubernetes deployments, configure via environment variables:
- `Cache__Type=Redis`
- `Cache__Redis__ConnectionString=redis.inkycal:6379`

## Kubernetes Deployment

### 1. Deploy Redis

```bash
kubectl apply -f k8s/redis-configmap.yaml
kubectl apply -f k8s/redis-pvc.yaml
kubectl apply -f k8s/redis-deployment.yaml
kubectl apply -f k8s/redis-service.yaml
```

### 2. Configure InkyCal

Update `k8s/config.env`:
```bash
Cache__Type=Redis
```

Update `k8s/secrets.env`:
```bash
redis-connection-string=redis.inkycal:6379
```

Apply configuration:
```bash
./k8s/apply-config.ps1
./k8s/apply-secrets.ps1
```

### 3. Redeploy Application

```bash
kubectl rollout restart deployment/web -n inkycal
```

## Benefits

1. **Scalability**: Multiple application replicas can share cache via Redis
2. **Performance**: Reduced image regeneration across replicas
3. **Flexibility**: Easy switching between memory and Redis cache
4. **Resilience**: Automatic fallback to memory cache if Redis is unavailable
5. **Kubernetes-Ready**: Full Kubernetes deployment manifests included

## Backward Compatibility

All existing code continues to work without changes. The cache services are initialized at startup and provided to existing static cache implementations via setter methods. If no cache service is configured, the system automatically falls back to in-memory caching.

## Monitoring

Check cache status:
```bash
# Redis pod status
kubectl get pods -n inkycal -l component=redis

# Redis logs
kubectl logs -n inkycal -l component=redis

# Redis CLI
kubectl exec -it -n inkycal deployment/redis -- redis-cli
```

Inside Redis CLI:
```
INFO stats
INFO memory
```

## Troubleshooting

See `k8s\REDIS_CACHE.md` for detailed troubleshooting steps including:
- Redis pod not starting
- Connection issues from web pods
- Performance tuning
- Persistence configuration

## Future Enhancements

Potential improvements:
1. Add cache metrics and monitoring endpoints
2. Implement distributed locking for cache updates
3. Add cache warming strategies
4. Support for additional cache providers (e.g., Memcached)
5. Cache key versioning for invalidation strategies
