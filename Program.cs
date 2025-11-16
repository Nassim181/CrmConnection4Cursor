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

            var connectionString = configuration.GetConnectionString("Dataverse") 
                ?? throw new InvalidOperationException("Connection string 'Dataverse' not found in configuration.");

            Console.WriteLine("Connecting to Dynamics 365 CRM Online...");

            // Create service client
            using var serviceClient = new ServiceClient(connectionString);

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

