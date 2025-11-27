# Script to create Kubernetes secrets from local env file
# This keeps secrets out of git while still being deployable

$ErrorActionPreference = "Stop"

$secretsFile = Join-Path $PSScriptRoot "secrets.env"
$namespace = "inkycal"
$secretName = "inkycal-secrets"

if (-not (Test-Path $secretsFile)) {
    Write-Error "secrets.env file not found at: $secretsFile"
    Write-Host "Please copy secrets.env.template to secrets.env and fill in your values"
    exit 1
}

Write-Host "Creating/updating Kubernetes secret '$secretName' in namespace '$namespace'..."

# Delete existing secret if it exists (ignore errors if it doesn't)
kubectl delete secret $secretName -n $namespace 2>$null

# Create secret from env file
kubectl create secret generic $secretName `
    --from-env-file=$secretsFile `
    --namespace=$namespace

if ($LASTEXITCODE -eq 0) {
    Write-Host "Secret '$secretName' created successfully!" -ForegroundColor Green
} else {
    Write-Error "Failed to create secret"
    exit 1
}
