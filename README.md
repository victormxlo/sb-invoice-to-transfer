# SB.InvoiceToTransfer

Backend service responsible for issuing invoices, confirming payments, and executing transfers using Stark Bank APIs.

The system is designed with **Clean Architecture** principles and a strong focus on **correctness, idempotency, and resilience**, ensuring **exactly-once transfer execution**, even in the presence of retries, webhook replays, or partial failures.

---

## Overview

This service handles the full lifecycle of a paid invoice:

- Invoice creation in Stark Bank  
- Payment confirmation via webhook  
- Transfer of the paid amount (minus fees) to a recipient  
- Guaranteed exactly-once transfer execution  
- Safe recovery from crashes or partial failures  

It is a **backend-only service**, composed of an API layer and background jobs.

---

## Business Workflow

### Invoice Issuance
A scheduled job periodically issues invoices to Stark Bank.

### Payment Confirmation
Stark Bank notifies the system via `invoice.paid` webhook events.

### Transfer Execution
Once payment is confirmed, the system calculates the net amount and creates a transfer.

### Recovery From Partial Failures
If the process crashes after persisting payment data but before executing the transfer, a recovery job retries **only** invoices in the `Processing` state.

---

## Architecture

The project follows Clean Architecture, with strict separation of concerns:

### Layers
- **API**: ASP.NET Core Web API  
- **Application**: Use cases and orchestration (MediatR)  
- **Domain**: Entities, invariants, and business rules  
- **Infrastructure**: Persistence, external clients, background jobs  

### Main Components
- Webhook endpoint (`/api/webhook`)  
- Invoice issuance scheduler  
- Invoice processing (recovery) background job  

---

## Invoice Lifecycle

### Creation
- Invoice is created locally  
- Amount is stored in **major units (reais)**  
- Converted to **minor units (cents)** before sending to Stark Bank  

### Payment (Webhook)
- Webhook payload is validated  
- Invoice state transitions: `Created → Processing`  
- `AmountPaid` and `Fee` are persisted  

### Transfer
- Transfer is executed **only if `TransferId` is null**  
- Invoice state transitions: `Processing → Paid`  

### Recovery
- Background job processes invoices in `Processing`  
- Transfers are retried safely and idempotently  

---

## Financial Model

### Amount
- Original invoice value  
- Stored in **major units**  
- Immutable after creation  

### AmountPaid & Fee
- Received from Stark Bank in **minor units (cents)**  
- Converted to **major units** for storage and calculations  

### Rationale
Stark Bank uses minor units to avoid floating-point precision errors.  
This system stores values in major units for clarity and reporting, with all conversions centralized in the domain to ensure correctness and consistency.

---

## Idempotency & Safety

### Guarantees
- No duplicate transfers  
- Safe retries  
- Crash recovery  

### Mechanisms
- Explicit and persisted state transitions  
- Transfer creation guarded by `TransferId == null`  
- Recovery job processes only `Processing` invoices  
- Webhook replays are ignored for already `Paid` invoices  

---

## Scheduling

### Invoice Issuance Scheduler
- Runs on a configurable schedule  
- Default interval: **3 hours**  

### Invoice Processing (Recovery) Job
- Interval: **01:00:00**  
- Processes invoices stuck in `Processing`  

---

## Environment Variables

The application is fully configured via environment variables.  
**All variables are required.**

| Key | Description | Example |
|---|---|---|
| `SB_DB_CONNECTION` | SQLite file path (path only) | `/app/data/invoice_to_transfer.db` |
| `SB_ENVIRONMENT` | Stark Bank environment | `sandbox` |
| `SB_PROJECT_ID` | Stark Bank project ID | `...` |
| `SB_PRIVATE_KEY` | Stark Bank private key (multiline) | `-----BEGIN EC PRIVATE KEY----- ...` |

**Note:**  
`SB_DB_CONNECTION` is used in the code as:  
`Data Source={SB_DB_CONNECTION}`

---

## Running Locally

### Prerequisites
- .NET 9 SDK

### Run with .NET CLI
```bash
cd src/SB.InvoiceToTransfer.Api

export SB_DB_CONNECTION="/app/data/invoice_to_transfer.db"
export SB_ENVIRONMENT="sandbox"
export SB_PROJECT_ID="..."
export SB_PRIVATE_KEY="-----BEGIN EC PRIVATE KEY----- ..."

dotnet run
```

### Run with Docker
```bash
docker build -t sb-invoice-to-transfer .

docker run -p 8080:8080 \
  -e SB_DB_CONNECTION="/app/data/invoice_to_transfer.db" \
  -e SB_ENVIRONMENT="sandbox" \
  -e SB_PROJECT_ID="..." \
  -e SB_PRIVATE_KEY="-----BEGIN EC PRIVATE KEY----- ..." \
  sb-invoice-to-transfer
```
---

## API Endpoints

### Health Check

    GET /api/health

### Swagger

    GET /swagger

---

## Deployment (Render)

- Create a **Web Service**
- Runtime: **Docker**
- Port: **8080**
- Health Check Path: `/api/health`
- Dockerfile Path: `Dockerfile`

All environment variables must be configured in Render, exactly as in local execution.

---

## Observability

- Console logging with structured logs
- Correlation via `ExternalId`
- Logs for:
  - State transitions
  - Background job execution

---

## Limitations & Improvements

### Current Limitations

- SQLite persistence is ephemeral in cloud environments
- No distributed locking for background jobs
- No metrics or alerting

### Possible Improvements

- Migrate to PostgreSQL
- Add distributed locking (e.g., Redis)
- Implement retries with exponential backoff
- Add OpenTelemetry metrics and traces
- Introduce a `Money` value object in the domain

---

## Notes

This project prioritizes **correctness, resilience, and clarity**.  
All design decisions aim to ensure safe financial processing, strict idempotency, and reliable recovery under failure scenarios.