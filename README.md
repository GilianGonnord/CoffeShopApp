# Coffee Shop Management System

A modern ASP.NET Core MVC application for managing coffee shop inventory, built with Entity Framework Core and featuring role-based access control with multiple authentication options.

## Features

### 🔐 Authentication & Authorization
- **Local Authentication**: Username/password authentication with BCrypt password hashing
- **External Authentication**: Unified support for OIDC and SAML protocols
  - **Microsoft Authentication (OIDC)**: OAuth integration with Microsoft accounts
  - **SAML 2.0 Authentication**: Enterprise SSO with SAML identity providers
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
- **External Auth**: OpenID Connect (OIDC) and SAML 2.0 support
- **SAML Library**: Sustainsys.Saml2.AspNetCore2
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

### External Authentication Configuration

The application supports both OIDC and SAML authentication protocols. You can configure one of them (not both simultaneously) in your `appsettings.json`:

#### Basic Structure

```json
{
  "Authentication": {
    "External": {
      "Protocol": "OIDC",  // or "SAML" or "None"
      "Oidc": {
        // OIDC configuration (required if Protocol is "OIDC")
      },
      "Saml": {
        // SAML configuration (required if Protocol is "SAML")
      }
    }
  }
}
```

#### OIDC Configuration (Microsoft Authentication)

```json
{
  "Authentication": {
    "External": {
      "Protocol": "OIDC",
      "Oidc": {
        "ClientId": "your-microsoft-client-id",
        "ClientSecret": "your-microsoft-client-secret",
        "Authority": "https://login.microsoftonline.com/common/v2.0",
        "AdditionalScopes": ["User.Read"]
      }
    }
  }
}
```

**Setting up Microsoft OIDC Authentication:**

1. Register your app in [Azure App Registrations](https://portal.azure.com/#blade/Microsoft_AAD_RegisteredApps/ApplicationsListBlade)
2. Configure redirect URI: `https://localhost:5001/signin-external`
3. Add the Client ID and Client Secret to your configuration
4. Users must have existing accounts in the system (matched by email)

#### SAML Configuration

```json
{
  "Authentication": {
    "External": {
      "Protocol": "SAML",
      "Saml": {
        "EntityId": "https://your-app-domain.com",
        "MetadataUrl": "https://your-idp.com/saml/metadata",
        "CertificatePath": "/path/to/certificate.pfx",
        "CertificatePassword": "cert-password",
        "SignRequests": false,
        "RequireSignedResponses": true
      }
    }
  }
}
```

**Setting up SAML Authentication:**

1. Configure your SAML Identity Provider (IdP) with:
   - **Entity ID**: Your application's entity ID (e.g., your domain)
   - **ACS URL**: `https://your-domain/Saml2/Acs`
   - **Single Logout URL**: `https://your-domain/Saml2/Logout`
2. Add the IdP metadata URL to your configuration
3. Optionally configure signing certificates for enhanced security
4. Users must have existing accounts in the system (matched by email)

### Configuration Options

#### Authentication Protocols

- **None**: Only local authentication (default if no configuration provided)
- **OIDC**: OpenID Connect authentication (typically Microsoft)
- **SAML**: SAML 2.0 authentication for enterprise SSO

#### OIDC Options

| Property | Description | Required | Default |
|----------|-------------|----------|---------|
| `ClientId` | OAuth client ID | Yes | - |
| `ClientSecret` | OAuth client secret | Yes | - |
| `Authority` | OIDC authority endpoint | No | Microsoft common endpoint |
| `AdditionalScopes` | Extra scopes to request | No | `[]` |

#### SAML Options

| Property | Description | Required | Default |
|----------|-------------|----------|---------|
| `EntityId` | Your application's SAML entity ID | Yes | - |
| `MetadataUrl` | IdP metadata URL | Yes | - |
| `CertificatePath` | Path to signing certificate | No | - |
| `CertificatePassword` | Certificate password | No | - |
| `SignRequests` | Sign SAML requests | No | `false` |
| `RequireSignedResponses` | Require signed responses | No | `true` |

## Authentication Flow

### Local Authentication
1. User enters username/password
2. System validates credentials against database
3. User is signed in with cookie authentication

### External Authentication (OIDC/SAML)
1. User clicks external login button
2. User is redirected to external provider
3. User authenticates with external provider
4. External provider redirects back to application
5. System extracts email from external claims
6. System looks up user by email in database
7. If user exists, they are signed in with cookie authentication
8. If user doesn't exist, authentication fails with error message

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
- `POST /Account/Login` - Process local login
- `GET /Account/Register` - Registration page
- `POST /Account/Register` - Process registration
- `GET /Account/Logout` - Logout
- `GET /Account/LoginWithExternal` - External authentication (OIDC/SAML)
- `GET /Account/ExternalCallback` - External authentication callback

### Coffee Management
- `GET /Coffee` - List all coffees
- `GET /Coffee/Details/{id}` - Coffee details
- `GET /Coffee/Create` - Create coffee form (Manager only)
- `POST /Coffee/Create` - Process coffee creation (Manager only)
- `GET /Coffee/Edit/{id}` - Edit coffee form (Manager only)
- `POST /Coffee/Edit/{id}` - Process coffee update (Manager only)
- `GET /Coffee/Delete/{id}` - Delete confirmation (Manager only)
- `POST /Coffee/Delete/{id}` - Process coffee deletion (Manager only)

### SAML Endpoints (Auto-configured when SAML is enabled)
- `GET /Saml2/Acs` - Assertion Consumer Service
- `POST /Saml2/Acs` - Assertion Consumer Service (POST binding)
- `GET /Saml2/Logout` - Single Logout endpoint
- `GET /Saml2/Metadata` - Service Provider metadata

## Troubleshooting

### External Authentication Issues

1. **"External authentication is not configured"**
   - Verify your `appsettings.json` has the correct `Authentication:External` section
   - Ensure required fields are filled based on your chosen protocol

2. **"User not found" after successful external login**
   - The user's email from the external provider must match an existing user in the database
   - Check that the email claim is being correctly extracted from the external provider

3. **SAML authentication fails**
   - Verify the IdP metadata URL is accessible
   - Check that your application's entity ID matches what's configured in your IdP
   - Ensure callback URLs are correctly configured in your IdP

4. **OIDC authentication fails**
   - Verify Client ID and Client Secret are correct
   - Check that redirect URIs are properly configured in your OAuth provider
   - Ensure the authority URL is correct

### Configuration Validation

The application validates external authentication configuration at startup and will throw detailed error messages if:
- Protocol is set but required configuration is missing
- Invalid protocol values are specified
- Required fields are empty for the chosen protocol

## Support

For support, please open an issue in the GitHub repository or contact the development team.

---

**Happy Brewing! ☕**