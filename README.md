# Coffee Shop Management System

A modern ASP.NET Core MVC application for managing coffee shop inventory, built with Entity Framework Core and featuring role-based access control.

## Features

### 🔐 Authentication & Authorization
- **Local Authentication**: Username/password authentication with BCrypt password hashing
- **Microsoft Authentication**: Optional OAuth integration with Microsoft accounts
- **Role-Based Access Control**: Manager, Barista, and Customer roles with different permissions
- **Claims-Based Authorization**: Fine-grained permissions using custom claims

### ☕ Coffee Management
- **CRUD Operations**: Create, read, update, and delete coffee products
- **Product Details**: Name, description, price, origin, roast level, and availability
- **Inventory Status**: Track coffee availability and stock status
- **Rich UI**: Beautiful, responsive coffee-themed interface

### 👥 User Management
- **User Registration**: Self-service account creation
- **Profile Management**: User profiles with email and username
- **Role Assignment**: Admin-controlled role and permission assignment

## Technology Stack

- **Framework**: ASP.NET Core MVC (.NET 10.0)
- **Database**: SQLite with Entity Framework Core
- **Authentication**: ASP.NET Core Identity with Cookie Authentication
- **OAuth**: OpenID Connect for Microsoft authentication
- **Password Hashing**: BCrypt.Net
- **Frontend**: Bootstrap 5.3, Font Awesome icons
- **Styling**: Custom CSS with coffee-themed design

## Quick Start

### Prerequisites
- .NET 10.0 SDK
- Visual Studio 2022 or VS Code (optional)

### Installation

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd CoffeeShopApp
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Run the application**
   ```bash
   dotnet run
   ```

4. **Access the application**
   - Open your browser to `https://localhost:5001`
   - The database will be automatically created on first run

### Demo Accounts

The application comes with pre-seeded demo accounts:

| Role | Username | Password | Permissions |
|------|----------|----------|-------------|
| Manager | `admin` | `admin123` | Full coffee management + viewing |
| Barista | `barista` | `barista123` | Coffee viewing only |

## Configuration

### Database Configuration

The application uses SQLite by default. Connection string in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=coffeeshop.db"
  }
}
```

### Microsoft Authentication (Optional)

To enable Microsoft OAuth authentication, add the following to your `appsettings.json` or user secrets:

```json
{
  "Authentication": {
    "Microsoft": {
      "ClientId": "your-microsoft-client-id",
      "ClientSecret": "your-microsoft-client-secret"
    }
  }
}
```

**Setting up Microsoft Authentication:**

1. Register your app in [Azure App Registrations](https://portal.azure.com/#blade/Microsoft_AAD_RegisteredApps/ApplicationsListBlade)
2. Configure redirect URI: `https://localhost:5001/signin-microsoft`
3. Add the Client ID and Client Secret to your configuration
4. Users must have existing accounts in the system (matched by email)

## Permissions System

The application uses a claims-based authorization system:

### Available Claims
- `CanViewCoffee`: View coffee menu and details
- `CanManageCoffee`: Create, edit, and delete coffee products
- `IsBarista`: Barista role indicator
- `IsManager`: Manager role indicator

### Default Permissions
- **New Users**: `CanViewCoffee` only
- **Barista**: `CanViewCoffee` + `IsBarista`
- **Manager**: All permissions

## Development

### Running Migrations

```bash
# Add a new migration
dotnet ef migrations add <MigrationName>

# Update the database
dotnet ef database update
```

### Building for Production

```bash
# Publish the application
dotnet publish -c Release -o ./publish

# Run the published application
cd publish
dotnet CoffeeShopApp.dll
```

## API Endpoints

### Authentication
- `GET /Account/Login` - Login page
- `POST /Account/Login` - Process login
- `GET /Account/Register` - Registration page
- `POST /Account/Register` - Process registration
- `GET /Account/Logout` - Logout
- `GET /Account/LoginWithMicrosoft` - Microsoft OAuth login

### Coffee Management
- `GET /Coffee` - List all coffees
- `GET /Coffee/Details/{id}` - Coffee details
- `GET /Coffee/Create` - Create coffee form (Manager only)
- `POST /Coffee/Create` - Process coffee creation (Manager only)
- `GET /Coffee/Edit/{id}` - Edit coffee form (Manager only)
- `POST /Coffee/Edit/{id}` - Process coffee update (Manager only)
- `GET /Coffee/Delete/{id}` - Delete confirmation (Manager only)
- `POST /Coffee/Delete/{id}` - Process coffee deletion (Manager only)

## Support

For support, please open an issue in the GitHub repository or contact the development team.

---

**Happy Brewing! ☕**