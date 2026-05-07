# Vehicle Sale Declarations

Fully dockerized monolithic ASP.NET Core MVC application for managing vehicle sale declarations with SQL Server and ADO.NET only.

## Stack

- ASP.NET Core MVC on .NET 8
- SQL Server 2022
- ADO.NET through Microsoft.Data.SqlClient
- Docker and docker-compose
- Razor views with responsive admin styling

## Features

- Create, read, update, and delete vehicle sale declarations
- Global search across declaration fields and attachment names
- Pagination, sorting, and page-size selection
- Multiple file uploads per declaration
- Downloadable PDF report per declaration
- Printable official-style declaration view
- Frontend validation and backend validation with data annotations
- SQL schema initialization from the database container and application startup

## Project layout

```text
Controllers/
Db/
Entity/
Repository/
Service/
Views/
wwwroot/
db/
Dockerfile
docker-compose.yml
```

The backend follows the requested separation:

- `Db` contains SQL Server connection and schema initialization
- `Entity` contains domain models and shared data structures
- `Repository` contains ADO.NET queries
- `Service` contains business logic, upload handling, and PDF generation
- `Controllers` contains MVC route handlers that delegate to services

## Data captured

The declaration form includes:

- Writer information: nom du rédacteur, numéro autorisation, téléphone, ville, date and time
- Seller information: nom, adresse, CIN, téléphone
- Sale declaration: déclaration text and numéro d'ordre
- Vehicle information: type, marque, numéro châssis
- Buyer information: nom, adresse, CIN, téléphone
- Documents and metadata: titre de propriété, multiple attachments, observation
- Signatures: vendeur, gérant, acheteur

## Run with Docker

```bash
docker compose up --build
```

Open the application at:

```text
http://localhost:8080
```

SQL Server is exposed at:

```text
localhost,1433
```

Default database credentials used by docker-compose:

```text
Database: VehicleDeclarationsDb
User: sa
Password: YourStrong!Passw0rd
```

## Database setup

The database container builds from `db/Dockerfile` and runs `db/init.sql` on first startup. The script creates:

- `VehicleDeclarationsDb`
- `dbo.VehicleSaleDeclarations`
- `dbo.DeclarationAttachments`
- supporting indexes and foreign key constraints

The application also checks the required schema at startup for resilience.

## Local development without compose

Install the .NET 8 SDK and run SQL Server locally, then set the connection string:

```bash
export ConnectionStrings__DefaultConnection="Server=localhost,1433;Database=VehicleDeclarationsDb;User Id=sa;Password=YourStrong!Passw0rd;Encrypt=True;TrustServerCertificate=True"
dotnet restore
dotnet run
```

## File uploads

Uploaded files are stored under:

```text
wwwroot/uploads
```

In Docker, uploads are persisted in the `app-uploads` volume.

## PDF reports

Each declaration detail page includes a PDF download action. The report contains all declaration fields, attachment names, observations, and signature values in an administrative form layout.
