param(
    [switch]$LoadToKind
)

$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$namespace = 'resume-demo'
$consumerImage = 'consumer:local'
$producerProject = Join-Path $repoRoot 'producer\producer.csproj'
$k8sPath = Join-Path $repoRoot 'infra\k8s'
$runtimePath = Join-Path $PSScriptRoot '.runtime'
$processFile = Join-Path $runtimePath 'demo-processes.json'

function Require-Command {
    param([string]$Name)

    if (-not (Get-Command $Name -ErrorAction SilentlyContinue)) {
        throw "Required command '$Name' was not found in PATH."
    }
}

function Test-LocalKubectlContext {
    $currentContext = kubectl config current-context 2>$null
    if (-not $currentContext) {
        throw 'kubectl does not have a current context configured.'
    }

    $allowedPatterns = @(
        '^docker-desktop$',
        '^minikube$',
        '^kind-.+$',
        '^kind$'
    )

    $isAllowed = $false
    foreach ($pattern in $allowedPatterns) {
        if ($currentContext -match $pattern) {
            $isAllowed = $true
            break
        }
    }

    if (-not $isAllowed) {
        throw "Refusing to run against kubectl context '$currentContext'. Use a local demo context such as docker-desktop, minikube, or kind-*."
    }

    return $currentContext
}

function Test-DockerDaemon {
    docker info | Out-Null
}

function Start-PortForwardWindow {
    param(
        [string]$Title,
        [string]$Arguments
    )

    $command = "`$Host.UI.RawUI.WindowTitle = '$Title'; kubectl $Arguments"
    return Start-Process powershell -ArgumentList '-NoExit', '-Command', $command -PassThru
}

function Start-KubectlWindow {
    param(
        [string]$Title,
        [string]$Arguments
    )

    $command = "`$Host.UI.RawUI.WindowTitle = '$Title'; kubectl $Arguments"
    return Start-Process powershell -ArgumentList '-NoExit', '-Command', $command -PassThru
}

function Start-ProducerWindow {
    param(
        [string]$Title
    )

    $producerCommand = @"
`$Host.UI.RawUI.WindowTitle = '$Title'
`$env:RABBITMQ_HOST = 'localhost'
`$env:RABBITMQ_PORT = '5672'
`$env:CONSUMER_RESULTS_BASE_URL = 'http://localhost:5002'
`$env:CONSUMER_RESULTS_HOST = 'localhost'
`$env:CONSUMER_RESULTS_PORT = '5002'
dotnet run --project '$producerProject'
"@

    return Start-Process powershell -ArgumentList '-NoExit', '-Command', $producerCommand -PassThru
}

function Wait-ForUrl {
    param(
        [string]$Url,
        [int]$TimeoutSeconds = 30
    )

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)

    while ((Get-Date) -lt $deadline) {
        try {
            Invoke-WebRequest -Uri $Url -UseBasicParsing -TimeoutSec 2 | Out-Null
            return $true
        }
        catch {
            Start-Sleep -Seconds 1
        }
    }

    return $false
}

Require-Command docker
Require-Command kubectl
Require-Command dotnet

New-Item -ItemType Directory -Path $runtimePath -Force | Out-Null

Write-Host 'Checking Docker daemon...'
Test-DockerDaemon

$currentKubectlContext = Test-LocalKubectlContext
Write-Host "Using local kubectl context: $currentKubectlContext"

Write-Host 'Building consumer image...'
docker build -t $consumerImage (Join-Path $repoRoot 'consumer')

if ($LoadToKind) {
    Require-Command kind
    Write-Host 'Loading consumer image into kind...'
    kind load docker-image $consumerImage
}

Write-Host 'Applying Kubernetes manifests...'
kubectl apply -f (Join-Path $k8sPath 'namespace.yaml')
kubectl apply -f (Join-Path $k8sPath 'rabbitmq-deployment.yaml')
kubectl apply -f (Join-Path $k8sPath 'rabbitmq-service.yaml')
kubectl apply -f (Join-Path $k8sPath 'consumer-deployment.yaml')
kubectl apply -f (Join-Path $k8sPath 'consumer.yaml')

Write-Host 'Waiting for RabbitMQ rollout...'
kubectl rollout status deployment/rabbitmq -n $namespace --timeout=120s

Write-Host 'Waiting for consumer rollout...'
kubectl rollout status deployment/consumer -n $namespace --timeout=120s

Write-Host 'Starting port-forwards in new PowerShell windows...'
$helperProcesses = @(
    (Start-PortForwardWindow -Title 'resume-demo rabbitmq amqp' -Arguments "port-forward -n $namespace service/rabbitmq 5672:5672")
    (Start-PortForwardWindow -Title 'resume-demo rabbitmq ui' -Arguments "port-forward -n $namespace service/rabbitmq 15672:15672")
    (Start-PortForwardWindow -Title 'resume-demo consumer results' -Arguments "port-forward -n $namespace service/consumer 5002:8080")
)

Write-Host 'Starting consumer logs in a new PowerShell window...'
$helperProcesses += Start-KubectlWindow -Title 'resume-demo consumer logs' -Arguments "logs -n $namespace deployment/consumer -f"

$helperProcesses |
    Select-Object Id, StartTime, ProcessName |
    ConvertTo-Json |
    Set-Content -Path $processFile

Write-Host 'Opening demo URLs...'
if (Wait-ForUrl -Url 'http://localhost:15672' -TimeoutSeconds 45) {
    Start-Process 'http://localhost:15672/#/queues' | Out-Null
}
else {
    Write-Host 'RabbitMQ management UI did not become available within 45 seconds.'
}

Write-Host 'Starting producer in a new PowerShell window...'
$producerProcess = Start-ProducerWindow -Title 'resume-demo producer'

$helperProcesses += $producerProcess

$helperProcesses |
    Select-Object Id, StartTime, ProcessName |
    ConvertTo-Json |
    Set-Content -Path $processFile

Write-Host 'Waiting for Swagger to become available...'
if (Wait-ForUrl -Url 'http://localhost:8080/swagger' -TimeoutSeconds 45) {
    Start-Process 'http://localhost:8080/swagger' | Out-Null
}
else {
    Write-Host 'Swagger did not become available within 45 seconds. The producer window may show the startup error.'
}

Write-Host ''
Write-Host 'Demo flow:'
Write-Host '1. Open POST /publish-resume-event in Swagger and execute it.'
Write-Host '2. Copy the returned eventId.'
Write-Host '3. Open GET /results/{eventId} in Swagger and paste the eventId.'
Write-Host '4. Re-run GET /results/{eventId} until a result appears.'
Write-Host '5. Watch the consumer logs window to verify the message was consumed.'
Write-Host '6. Confirm the result includes candidateName, detectedSkills, processedAt, and processedByPod.'
Write-Host "7. When you are done, run .\scripts\stop-demo.ps1 in another PowerShell window."
Write-Host ''
