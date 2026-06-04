# TaskFlow — Backend (API)

[← Back to project overview](../../README.md)

ASP.NET Core 10 REST API following Clean Architecture and CQRS. Uses an in-memory database so no database engine is required to run locally.

---

## Table of Contents

- [Prerequisites](#prerequisites)
- [Running Locally](#running-locally)
- [Running the Tests](#running-the-tests)

---

## Prerequisites

| Tool | Version | Download |
|------|---------|----------|
| .NET SDK | **10.0** | https://dotnet.microsoft.com/download/dotnet/10.0 |
| Git | Any recent | https://git-scm.com |

Verify your installation:

```bash
dotnet --version
# Expected: 10.0.x
```

> **HTTPS dev certificate** — the API runs on HTTPS locally. If you have never trusted the .NET dev certificate on this machine, run the following once and accept the security prompt:
>
> ```bash
> dotnet dev-certs https --trust
> ```

---

## Running Locally

```bash
# 1. Navigate to the solution root
cd api/TaskFlow

# 2. Restore NuGet packages (first time or after dependency changes)
dotnet restore

# 3. Start the API
dotnet run --project src/TaskFlow.Api/TaskFlow.Api.csproj
```

The API listens on:

| Protocol | URL |
|----------|-----|
| HTTPS | `https://localhost:7045` |
| HTTP  | `http://localhost:5121`  |

Once running:

| Resource | URL |
|----------|-----|
| Interactive docs (Scalar UI) | `https://localhost:7045/scalar/v1` |
| OpenAPI JSON spec | `https://localhost:7045/openapi/v1.json` |
| Health check | `https://localhost:7045/api/healthcheck/ping` |

> Data is stored **in memory** and resets on every restart. No connection strings or migrations are needed.

---

## Running the Tests

The test project uses **xUnit 2.9** with **NSubstitute 5.3** for mocking and **coverlet** for coverage.

```bash
# From the solution root
cd api/TaskFlow

# Run all tests
dotnet test

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run with code coverage report
dotnet test --collect:"XPlat Code Coverage"
```

Coverage results are written to `TestResults/` as a Cobertura XML file. Open it with a tool like ReportGenerator or upload it to a coverage service.

### What is covered

| Area | Test file |
|------|-----------|
| `TaskTitle` value object validation | `TaskTitleTests.cs` |
| `TaskItem` domain entity logic | `TaskItemTests.cs` |
| `Create` command handler | `CreateHandlerTests.cs` |
| `Update` command handler | `UpdateHandlerTests.cs` |
| `Patch` (toggle status) handler | `PatchHandlerTests.cs` |
| `Delete` command handler | `DeleteHandlerTests.cs` |
| `GetAll` query handler | `GetAllHandlerTests.cs` |
| `GetById` query handler | `GetByIdHandlerTests.cs` |
| `TaskRepository` data access | `TaskRepositoryTests.cs` |
| `TasksController` HTTP layer | `TasksControllerTests.cs` |
| `TaskLinker` HATEOAS links | `TaskLinkerTests.cs` |
| `PaginationLinksHelper` | `PaginationLinksHelperTests.cs` |

---

[← Back to project overview](../../README.md)
