# Resume Event Demo

Minimal backend-first event-driven demo using:

- .NET 10 (Producer API + Consumer Service)
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
Swagger UI (Producer API)
          |
Producer API (.NET 10)
          |
RabbitMQ (Kubernetes)
          |
Consumer Pod (.NET 10)
          |
Results Endpoint (/results/{eventId})
```

## Clean code structure

The Producer API now follows a lightweight DDD-style separation:

- `Controllers/` (HTTP layer)
- `Application/` (use-case services + contracts)
- `Infrastructure/` (RabbitMQ and outbound service integration)
- `Domain` objects (`ResumeEventFactory`, CloudEvent payload models)

Consumer service exposes its HTTP endpoint through a controller and keeps processing concerns in dedicated services.

## Project structure

```text
resume-delivery-express/
  Program.cs                       # root runner (dotnet run)
  resume-event-demo.csproj
  resume-event-demo.slnx
  producer-api/
    Controllers/
    Application/
    Infrastructure/
  consumer-service/
    Controllers/
  consumer-service.tests/
  producer-api.tests/
  infra/
    k8s/
      rabbitmq-deployment.yaml
      rabbitmq-service.yaml
      consumer-deployment.yaml
      consumer-service.yaml
```

## Endpoints (via Swagger)

Once running on `http://localhost:8080/swagger`, you can invoke:

- `POST /publish-resume-event`
- `GET /results/{eventId}`
- `GET /health`

## Local development setup

### Prerequisites

- .NET 10 SDK
- Docker
- Kubernetes cluster (Docker Desktop Kubernetes, kind, or minikube)
- kubectl

### 1) Build consumer image

```bash
docker build -t consumer-service:local ./consumer-service
```

If using kind:

```bash
kind load docker-image consumer-service:local
```

### 2) Deploy RabbitMQ + Consumer

```bash
kubectl apply -f infra/k8s/rabbitmq-deployment.yaml
kubectl apply -f infra/k8s/rabbitmq-service.yaml
kubectl apply -f infra/k8s/consumer-deployment.yaml
kubectl apply -f infra/k8s/consumer-service.yaml
```

### 3) Port-forward Kubernetes services

RabbitMQ AMQP:

```bash
kubectl port-forward service/rabbitmq 5672:5672
```

RabbitMQ Management UI:

```bash
kubectl port-forward service/rabbitmq 15672:15672
```

Consumer results endpoint:

```bash
kubectl port-forward service/consumer-service 5002:8080
```

### 4) Run backend locally

From repository root:

```bash
dotnet run
```

This starts Producer API on `http://localhost:8080`.

## Verifying the system works

1. Open `http://localhost:8080/swagger`.
2. Execute `POST /publish-resume-event` and capture `eventId`.
3. Execute `GET /results/{eventId}` until a result appears.
4. Open RabbitMQ UI at `http://localhost:15672` and verify queue activity.
5. Check consumer logs:

   ```bash
   kubectl get pods -l app=consumer-service
   kubectl logs <consumer-pod>
   ```

6. Confirm response includes:
   - `detectedSkills`
   - `processedByPod`

## 30 Second Demo Walkthrough

1. Open Swagger (`/swagger`).
2. Call `POST /publish-resume-event`.
3. Show RabbitMQ queue count in management UI.
4. Show consumer pod logs receiving/processing the event.
5. Call `GET /results/{eventId}` and show processed output.
6. Highlight `processedByPod` returning Kubernetes pod name.
