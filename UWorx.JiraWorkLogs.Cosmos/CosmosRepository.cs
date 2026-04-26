using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;

namespace UWorx.JiraWorkLogs.Cosmos;

class Page
{
    public string id { get; set; }
    public string partitionKey { get; set; }
    public string Html { get; set; } = null;
    public DateTime? LastUpdated { get; set; } = null;
}

class CosmosRepository : IWebAppRepository, IServiceRepository
{
    const string EndpointUri = @"https://cosmos-jiraworklogs-dev.documents.azure.com:443/";
    const string PrimaryKey = Environment.GetEnvironmentVariable("COSMOS_KEY");
    const string DatabaseId = "JiraWorkLogs";
    const string ContainerId = "Pages";

    readonly ILogger logger;

    public CosmosRepository(ILogger logger)
    {
        this.logger = logger;
    }

    async Task<Page> createPageAsync(Container container, string id)
    {
        var page = new Page { partitionKey = id, id = id, Html = $"{id} is not yet available" };

        try
        {
            ItemResponse<Page> response = await container.ReadItemAsync<Page>(page.id, new PartitionKey(page.partitionKey));
            page = response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            try
            {
                ItemResponse<Page> response = await container.CreateItemAsync<Page>(page, new PartitionKey(page.partitionKey));

                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                this.logger.LogInformation("Created item in database with id: {0} Operation consumed {1} RUs.\n", response.Resource.id, response.RequestCharge);
            }
            catch (Exception ex2)
            {
                this.logger.LogError(ex2, "Failed to create {id}", id);
            }
        }

        return page;
    }

    public async Task<string> GetHtmlAsync(int page)
    {
        using (var cosmosClient = new CosmosClient(EndpointUri, PrimaryKey, new CosmosClientOptions() { ApplicationName = "JiraLogsWebApp" }))
        {
            Database database = await cosmosClient.CreateDatabaseIfNotExistsAsync(DatabaseId);
            Container container = await database.CreateContainerIfNotExistsAsync(ContainerId, "/partitionKey");

            Page p = await createPageAsync(container, $"page{page}");
            return p.Html;
        }
    }

    public async Task<string> GetLastUpdateAsync()
    {
        throw new NotImplementedException();
    }

    public async Task InitializeAsync()
    {
        throw new NotImplementedException();
    }

    public async Task SaveHtmlAsync(int page, string html)
    {
        throw new NotImplementedException();
    }
}
