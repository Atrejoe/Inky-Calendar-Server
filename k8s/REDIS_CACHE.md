# Redis Cache for InkyCal

This directory contains Kubernetes resources for deploying Redis as a caching layer for InkyCal.

## Overview

InkyCal can use Redis for distributed caching, which is particularly useful in Kubernetes deployments with multiple replicas. This allows cache sharing across pods and provides better performance and consistency.

## Components

- **redis-configmap.yaml**: Redis configuration including memory limits and eviction policy
- **redis-deployment.yaml**: Redis deployment with persistent storage
- **redis-service.yaml**: ClusterIP service for Redis access within the cluster
- **redis-pvc.yaml**: PersistentVolumeClaim for Redis data (uses `nfs-readwriteonce` storage class)

## Prerequisites

- Kubernetes cluster with NFS storage provisioner
- Storage class `nfs-readwriteonce` available in your cluster
- Namespace `inkycal` created

## Deployment

### 1. Apply the Redis resources

```bash
kubectl apply -f k8s/redis-configmap.yaml
kubectl apply -f k8s/redis-pvc.yaml
kubectl apply -f k8s/redis-deployment.yaml
kubectl apply -f k8s/redis-service.yaml
```

### 2. Configure InkyCal to use Redis

Update your `config.env` file:

```bash
# Cache type can be: Memory or Redis
Cache__Type=Redis
```

Update your `secrets.env` file:

```bash
redis-connection-string=redis.inkycal:6379
```

### 3. Apply the updated configuration

```bash
./k8s/apply-config.ps1
./k8s/apply-secrets.ps1
```

### 4. Redeploy the web application

```bash
kubectl rollout restart deployment/web -n inkycal
```

## Configuration Options

### Cache Type

In `config.env`:
- `Cache__Type=Memory` - Use in-memory cache (default, no Redis needed)
- `Cache__Type=Redis` - Use Redis for distributed caching

### Memory Cache Settings

When using in-memory cache:
- `Cache__Memory__SizeLimit=524288000` - Size limit in bytes (default: 500MB)

### Redis Connection

In `secrets.env`:
- `redis-connection-string=redis.inkycal:6379` - Redis connection string

For advanced Redis configurations, you can use:
```
redis-connection-string=redis.inkycal:6379,password=yourpassword,ssl=true
```

## Monitoring

Check Redis status:

```bash
# Check pod status
kubectl get pods -n inkycal -l component=redis

# Check Redis logs
kubectl logs -n inkycal -l component=redis

# Connect to Redis CLI
kubectl exec -it -n inkycal deployment/redis -- redis-cli

# Inside Redis CLI, check stats
INFO stats
INFO memory
```

## Troubleshooting

### Redis pod not starting

Check the pod logs:
```bash
kubectl logs -n inkycal -l component=redis
```

Check PVC status:
```bash
kubectl get pvc -n inkycal redis-data-pvc
kubectl describe pvc -n inkycal redis-data-pvc
```

**If PVC is pending:**
- Verify the storage class exists: `kubectl get storageclass nfs-readwriteonce`
- Check if NFS provisioner is running
- Ensure sufficient storage quota available

### Web pods can't connect to Redis

Check if the service is accessible:
```bash
kubectl get svc -n inkycal redis
```

Test connectivity from a web pod:
```bash
kubectl exec -it -n inkycal deployment/web -- sh
# Inside the container
nc -zv redis.inkycal 6379
```

If Redis is unavailable, the application will automatically fall back to in-memory caching and log warnings.

## Performance Tuning

### Memory Limits

The Redis configuration uses an LRU (Least Recently Used) eviction policy with a 512MB limit. Adjust this in `redis-configmap.yaml`:

```yaml
maxmemory 512mb
maxmemory-policy allkeys-lru
```

### Persistence

By default, Redis persistence is disabled for cache use (data loss on restart is acceptable). If you need persistence:

Edit `redis-configmap.yaml`:
```yaml
save 900 1
save 300 10
save 60 10000
appendonly yes
```

## Cleanup

To remove Redis from your cluster:

```bash
kubectl delete -f k8s/redis-service.yaml
kubectl delete -f k8s/redis-deployment.yaml
kubectl delete -f k8s/redis-pvc.yaml
kubectl delete -f k8s/redis-configmap.yaml
```

Set cache type back to Memory in `config.env` and redeploy the web application.
