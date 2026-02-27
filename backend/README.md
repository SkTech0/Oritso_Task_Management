# Task Manager API (Backend)

## Run without Docker (`dotnet run`)

### 1. PostgreSQL

The API uses PostgreSQL. Either:

- **Option A – PostgreSQL installed locally**  
  Create database and user (e.g. in `psql`):

  ```sql
  CREATE USER taskuser WITH PASSWORD 'taskpassword';
  CREATE DATABASE taskmanagerdb OWNER taskuser;
  ```

- **Option B – PostgreSQL in Docker (DB only)**  

  ```bash
  docker run -d --name taskmanager-db -p 5432:5432 \
    -e POSTGRES_USER=taskuser \
    -e POSTGRES_PASSWORD=taskpassword \
    -e POSTGRES_DB=taskmanagerdb \
    postgres:15
  ```

### 2. Configuration

For local development, configuration is read from **`appsettings.Development.json`** when `ASPNETCORE_ENVIRONMENT=Development` (default when you run the API).

That file is set up for:

- **Connection:** `Host=localhost;Port=5432;Database=taskmanagerdb;Username=taskuser;Password=taskpassword`

To use a different database, either:

- Edit `TaskManager.API/appsettings.Development.json` and set `ConnectionStrings:DefaultConnection`, or  
- Set the connection string in environment:

  ```bash
  export ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=yourdb;Username=user;Password=pass"
  ```

(Optional) For production-like runs, copy `appsettings.example.json` to `appsettings.json` and set a real JWT secret and auth salt.

### 3. Run the API

From the repo root:

```bash
cd backend
dotnet run --project TaskManager.API
```

Or from the API project:

```bash
cd backend/TaskManager.API
dotnet run
```

- **URL:** `http://localhost:5001` (Development) or `http://localhost:8080` if `PORT` is set.  
- **Swagger:** `http://localhost:5001/swagger` (or the port you use).

Migrations run automatically in Development; the DB is created/updated and seed data applied.

### 4. Frontend

Point the frontend API base URL to this backend, e.g. in `frontend/src/environments/environment.ts`:

- `apiBaseUrl: 'http://localhost:5001/api'` (or the port the API is using).
