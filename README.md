# TradeTrackr API

## Overview
TradeTrackr API is a backend service for managing clients, jobs, and invoices for trade professionals. It is built with ASP.NET Core (.NET 8) and uses PostgreSQL for data storage. The API supports secure authentication via Auth0 and provides a RESTful interface for integration with frontend applications.

## Features
- Manage clients, jobs, and invoices
- JWT-based authentication (Auth0)
- PostgreSQL database with Entity Framework Core
- OpenAPI/Swagger documentation
- CORS support for frontend integration
- Comprehensive unit and integration tests

## Technology Stack
- ASP.NET Core (.NET 8)
- Entity Framework Core (PostgreSQL)
- Auth0 (JWT authentication)
- Swashbuckle (Swagger/OpenAPI)
- Mapster (object mapping)
- xUnit, Moq, FluentAssertions (testing)

## Getting Started

### Prerequisites
- .NET 8 SDK
- PostgreSQL database
- Auth0 account (for authentication)

### Configuration
1. **Database Connection**: Set the PostgreSQL connection string in `TradetrackrDb:ConnectionStrings` (e.g., in `appsettings.Development.json` or user secrets).
2. **Auth0 Settings**: Update the Auth0 `Authority` and `Audience` in `Program.cs` to match your Auth0 tenant and API identifier.
3. **CORS**: Adjust allowed origins in `Program.cs` as needed for your frontend.

### Running the API
```sh
dotnet build
dotnet ef database update # Apply migrations
dotnet run
```
The API will be available at `https://localhost:44395` (or your configured port).

### API Documentation
Swagger UI is available in development mode at `/swagger`.

## API Overview

### Authentication
All endpoints require JWT authentication via Auth0. Include the access token in the `Authorization` header:
```
Authorization: Bearer {token}
```

### Endpoints

#### Clients
- `GET /api/clients` — List all clients for the authenticated user
- `GET /api/clients/{id}` — Get a specific client
- `POST /api/clients` — Create a new client
- `PUT /api/clients/{id}` — Update a client
- `DELETE /api/clients/{id}` — Delete a client

#### Jobs
- `GET /api/jobs` — List all jobs for the authenticated user
- `GET /api/jobs/{id}` — Get a specific job
- `POST /api/jobs` — Create a new job
- `PUT /api/jobs/{id}` — Update a job
- `DELETE /api/jobs/{id}` — Delete a job

#### Invoices
- `GET /api/invoices` — List all invoices for the authenticated user
- `GET /api/invoices/{id}` — Get a specific invoice
- `POST /api/invoices` — Create a new invoice
- `PUT /api/invoices/{id}` — Update an invoice
- `DELETE /api/invoices/{id}` — Delete an invoice

## Project Structure
- `Controllers/` — API controllers
- `Models/` — Entity models
- `Dto/` — Data transfer objects
- `Data/` — Entity Framework DbContext
- `Migrations/` — Database migrations
- `tradetrackr.api.tests/` — Test project

## Testing
Run tests with:
```sh
dotnet test
```

## Contribution Guidelines
- Use modern C# and .NET 8 features
- Write clear, maintainable code
- Add or update tests for new features
- Follow the existing project structure and naming conventions

## License
This project is licensed for internal use. See LICENSE file if present.
