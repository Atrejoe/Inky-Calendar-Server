# Script to generate a Bitnami SealedSecret from the local env file.
#
# Unlike apply-secrets.ps1 (which creates a plain Secret directly in the
# cluster), this encrypts the values with the cluster's public key using
# `kubeseal`. The resulting SealedSecret is safe to commit to git: only the
# Sealed Secrets controller running in the target cluster can decrypt it.
#
# Prerequisites:
#   - kubectl, pointed at the target cluster
#   - kubeseal (https://github.com/bitnami-labs/sealed-secrets/releases)
#   - The Sealed Secrets controller installed in the cluster
#
# Usage:
#   ./seal-secrets.ps1                      # write sealed-secrets.yaml
#   ./seal-secrets.ps1 -Apply               # write and kubectl apply it

param(
    # Apply the generated SealedSecret to the cluster after creating it.
    [switch]$Apply,
    # Name/namespace of the Sealed Secrets controller (defaults match the
    # upstream Helm chart / manifest install).
    [string]$ControllerName = "sealed-secrets-controller",
    [string]$ControllerNamespace = "kube-system"
)

$ErrorActionPreference = "Stop"

$secretsFile = Join-Path $PSScriptRoot "secrets.env"
$outputFile = Join-Path $PSScriptRoot "sealed-secrets.yaml"
$namespace = "inkycal"
$secretName = "inkycal-secrets"

if (-not (Test-Path $secretsFile)) {
    Write-Error "secrets.env file not found at: $secretsFile"
    Write-Host "Please copy secrets.env.template to secrets.env and fill in your values"
    exit 1
}

# Verify kubeseal is available before doing anything else.
if (-not (Get-Command kubeseal -ErrorAction SilentlyContinue)) {
    Write-Error "kubeseal not found. Install it from https://github.com/bitnami-labs/sealed-secrets/releases"
    exit 1
}

Write-Host "Generating SealedSecret '$secretName' for namespace '$namespace'..."

# 1. Render a plain Secret manifest locally (never sent to the cluster).
# 2. Pipe it through kubeseal, which fetches the controller's public key and
#    encrypts the values so only the controller can decrypt them.
kubectl create secret generic $secretName `
    --from-env-file=$secretsFile `
    --namespace=$namespace `
    --dry-run=client -o yaml |
    kubeseal `
        --controller-name=$ControllerName `
        --controller-namespace=$ControllerNamespace `
        --format yaml > $outputFile

if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to generate SealedSecret"
    exit 1
}

Write-Host "SealedSecret written to '$outputFile' - this file is safe to commit to git." -ForegroundColor Green

if ($Apply) {
    Write-Host "Applying SealedSecret to the cluster..."
    kubectl apply -f $outputFile
    if ($LASTEXITCODE -eq 0) {
        Write-Host "SealedSecret applied successfully!" -ForegroundColor Green
    } else {
        Write-Error "Failed to apply SealedSecret"
        exit 1
    }
}
