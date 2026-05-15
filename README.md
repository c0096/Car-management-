# Vehicle Sale Orders

Fully dockerized monolithic ASP.NET Core MVC application for managing vehicle sale orders with SQL Server and ADO.NET only.

## Stack

- ASP.NET Core MVC on .NET 8
- SQL Server 2022
- ADO.NET through Microsoft.Data.SqlClient
- Docker and docker-compose
- Razor views with responsive admin styling

## Features

- Create, read, update, and delete vehicle sale orders
- Global search across order fields and attachment names
- Pagination, sorting, and page-size selection
- Multiple file uploads per order
- Downloadable PDF report per order
- Printable official-style order view
- Email and password authentication with protected order pages
- Frontend validation and backend validation with data annotations
- SQL schema initialization from the database container and application startup
- Product CRUD with category assignment
- Category CRUD from the Products page

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

The order form includes:

- Writer information: nom du rédacteur, numéro autorisation, téléphone, ville, date and time
- Seller information: nom, adresse, CIN, téléphone
- Sale order: sale statement and numéro d'ordre
- Vehicle information: type, marque, numéro châssis
- Buyer information: nom, adresse, CIN, téléphone
- Documents and metadata: titre de propriété, multiple attachments, observation
- Signatures: vendeur, gérant, acheteur

The catalogue area includes:

- Products: name, reference, description, category, unit price, and stock
- Categories: name and description

## Run with Docker

```bash
docker compose up --build
```

Open the application at:

```text
http://localhost:8080
```

Open Adminer for database browsing at:

```text
http://localhost:8081
```

Use these Adminer values:

```text
System: MS SQL
Server: db
Username: sa
Password: YourStrong!Passw0rd
Database: VehicleDeclarationsDb
```

Default application login:

```text
Email: admin@example.com
Password: Admin123!
```

Change these credentials before first startup by editing `Auth__DefaultEmail` and `Auth__DefaultPassword` in `docker-compose.yml`. The app creates the default user only when the user table is empty.

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
- `dbo.Users`
- `dbo.Orders`
- `dbo.OrderAttachments`
- `dbo.Products`
- `dbo.Categories`
- supporting indexes and foreign key constraints

The application also checks the required schema at startup for resilience and seeds the configured default login when no user exists.

## Local development without compose

Install the .NET 8 SDK and run SQL Server locally, then set the connection string:

```bash
export ConnectionStrings__DefaultConnection="Server=localhost,1433;Database=VehicleDeclarationsDb;User Id=sa;Password=YourStrong!Passw0rd;Encrypt=True;TrustServerCertificate=True"
export Auth__DefaultEmail="admin@example.com"
export Auth__DefaultPassword="Admin123!"
dotnet restore
dotnet run
```

## File uploads

Uploaded files are stored under:

```text
storage/uploads
```

In Docker, uploads are persisted in the `app-uploads` volume and downloaded through authenticated MVC actions.

## PDF reports

Each order detail page includes a PDF download action. The report is generated as a compact one-page A4 administrative form with order fields, attachment names, observations, and signature values.
