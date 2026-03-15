# Resume Event Demo

Minimal backend-first event-driven demo using:

- .NET 10 (producer + consumer)
- RabbitMQ
- CloudEvents 1.0
- Kubernetes (RabbitMQ + Consumer)
- Swagger UI (single interaction surface)

No database is used. Processed results are stored in-memory inside the consumer.

## Overview

This solution demonstrates the full event flow while staying intentionally small:

1. `POST /publish-resume-event` publishes a CloudEvent to RabbitMQ.
2. Consumer service reads `resume.events` and extracts skills.
3. Consumer stores result in memory (`Dictionary` equivalent store).
4. `GET /results/{eventId}` returns processing output.
5. `GET /health` reports dependency connectivity.

Swagger is the primary local interface at `http://localhost:8080/swagger`.

## Architecture

```text
Swagger UI (producer)
          |
producer (.NET 10)
          |
RabbitMQ (Kubernetes)
          |
Consumer Pod (.NET 10)
          |
Results Endpoint (/results/{eventId})
```

## Clean code structure

The producer project follows a lightweight DDD-style separation:

- `Api/` (HTTP layer)
- `Application/Producer/Contracts/` (application contracts + DTOs)
- `Application/Producer/Services/` (use-case services)
- `Domain/Producer/Events/` (CloudEvent + payload model and factory)
- `Infrastructure/Producer/Messaging/` (RabbitMQ publisher)
- all of these folders live inside the single `producer` project

The consumer project uses a similar separation:

- `Api/` (HTTP layer)
- `Domain/Consumer/Models/` (event + result models)
- `Domain/Consumer/Services/` (domain logic such as skill detection)
- `Infrastructure/Consumer/Persistence/` (in-memory result store)
- `Infrastructure/Producer/Messaging/` (RabbitMQ consumer worker)
- all of these folders live inside the single `consumer` project

## Project structure

```text
resume-delivery-express/
  resume-event-demo.slnx
  producer/
    Api/
    producer.csproj
    Application/
      Producer/
        Contracts/
        Services/
    Domain/
      Producer/
        Events/
    Infrastructure/
      Producer/
        Messaging/
  consumer/
    Api/
    consumer.csproj
    Domain/
      Consumer/
        Models/
        Services/
    Infrastructure/
      Consumer/
        Messaging/
        Persistence/
  tests/
    tests.csproj
    Producer/
    Consumer/
  infra/
    k8s/
      rabbitmq-deployment.yaml
      rabbitmq-service.yaml
      consumer-deployment.yaml
      consumer.yaml
```

## Endpoints (via Swagger)

Once running on `http://localhost:8080/swagger`, you can invoke:

- `POST /publish-resume-event`
- `GET /results/{eventId}`
- `GET /health`

## Local development setup

### Prerequisites

- .NET 10 SDK
- Docker Desktop or a running local Docker daemon
- Local Kubernetes cluster only (`docker-desktop`, `kind`, or `minikube`)
- kubectl

This demo is intentionally local-only. The scripts are designed to refuse non-local `kubectl` contexts so they do not touch shared or remote infrastructure.

### Quick start

From the repository root:

```powershell
.\scripts\start-demo.ps1
```

The script will stop immediately unless:

- Docker is running
- `kubectl` is pointed at a local context such as `docker-desktop`, `minikube`, or `kind-*`

If using `kind`, load the image into the cluster as part of startup:

```powershell
.\scripts\start-demo.ps1 -LoadToKind
```

The script does the following:

- builds the consumer Docker image
- optionally loads the image into `kind`
- applies the Kubernetes manifests
- waits for RabbitMQ and consumer rollouts
- starts port-forwards for RabbitMQ AMQP, RabbitMQ UI, and consumer results API
- starts a live consumer logs window
- opens the RabbitMQ queues page and producer Swagger
- runs the producer locally with the correct environment variables

After it starts, use:

