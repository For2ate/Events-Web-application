[Читать на русском](README_RU.md)

## Project Description: EventApp API

**EventApp API** is the backend for a web application designed for event management, built on the **.NET 9** platform. The application provides a  REST-like API for creating, viewing, updating, and deleting events, event categories, and handling user registrations for these events.

**Core Features:**

*   **Event Management:** CRUD operations for events (name, description, date, place, max participants, image, category).
*   **Category Management:** CRUD operations for event categories.
*   **User Registration:** User registration/cancellation for events with validation checks (available slots, event date, duplicate registrations).
*   **List Retrieval:** Ability to retrieve a list of events with filtering (by date, place, category) and pagination. Retrieval of event participant lists.
*   **Authentication:** REST-like API, JWT Bearer authentication is implemented to secure endpoints.
*   **File Uploads:** Capability to upload images for events (stored locally in `wwwroot/images` with unique names).
*   **Database:** Utilizes PostgreSQL for data storage.
*   **Migrations:** Employs Entity Framework Core migrations for database schema management (with an option for auto-apply on startup during development).

**Technology Stack:**

*   **Platform:** .NET 9 / ASP.NET Core
*   **ORM:** Entity Framework Core 9
*   **Database:** PostgreSQL (using `Npgsql.EntityFrameworkCore.PostgreSQL` driver)
*   **API & Services:** RESTful API, JWT Bearer Authentication (`Microsoft.AspNetCore.Authentication.JwtBearer`), AutoMapper, FluentValidation
*   **API Documentation:** Scalar (OpenAPI/Swagger via `Microsoft.AspNetCore.OpenApi` and `Scalar.AspNetCore`)
*   **Logging:** Serilog (with Console and File sinks)
*   **File Handling:** Local Storage (`wwwroot`)
*   **Containerization:** Docker, Docker Compose
*   **Testing:** xUnit, Moq, AutoFixture (with AutoMoq), FluentAssertions, EF Core InMemory Provider, Coverlet (Code Coverage via `coverlet.collector`)

**Architecture:**

*   The application utilizes the Repository Pattern for data access, a Service Layer for business logic, and Controllers for handling API requests.

---

## Project Setup and Running Instructions (using Docker Compose)

These instructions assume the project will be run locally using Docker and Docker Compose.

**1. Prerequisites:**

*   **Docker:** Docker Desktop (Windows, macOS) or Docker Engine/Docker Compose (Linux) installed and running. [Official Docker Website](https://www.docker.com/products/docker-desktop/)
*   **.NET SDK:** .NET SDK (version 9 or the primary version used in the project) installed, required for certificate management (`dotnet dev-certs`). [Official .NET Website](https://dotnet.microsoft.com/download)
*   **Git:** For cloning the repository.

**2. Setup and Run:**

1.  **Clone the repository:**
    ```bash
    git clone https://github.com/For2ate/Events-Web-application.git
    cd Events-Web-application
    ```

2.  **Configure HTTPS Development Certificate:**
    The Docker container will serve HTTPS. For local development, you need to create, export, and trust the certificate:

    *   **Export the certificate to PFX format** (for Kestrel inside the container):
        ```powershell
        # In PowerShell (Windows) - ensure the .aspnet\https folder exists
        dotnet dev-certs https -ep $env:USERPROFILE\.aspnet\https\EventApi.pfx -p pass
        ```
        ```bash
        # In Bash (Linux/macOS) - ensure the ~/.aspnet/https folder exists
        dotnet dev-certs https -ep ${HOME}/.aspnet/https/EventApi.pfx -p pass
        ```
        *   `-ep`: Export path. The `EventApi.pfx` file will be created in the standard ASP.NET Core certificate folder in your home directory.
        *   `-p pass`: Sets the password `pass` for the PFX file (this password is used in `docker-compose.yml`). **The filename (`EventApi.pfx`) and password (`pass`) must exactly match** those specified in the `ASPNETCORE_Kestrel__Certificates__Default__Path` and `ASPNETCORE_Kestrel__Certificates__Default__Password` environment variables in `docker-compose.yml`.

    *   **Trust the certificate on the host machine** (so your browser doesn't show warnings):
        ```powershell
        # In PowerShell or Command Prompt/Terminal (Windows/Linux/macOS)
        dotnet dev-certs https --trust
        ```
        (You might need to confirm the certificate installation in the system's trusted store).

3.  **Create and configure the `.env` file:**
    *   In the project's root folder (next to `docker-compose.yml`), create a file named `.env`.
    *   Copy the following content into it, **making sure to replace `YourSuperSecretKeyHere...` with your actual strong JWT secret**:

        ```dotenv
        # .env file

        # Database Secrets (can be left as is if the password matches docker-compose)
        POSTGRES_USER=postgres
        POSTGRES_PASSWORD=mysecretpassword # Ensure this matches the db service password in docker-compose
        POSTGRES_DB=Event.Web.App
        DB_HOST=db
        DB_PORT=5432
        DB_CONNECTION_STRING=Host=${DB_HOST};Port=${DB_PORT};Database=${POSTGRES_DB};Username=${POSTGRES_USER};Password=${POSTGRES_PASSWORD}

        # IMPORTANT: Set your real JWT secret key
        JWT_SECRET_KEY=YourSuperSecretKeyHereWhichIsVeryLongAndSecure123!@#

        # Password for the PFX certificate (must match export command and docker-compose)
        CERT_PASSWORD=pass
        ```

4.  **Build and run the containers:**
    Open a terminal in the project's root folder (where `docker-compose.yml` is located) and run:
    ```bash
    docker-compose up --build -d
    ```
    *   `up`: Starts the services defined in `docker-compose.yml`.
    *   `--build`: Forces a rebuild of the images before starting (important after code or Dockerfile changes).
    *   `-d`: Runs the containers in detached mode (in the background).

    The first run might take some time as Docker downloads base images and builds your project.

**3. Accessing the Application:**

*   **API HTTP:** `http://localhost:5274` (Port 5274 on the host maps to 8080 in the container)
*   **API HTTPS:** `https://localhost:7026` (Port 7026 on the host maps to 8081 in the container)
*   **API Documentation (Scalar UI):** Open `https://localhost:7026/scalar` in your browser (HTTPS recommended). You can also use `http://localhost:5274/scalar`.
*   **Database (if needed):** PostgreSQL is accessible on the host at `localhost:54399` (Port 54399 maps to 5432 in the container). Use the username `postgres` and the password from your `.env` file.

**4. Stopping the Application:**

To stop and remove the containers, run the following command in the same terminal/folder:

    ```bash
    docker-compose down
    ```

   * This command stops and removes the containers but does not delete the PostgreSQL data volume (postgres_data).
   * If you want to remove the database volume as well (e.g., for a complete reset), use:

    ```bash
    docker-compose down -v
    ```

**Additional Notes:**

* Ensure the ports specified in docker-compose.yml (e.g., 5274, 7026, 54399) are not already in use by other applications on your machine.
* Check container logs if you encounter issues during startup: docker-compose logs api or docker-compose logs db.