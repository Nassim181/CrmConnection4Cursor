# Dynamics 365 CRM Online Connector

A C# console application to connect to Dynamics 365 CRM Online using the Dataverse SDK.

## Prerequisites

- .NET 8.0 SDK or later
- Dynamics 365 CRM Online instance
- Valid user credentials with access to the Dynamics 365 organization

## Setup

1. **Clone or download this project**

2. **Configure Connection String**

   Edit `appsettings.json` and update the connection string with your Dynamics 365 details:

   ```json
   {
     "ConnectionStrings": {
       "Dataverse": "AuthType=OAuth;Url=https://yourorg.crm.dynamics.com;Username=yourusername@yourorg.onmicrosoft.com;Password=yourpassword;AppId=51f81489-12ee-4a9e-aaae-a2591f45987d;RedirectUri=http://localhost;LoginPrompt=Auto"
     }
   }
   ```

   Replace:
   - `yourorg.crm.dynamics.com` - Your Dynamics 365 organization URL
   - `yourusername@yourorg.onmicrosoft.com` - Your username
   - `yourpassword` - Your password

3. **Alternative: Use Environment Variables**

   You can also set the connection string as an environment variable:
   ```powershell
   $env:ConnectionStrings__Dataverse = "AuthType=OAuth;Url=https://yourorg.crm.dynamics.com;..."
   ```

## Authentication Methods

### OAuth (Username/Password)
```
AuthType=OAuth;Url=https://yourorg.crm.dynamics.com;Username=user@domain.com;Password=password;AppId=51f81489-12ee-4a9e-aaae-a2591f45987d;RedirectUri=http://localhost;LoginPrompt=Auto
```

### Client Secret (Recommended for Production)
```
AuthType=ClientSecret;Url=https://yourorg.crm.dynamics.com;ClientId=your-client-id;ClientSecret=your-client-secret
```

### Certificate Authentication
```
AuthType=Certificate;Url=https://yourorg.crm.dynamics.com;ClientId=your-client-id;CertificateThumbprint=your-thumbprint;CertificateStoreName=My
```

## Running the Application

1. **Restore NuGet packages:**
   ```bash
   dotnet restore
   ```

2. **Build the project:**
   ```bash
   dotnet build
   ```

3. **Run the application:**
   ```bash
   dotnet run
   ```

## What the Program Does

1. Connects to Dynamics 365 CRM Online using the configured connection string
2. Displays connection information (Organization name, ID, User ID, Service URL)
3. Retrieves and displays the first 5 account records
4. Creates a test account record

## Features

- ✅ OAuth authentication support
- ✅ Client Secret authentication support
- ✅ Certificate authentication support
- ✅ Retrieve multiple records
- ✅ Create records
- ✅ Error handling and logging

## Troubleshooting

### Common Issues

1. **Authentication Failed**
   - Verify your username and password are correct
   - Ensure your account has access to the Dynamics 365 organization
   - Check if MFA is enabled (may require interactive login)

2. **Connection Timeout**
   - Verify the organization URL is correct
   - Check your network connection
   - Ensure firewall allows connections to Dynamics 365

3. **Permission Errors**
   - Ensure your user has appropriate security roles
   - Verify you have read/write permissions for the entities you're accessing

## Additional Resources

- [Microsoft Power Platform Dataverse Client Documentation](https://learn.microsoft.com/en-us/dotnet/api/microsoft.powerplatform.dataverse.client)
- [Connection String Format](https://learn.microsoft.com/en-us/power-apps/developer/data-platform/xrm-tooling/use-connection-strings-xrm-tooling-connect)
- [Dynamics 365 Web API](https://learn.microsoft.com/en-us/dynamics365/customer-engagement/web-api/)

## License

This project is provided as-is for educational and development purposes.

