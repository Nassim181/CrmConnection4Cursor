using Microsoft.Extensions.Configuration;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System.Linq;

namespace Dynamics365Connector;

class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            // Load configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            // Build connection string from configuration
            var dynamicsConfig = configuration.GetSection("Dynamics365");
            var url = dynamicsConfig["Url"] ?? throw new InvalidOperationException("Dynamics365:Url not found in configuration.");
            var username = dynamicsConfig["Username"] ?? throw new InvalidOperationException("Dynamics365:Username not found in configuration.");
            var password = dynamicsConfig["Password"] ?? throw new InvalidOperationException("Dynamics365:Password not found in configuration.");
            var authType = dynamicsConfig["AuthType"] ?? "OAuth";
            var redirectUri = dynamicsConfig["RedirectUri"] ?? "http://localhost";
            var loginPrompt = dynamicsConfig["LoginPrompt"] ?? "Auto";
            var appId = dynamicsConfig["AppId"];

            var connectionString = $"AuthType={authType};Url={url};Username={username};Password={password};RedirectUri={redirectUri};LoginPrompt={loginPrompt}";
            
            // AppId is optional - if not specified, the SDK uses the default public client AppId
            // Default AppId: 51f81489-12ee-4a9e-aaae-a2591f45987d (Microsoft's public client)
            if (!string.IsNullOrWhiteSpace(appId))
            {
                connectionString += $";AppId={appId}";
                Console.WriteLine($"Using custom AppId: {appId}");
            }
            else
            {
                Console.WriteLine("Using default public client AppId: 51f81489-12ee-4a9e-aaae-a2591f45987d");
            }

            Console.WriteLine("Connecting to Dynamics 365 CRM Online...");
            Console.WriteLine($"Organization URL: {url}");
            Console.WriteLine($"Username: {username}");
            Console.WriteLine($"Connection String: {connectionString.Replace("Password=", "Password=***")}");

            // Create service client
            ServiceClient? serviceClient = null;
            try
            {
                serviceClient = new ServiceClient(connectionString);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Exception during connection creation:");
                Console.WriteLine($"  Type: {ex.GetType().Name}");
                Console.WriteLine($"  Message: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"  Inner Exception: {ex.InnerException.Message}");
                    if (ex.InnerException.InnerException != null)
                    {
                        Console.WriteLine($"  Inner Inner Exception: {ex.InnerException.InnerException.Message}");
                    }
                }
                Console.WriteLine($"  Stack Trace: {ex.StackTrace}");
                return;
            }

            if (serviceClient == null)
            {
                Console.WriteLine("✗ Failed to create service client.");
                return;
            }

            using (serviceClient)
            {
                if (serviceClient.IsReady)
                {
                    Console.WriteLine("✓ Successfully connected to Dynamics 365 CRM!");
                    Console.WriteLine($"  Organization: {serviceClient.ConnectedOrgUniqueName}");
                    Console.WriteLine($"  Organization ID: {serviceClient.ConnectedOrgId}");
                    Console.WriteLine($"  Service URL: {serviceClient.ConnectedOrgPublishedEndpoints?.Values.FirstOrDefault() ?? "N/A"}");

                    // Example: Retrieve account records
                    await RetrieveAccountsAsync(serviceClient);

                    // Example: Create a test account
                    await CreateTestAccountAsync(serviceClient);
                }
                else
                {
                    Console.WriteLine("✗ Failed to connect to Dynamics 365 CRM.");
                    Console.WriteLine($"  Last Error: {serviceClient.LastError}");
                    if (serviceClient.LastException != null)
                    {
                        Console.WriteLine($"  Exception: {serviceClient.LastException.Message}");
                        Console.WriteLine($"  Exception Type: {serviceClient.LastException.GetType().Name}");
                        
                        // Check for inner exception
                        if (serviceClient.LastException.InnerException != null)
                        {
                            Console.WriteLine($"  Inner Exception: {serviceClient.LastException.InnerException.Message}");
                        }
                    }
                    
                    // Additional troubleshooting info
                    if (serviceClient.LastError?.Contains("AADSTS500113") == true || 
                        serviceClient.LastException?.Message?.Contains("AADSTS500113") == true)
                    {
                        Console.WriteLine("\n  Troubleshooting:");
                        Console.WriteLine("  The redirect URI is not registered in Azure AD.");
                        Console.WriteLine("  Try one of these solutions:");
                        Console.WriteLine("  1. Register the redirect URI in Azure AD App Registration");
                        Console.WriteLine("  2. Use the default AppId: 51f81489-12ee-4a9e-aaae-a2591f45987d");
                        Console.WriteLine("  3. Use redirect URI: app://58145B91-0C36-4501-8424-6E796E04D6B7");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
        }
    }

    static async Task RetrieveAccountsAsync(IOrganizationService service)
    {
        try
        {
            Console.WriteLine("\n--- Retrieving Accounts ---");
            
            var query = new QueryExpression("account")
            {
                ColumnSet = new ColumnSet("name", "accountnumber", "telephone1"),
                TopCount = 5
            };

            var accounts = await Task.Run(() => service.RetrieveMultiple(query));

            Console.WriteLine($"Found {accounts.Entities.Count} account(s):");
            foreach (var account in accounts.Entities)
            {
                Console.WriteLine($"  - {account.GetAttributeValue<string>("name")} " +
                    $"(Phone: {account.GetAttributeValue<string>("telephone1") ?? "N/A"})");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving accounts: {ex.Message}");
        }
    }

    static async Task CreateTestAccountAsync(IOrganizationService service)
    {
        try
        {
            Console.WriteLine("\n--- Creating Test Account ---");
            
            var account = new Entity("account")
            {
                ["name"] = $"Test Account - {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
                ["telephone1"] = "555-0100",
                ["description"] = "Test account created by Dynamics365Connector"
            };

            var accountId = await Task.Run(() => service.Create(account));
            Console.WriteLine($"✓ Test account created successfully!");
            Console.WriteLine($"  Account ID: {accountId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating account: {ex.Message}");
        }
    }
}