- Swagger: `http://localhost:8080/swagger`
- RabbitMQ queues page: `http://localhost:15672/#/queues`
- Consumer results API: `http://localhost:5002/results/{eventId}`
- Stop script: `.\scripts\stop-demo.ps1`

Keep the extra PowerShell windows open while the demo is running.

### Expected demo flow

1. Run:

   ```powershell
   .\scripts\start-demo.ps1
   ```

2. Wait for Swagger to open at `http://localhost:8080/swagger`.
3. Execute `GET /health` and confirm the system reports healthy dependencies.
4. Execute `POST /publish-resume-event` and copy the returned `eventId`.
5. Watch the `resume-demo consumer logs` PowerShell window and confirm a message was processed.
6. Execute `GET /results/{eventId}` in producer Swagger using the same `eventId`.
7. Re-run `GET /results/{eventId}` until the result appears.
8. Confirm the returned JSON includes:
   - `candidateName`
   - `detectedSkills`
   - `processedAt`
   - `processedByPod`
9. Optionally watch queue activity in RabbitMQ at `http://localhost:15672/#/queues` while the demo is running.
10. When done, stop everything with:

   ```powershell
   .\scripts\stop-demo.ps1
   ```

### Manual setup

If you prefer to run each step yourself:

Make sure your current `kubectl` context is a local cluster before applying any manifests.

#### 1) Build consumer image

```powershell
docker build -t consumer:local .\consumer
```

If using kind:

```powershell
kind load docker-image consumer:local
```

#### 2) Deploy RabbitMQ + Consumer

```powershell
kubectl apply -f infra/k8s/namespace.yaml
kubectl apply -f infra/k8s/rabbitmq-deployment.yaml
kubectl apply -f infra/k8s/rabbitmq-service.yaml
kubectl apply -f infra/k8s/consumer-deployment.yaml
kubectl apply -f infra/k8s/consumer.yaml
```

#### 3) Port-forward Kubernetes services

RabbitMQ AMQP:

```powershell
kubectl port-forward -n resume-demo service/rabbitmq 5672:5672
```

RabbitMQ Management UI:

```powershell
kubectl port-forward -n resume-demo service/rabbitmq 15672:15672
```

Consumer results endpoint:

```powershell
kubectl port-forward -n resume-demo service/consumer 5002:8080
```

#### 4) Run backend locally

From repository root:

```powershell
$env:RABBITMQ_HOST='localhost'
$env:RABBITMQ_PORT='5672'
$env:CONSUMER_RESULTS_BASE_URL='http://localhost:5002'
$env:CONSUMER_RESULTS_HOST='localhost'
$env:CONSUMER_RESULTS_PORT='5002'
dotnet run --project .\producer\producer.csproj
```

This starts producer on `http://localhost:8080`.

## Verifying the system works

1. Run `.\scripts\start-demo.ps1`.
2. Open `http://localhost:8080/swagger`.
3. Execute `GET /health` and confirm the status is healthy.
4. Execute `POST /publish-resume-event` and capture `eventId`.
5. Execute `GET /results/{eventId}` until a result appears.
6. Open RabbitMQ UI at `http://localhost:15672` and verify queue activity.
7. Check consumer logs:

   ```powershell
   kubectl logs -n resume-demo deployment/consumer -f
   ```

8. Confirm response includes:
   - `candidateName`
   - `detectedSkills`
   - `processedAt`
   - `processedByPod`
9. Stop the demo:

   ```powershell
   .\scripts\stop-demo.ps1
   ```

## 30 Second Demo Walkthrough

1. Run `.\scripts\start-demo.ps1`.
2. Show `GET /health` in Swagger.
3. Call `POST /publish-resume-event`.
4. Show the live consumer logs window receiving and processing the message.
5. Call `GET /results/{eventId}` in Swagger and show processed output.
6. Highlight `detectedSkills` and `processedByPod`.
7. Optionally show RabbitMQ UI.

