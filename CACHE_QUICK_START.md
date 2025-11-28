# Quick Start: Redis Cache Configuration

## Local Development

### Using Memory Cache (Default)
No configuration needed. The application uses in-memory cache by default.

### Using Redis Locally

1. Start Redis using Docker:
```bash
docker run -d --name redis -p 6379:6379 redis:7-alpine
```

2. Update `appsettings.json`:
```json
"Cache": {
  "Type": "Redis",
  "Redis": {
    "ConnectionString": "localhost:6379"
  }
}
```

3. Run the application

## Kubernetes Deployment

### Quick Deploy Redis

```bash
cd k8s
kubectl apply -f redis-configmap.yaml
kubectl apply -f redis-pvc.yaml
kubectl apply -f redis-deployment.yaml
kubectl apply -f redis-service.yaml
```

### Configure InkyCal to Use Redis

1. Edit `k8s/config.env`:
```bash
Cache__Type=Redis
```

2. Edit `k8s/secrets.env`:
```bash
redis-connection-string=redis.inkycal:6379
```

3. Apply changes:
```bash
./apply-config.ps1
./apply-secrets.ps1
kubectl rollout restart deployment/web -n inkycal
```

## Verify It's Working

Check the startup logs for:
```
Using Redis cache with connection string: redis.inkycal...
```

Or for memory cache:
```
Using in-memory cache with size limit: 524,288,000 bytes
```

## Switch Back to Memory Cache

1. Edit `k8s/config.env`:
```bash
Cache__Type=Memory
```

2. Apply changes:
```bash
./apply-config.ps1
kubectl rollout restart deployment/web -n inkycal
```

## Configuration Options

| Setting | Values | Default | Description |
|---------|--------|---------|-------------|
| `Cache:Type` | `Memory`, `Redis` | `Memory` | Cache provider to use (enum values) |
| `Cache:Memory:SizeLimit` | Number (bytes) | `524288000` | Memory cache size limit (500 MB) |
| `Cache:Redis:ConnectionString` | Connection string | Empty | Redis connection string |

**Note:** The `Cache:Type` setting is parsed as a `CacheType` enum, so values are case-insensitive but should be either "Memory" or "Redis".

## Common Redis Connection Strings

| Scenario | Connection String |
|----------|------------------|
| Local Redis | `localhost:6379` |
| Kubernetes Redis | `redis.inkycal:6379` |
| Redis with password | `redis-host:6379,password=yourpassword` |
| Redis with SSL | `redis-host:6379,ssl=true,password=yourpassword` |
| Redis Cluster | `redis-host1:6379,redis-host2:6379,redis-host3:6379` |

## Troubleshooting

### Application won't start with Redis configured

The application will automatically fall back to memory cache and log a warning if Redis is unavailable. Check logs for:
```
Redis cache type selected but no connection string provided. Falling back to memory cache.
```

### Redis connection issues

Test Redis connectivity:
```bash
# From local machine
redis-cli -h localhost -p 6379 ping

# From Kubernetes pod
kubectl exec -it -n inkycal deployment/web -- sh
nc -zv redis.inkycal 6379
```

### Performance issues

Monitor Redis memory usage:
```bash
kubectl exec -it -n inkycal deployment/redis -- redis-cli
> INFO memory
> INFO stats
```

Adjust memory limit in `k8s/redis-configmap.yaml` if needed.

## More Information

- Full documentation: `k8s/REDIS_CACHE.md`
- Implementation details: `CACHE_IMPLEMENTATION_SUMMARY.md`
