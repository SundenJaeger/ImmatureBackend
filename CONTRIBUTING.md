# Contributing

# Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- A PostgreSQL database
- [Docker](https://docs.docker.com/get-started/get-docker/), running (required by the integration and performance tests, which start a temporary PostgreSQL container)
- [EF Core tools](https://learn.microsoft.com/en-us/ef/core/cli/dotnet) (`dotnet tool install --global dotnet-ef`)

# Configuration

Set the following environment variables (or use user secrets).

| Variable                     | Description                                         |
|------------------------------|-----------------------------------------------------|
| `Cloudinary__ApiKey`         | Cloudinary API key used to authenticate requests to the image service |
| `Cloudinary__ApiSecret`      | Cloudinary API secret used for secure API authentication and request signing |
| `Cloudinary__Cloudname`      | Name of the Cloudinary cloud account used for image storage and management |
| `Supabase__Url`              | Supabase project URL used to validate authentication token issuers |
| `Database__ConnectionString` | PostgreSQL connection string                        |
| `Sentry__Dsn`                | Sentry DSN for error reporting                      |
| `Swagger__Enabled`           | Set to `true` to expose Swagger outside Development |

# Building Locally

Apply the migrations and run the API:

```bash
dotnet restore
dotnet ef database update --project ImmatureBackend.Infrastructure --startup-project ImmatureBackend.Api
dotnet run --project ImmatureBackend.Api
```

The API runs on `http://localhost:5153`

# Running Tests

Docker must be running for the integration and performance tests (check with `docker ps`). They start a temporary
PostgreSQL container, apply the migrations and call the API in memory, so they don't need your own database, Supabase
or Cloudinary credentials and never touch real data. The first run pulls the `postgres:17-alpine` image.

| Project                            | What it covers                                                  |
|------------------------------------|-----------------------------------------------------------------|
| `Tests/ImmatureBackend.UnitTest`        | Services, validators and calculations in isolation              |
| `Tests/ImmatureBackend.IntegrationTest` | The real API and database (WebApplicationFactory + Testcontainers) |
| `Tests/ImmatureBackend.PerformanceTest` | Load tests with NBomber; reports are written to `nbomber-reports/` |

```bash
# Unit + integration tests (what CI runs)
dotnet test --filter "Category!=Performance"

# Only unit tests
dotnet test Tests/ImmatureBackend.UnitTest

# Only integration tests
dotnet test Tests/ImmatureBackend.IntegrationTest

# Only performance tests (takes about a minute per scenario; don't run other heavy work at the same time)
dotnet test --filter "Category=Performance"
```

`dotnet test` without a filter runs everything, including the performance tests.

# Docker

Build from the solution root:

```bash
docker build -f ImmatureBackend.Api/Dockerfile -t immature-backend .
docker run -p 8080:8080 \
  -e Database__ConnectionString="<connection string>" \
  -e Supabase__Url="<supabase project url>" \
  -e Cloudinary__Cloudname="<cloud name>" \
  -e Cloudinary__ApiKey="<api key>" \
  -e Cloudinary__ApiSecret="<api secret>" \
  -e Sentry__Dsn="<sentry dsn>" \
  immature-backend
```

See [Configuration](#configuration) for all variables. `Swagger__Enabled` is optional.

# Pull Requests

Tests run automatically on pushes to `master` and `dev`, and on pull requests to `master` and `dev`. Please make sure
`dotnet test --filter "Category!=Performance"` passes (with Docker running) before opening a pull request.