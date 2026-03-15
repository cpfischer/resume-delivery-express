# Resume Event Demo
To view a video demonstration, click here: https://youtu.be/SMvhYABorZ8

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

The producer project follows a lightweight separation:

- `Api/` for HTTP endpoints
- `Application/Contracts/` for request/response models and interfaces
- `Application/Services/` for application orchestration
- `Domain/Factories/` for CloudEvent creation
- `Infrastructure/Producer/` for RabbitMQ publishing
- `Infrastructure/Swagger/` for Swagger example/default payload configuration

The consumer project uses a similar separation:

- `Api/` for HTTP endpoints
- `Domain/Models/` for CloudEvent and result models
- `Domain/Services/` for domain logic such as skill detection
- `Infrastructure/Consumer/` for the RabbitMQ background worker
- `Infrastructure/Persistence/` for the in-memory result store

## Project structure

```text
resume-delivery-express/
  resume-event-demo.slnx
  producer/
    Api/
    producer.csproj
    Application/
      Contracts/
      Services/
    Domain/
      Factories/
    Infrastructure/
      Producer/
      Swagger/
  consumer/
    Api/
    consumer.csproj
    Domain/
      Models/
      Services/
    Infrastructure/
      Consumer/
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
  scripts/
    start-demo.ps1
    stop-demo.ps1
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
- PowerShell 5.1+ or PowerShell 7+

### Pre-demo setup

Before starting the demo, make sure all of the following are true:

- Docker Desktop is running
- your local Kubernetes cluster is running
- `kubectl config current-context` points to a local context such as `docker-desktop`, `minikube`, `kind`, or `kind-*`
- port `8080` is free for the producer Swagger/API
- port `5002` is free for the consumer results port-forward
- port `5672` is free for the RabbitMQ AMQP port-forward
- port `15672` is free for the RabbitMQ management UI port-forward

Optional but recommended before a demo run:

```powershell
dotnet test
```

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
- tracks the helper processes so `.\scripts\stop-demo.ps1` can shut them down cleanly

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
4. Expand `POST /publish-resume-event` and use the default CloudEvent payload shown in Swagger.
5. Execute `POST /publish-resume-event` and copy the returned `eventId`.
6. Note that the returned `eventId` is server-generated as a GUID.
7. Watch the `resume-demo consumer logs` PowerShell window and confirm a message was processed.
8. Execute `GET /results/{eventId}` in producer Swagger using the same `eventId`.
9. Re-run `GET /results/{eventId}` until the result appears.
10. Confirm the returned JSON includes:
   - `eventId`
   - `candidateName`
   - `detectedSkills`
   - `processedAt`
   - `processedByPod`
11. Optionally watch queue activity in RabbitMQ at `http://localhost:15672/#/queues` while the demo is running.
12. When done, stop everything with:

   ```powershell
   .\scripts\stop-demo.ps1
   ```

### Swagger request payload

`POST /publish-resume-event` accepts a CloudEvent-shaped request body. Swagger pre-populates a default payload for you.

The request `id` is only a placeholder to keep the body shaped like a CloudEvent. The actual published event ID is generated by the producer as a GUID and returned in the response.

Example request body:

```json
{
  "id": "00000000-0000-0000-0000-000000000000",
  "source": "/producer/resume-events",
  "type": "com.resume.submitted",
  "specversion": "1.0",
  "time": "2026-03-15T21:00:00.0000000+00:00",
  "datacontenttype": "application/json",
  "data": {
    "candidateName": "Caleb Fischer",
    "targetRole": "Software Engineer",
    "resumeText": "Kubernetes RabbitMQ .NET AWS Grafana Microservices"
  }
}
```

## 30 Second Demo Walkthrough

1. Run `.\scripts\start-demo.ps1`.
2. Show `GET /health` in Swagger.
3. Call `POST /publish-resume-event`.
4. Show the live consumer logs window receiving and processing the message.
5. Call `GET /results/{eventId}` in Swagger and show processed output.
6. Highlight `detectedSkills` and `processedByPod`.
7. Optionally show RabbitMQ UI.

