# Script to create Kubernetes ConfigMap from local env file

$ErrorActionPreference = "Stop"

$configFile = Join-Path $PSScriptRoot "config.env"
$namespace = "inkycal"
$configMapName = "inkycal-config"

if (-not (Test-Path $configFile)) {
    Write-Error "config.env file not found at: $configFile"
    Write-Host "Please copy config.env.template to config.env and fill in your values"
    exit 1
}

Write-Host "Creating/updating Kubernetes ConfigMap '$configMapName' in namespace '$namespace'..."

# Delete existing configmap if it exists (ignore errors if it doesn't)
kubectl delete configmap $configMapName -n $namespace 2>$null

# Create configmap from env file
kubectl create configmap $configMapName `
    --from-env-file=$configFile `
    --namespace=$namespace

if ($LASTEXITCODE -eq 0) {
    Write-Host "ConfigMap '$configMapName' created successfully!" -ForegroundColor Green
} else {
    Write-Error "Failed to create ConfigMap"
    exit 1
}
