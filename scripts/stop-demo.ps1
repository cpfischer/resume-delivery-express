$ErrorActionPreference = 'Stop'

$runtimePath = Join-Path $PSScriptRoot '.runtime'
$processFile = Join-Path $runtimePath 'demo-processes.json'
$repoRoot = Split-Path -Parent $PSScriptRoot
$k8sPath = Join-Path $repoRoot 'infra\k8s'

function Stop-ProcessTree {
    param([int]$ProcessId)

    try {
        taskkill /PID $ProcessId /T /F | Out-Null
    }
    catch {
        Stop-Process -Id $ProcessId -Force -ErrorAction SilentlyContinue
    }
}

function Test-LocalKubectlContext {
    $currentContext = kubectl config current-context 2>$null
    if (-not $currentContext) {
        Write-Host 'kubectl does not have a current context configured. Skipping Kubernetes cleanup.'
        return $null
    }

    $allowedPatterns = @(
        '^docker-desktop$',
        '^minikube$',
        '^kind-.+$',
        '^kind$'
    )

    foreach ($pattern in $allowedPatterns) {
        if ($currentContext -match $pattern) {
            return $currentContext
        }
    }

    Write-Host "Refusing Kubernetes cleanup against non-local kubectl context '$currentContext'."
    return $null
}

function Stop-TrackedProcesses {
    if (-not (Test-Path $processFile)) {
        Write-Host 'No tracked helper processes were found.'
        return
    }

    $processes = Get-Content $processFile -Raw | ConvertFrom-Json
    if ($processes -isnot [System.Array]) {
        $processes = @($processes)
    }

    foreach ($process in $processes) {
        try {
            $runningProcess = Get-Process -Id $process.Id -ErrorAction Stop
            Write-Host "Stopping helper process $($runningProcess.Id)..."
            Stop-ProcessTree -ProcessId $runningProcess.Id
        }
        catch {
            Write-Host "Helper process $($process.Id) is already stopped."
        }
    }

    Remove-Item $processFile -ErrorAction SilentlyContinue
}

function Remove-KubernetesResources {
    if (-not (Get-Command kubectl -ErrorAction SilentlyContinue)) {
        Write-Host 'kubectl not found. Skipping Kubernetes cleanup.'
        return
    }

    $currentContext = Test-LocalKubectlContext
    if (-not $currentContext) {
        return
    }

    Write-Host "Removing Kubernetes resources from local context '$currentContext'..."
    kubectl delete -f (Join-Path $k8sPath 'consumer.yaml') --ignore-not-found
    kubectl delete -f (Join-Path $k8sPath 'consumer-deployment.yaml') --ignore-not-found
    kubectl delete -f (Join-Path $k8sPath 'rabbitmq-service.yaml') --ignore-not-found
    kubectl delete -f (Join-Path $k8sPath 'rabbitmq-deployment.yaml') --ignore-not-found
    kubectl delete -f (Join-Path $k8sPath 'namespace.yaml') --ignore-not-found
}

Stop-TrackedProcesses
Remove-KubernetesResources

Write-Host ''
Write-Host 'Demo environment stopped.'
