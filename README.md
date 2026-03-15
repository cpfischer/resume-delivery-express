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
- Docker
- Kubernetes cluster (Docker Desktop Kubernetes, kind, or minikube)
- kubectl

### 1) Build consumer image

```bash
docker build -t consumer:local ./consumer
```

If using kind:

```bash
kind load docker-image consumer:local
```

### 2) Deploy RabbitMQ + Consumer

```bash
kubectl apply -f infra/k8s/namespace.yaml
kubectl apply -f infra/k8s/rabbitmq-deployment.yaml
kubectl apply -f infra/k8s/rabbitmq-service.yaml
kubectl apply -f infra/k8s/consumer-deployment.yaml
kubectl apply -f infra/k8s/consumer.yaml
```

### 3) Port-forward Kubernetes services

RabbitMQ AMQP:

```bash
kubectl port-forward service/rabbitmq 5672:5672
```

Add `-n resume-demo` if your current kubectl context is not already set to that namespace.

RabbitMQ Management UI:

```bash
kubectl port-forward service/rabbitmq 15672:15672
```

Add `-n resume-demo` if needed.

Consumer results endpoint:

```bash
kubectl port-forward service/consumer 5002:8080
```

Add `-n resume-demo` if needed.

### 4) Run backend locally

From repository root:

```bash
dotnet run
```

This starts producer on `http://localhost:8080`.

## Verifying the system works

1. Open `http://localhost:8080/swagger`.
2. Execute `POST /publish-resume-event` and capture `eventId`.
3. Execute `GET /results/{eventId}` until a result appears.
4. Open RabbitMQ UI at `http://localhost:15672` and verify queue activity.
5. Check consumer logs:

   ```bash
   kubectl get pods -n resume-demo -l app=consumer
   kubectl logs -n resume-demo <consumer-pod>
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

