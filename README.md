# Movies API Backend

This project is the backend for the Movies Application, built using ASP.NET Web API with a layered architecture. It manages the core functionality, including endpoints, data processing, authentication, and integration with the database. The backend ensures robust and scalable communication with the frontend application.

---

## Features

### Core Functionality:
- **RESTful Endpoints**: Provides endpoints for managing movies, reviews, wishlists, and more.
- **Authentication**: Implements JWT-based authentication for secure login and registration.
- **Entity Framework Integration**: Manages database interactions efficiently.
- **Layered Architecture**: Follows a clean architecture pattern with separation of concerns.

### Layers Overview:
1. **`Movies.API`**: Handles the API layer with controllers managing HTTP endpoints.
2. **`Movies.Application`**: Contains repositories, mappers, models, and business logic.
3. **`Movies.Contracts`**: Defines request and response models for consistent data transfer.
4. **`Movies.Identity`**: Responsible for creating and managing JWT tokens for authentication.
5. **`Movies.Application.Test`**: Contains unit tests and mock implementations to ensure code quality.

---

## Getting Started

### Prerequisites

Make sure you have the following installed on your system:
- [Visual Studio](https://visualstudio.microsoft.com/) (with .NET 8 SDK)
- SQL Server (or an equivalent database management system)

### Installation

1. Clone the repository:
   ```bash
   git clone
   ```

2. Open the solution in Visual Studio:
   ```bash
   cd Movies-App
   ```

3. Set up the database:
   - Update the connection string in `appsettings.json` to match your database.
   - Apply migrations:
     ```bash
     dotnet ef database update
     ```

4. Run the application:
   ```bash
   dotnet run --project Movies.API
   ```
   The API will be available at `http://localhost:5000`.

---

## Technologies Used

- **Framework**: ASP.NET Web API (.NET 8)
- **Database**: SQL Server with Entity Framework
- **Authentication**: JWT Tokens
- **Testing**: nUnit, Moq
- **Architecture**: Layered Architecture dds

---

## Live Demo

You can test the live application here: [Movies App](https://moviesfrontend.azurewebsites.net/)

---

## Contributing

Contributions are welcome! To contribute:
1. Fork the repository.
2. Create a new branch for your feature/fix:
   ```bash
   git checkout -b feature-name
   ```
3. Commit your changes:
   ```bash
   git commit -m "Add feature-name"
   ```
4. Push to the branch:
   ```bash
   git push origin feature-name
   ```
5. Open a pull request.

