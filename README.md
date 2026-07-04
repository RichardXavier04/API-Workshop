# Card Transactions API

This solution was developed as a production-style RESTful ASP.NET Core Web API for a take-home assessment. It implements all four functional requirements and includes automated unit and integration tests.

---

## Overview

The Card Transactions API manages card credit limits, records card transactions, and supports currency conversion using the US Treasury Reporting Rates of Exchange API.

The solution implements the following requirements:

- Create a Card
- Create a Transaction
- Retrieve a Transaction in a Specified Currency
- Retrieve the Available Balance of a Card in a Specified Currency

Cards and transactions are stored in SQL Server. Currency conversion uses Treasury exchange rates, with historical rates used for transaction retrieval and the latest available rates used for balance retrieval.

---

## Solution Architecture

The solution follows a layered architecture with clear separation of responsibilities.

| Project | Responsibility |
|----------|----------------|
| **CardTransactions.Api** | HTTP endpoints, request/response handling, application startup and dependency injection |
| **CardTransactions.Business** | Business rules, service interfaces and implementations |
| **CardTransactions.Data** | Entity Framework Core DbContext, entities, repositories and Treasury API client |
| **CardTransactions.Contracts** | Request and response DTOs shared across layers |
| **CardTransactions.Business.Tests** | Unit tests covering business logic |
| **CardTransactions.IntegrationTests** | End-to-end integration tests covering the full application pipeline |

### Dependency Flow

```
API
    ↓
Business
    ↓
Data
```

Dependency Injection is configured through `AddBusiness()` and `AddData()`. The Repository Pattern is used through `ICardRepository` and `ITransactionRepository`.

---

## Technologies

- C#
- .NET 10
- ASP.NET Core
- Entity Framework Core
- SQL Server
- Docker
- xUnit
- Moq
- Microsoft.AspNetCore.Mvc.Testing

---

## Prerequisites

Before running the application, ensure the following are installed:

- .NET 10 SDK
- Docker Desktop

---

## Running the Application

Run the following commands from the `CardTransactions` directory.

### 1. Start SQL Server

```powershell
docker compose up -d
```

### 2. Restore packages

```powershell
dotnet restore
```

### 3. Build the solution

```powershell
dotnet build
```

### 4. Apply database migrations

```powershell
dotnet ef database update --project CardTransactions.Data --startup-project CardTransactions.Api
```

### 5. Run the API

```powershell
dotnet run --project CardTransactions.Api
```

The API is available at:

```
http://localhost:5038
```

The endpoints can be tested using an HTTP client such as **Insomnia**.

---

## Automated Testing

### Unit Tests

Unit tests validate business rules in isolation by mocking external dependencies such as repositories and the Treasury API client. They cover input validation, currency conversion logic, and balance calculation.

```powershell
dotnet test CardTransactions.Business.Tests
```

### Integration Tests

Integration tests verify the complete application pipeline:

```
API
    ↓
Business
    ↓
Data
    ↓
SQL Server
```

A fake Treasury client (`FakeTreasuryExchangeRateClient`) is used so tests remain deterministic and do not depend on external services.

Tests run against a dedicated database (`CardTransactions_IntegrationTests`) configured in `appsettings.Testing.json`. EF Core migrations are applied automatically when the integration test host starts.

**Prerequisite:** Docker SQL Server must be running.

```powershell
dotnet test CardTransactions.IntegrationTests
```

---

## API Endpoints

| Requirement | Endpoint |
|-------------|----------|
| Create Card | `POST /api/v1/cards` |
| Create Transaction | `POST /api/v1/cards/{cardId}/transactions` |
| Retrieve Transaction | `GET /api/v1/transactions/{transactionId}?currency={currency}` |
| Retrieve Card Balance | `GET /api/v1/cards/{cardId}/balance?currency={currency}` |

---

## Project Structure

```text
CardTransactions/
├── CardTransactions.Api/               # Web API controllers and application entry point
├── CardTransactions.Business/          # Business services and interfaces
├── CardTransactions.Data/              # EF Core, repositories, Treasury client and migrations
├── CardTransactions.Contracts/         # Request and response models
├── CardTransactions.Business.Tests/    # Business layer unit tests
├── CardTransactions.IntegrationTests/  # End-to-end API integration tests
├── docker-compose.yml                  # SQL Server container for local development
└── CardTransactions.slnx               # Solution file
```