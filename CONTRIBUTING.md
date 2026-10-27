# Contributing

# Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- A PostgreSQL database
- [EF Core tools](https://learn.microsoft.com/en-us/ef/core/cli/dotnet) (`dotnet tool install --global dotnet-ef`)

# Configuration

Set the following environment variables (or use user secrets).

| Variable                     | Description                                         |
|------------------------------|-----------------------------------------------------|
| `API_KEY`                    | Key clients send in the `X-API-Key` header          |
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

The API runs on `http://localhost:5153`. In Development, Swagger UI is available at `/swagger`.

# Running Tests

```bash
dotnet test
```

# Docker

Build from the solution root:

```bash
docker build -f ImmatureBackend.Api/Dockerfile -t immature-backend .
docker run -p 8080:8080 -e API_KEY=your-key -e Database__ConnectionString="<connection string>" immature-backend
```

# Pull Requests

Tests run automatically on pushes to `master` and `dev`, and on pull requests to `master`. Please make sure
`dotnet test` passes before opening a pull request.