using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using AzureAiSearchServiceOverview.Console.Models;
using AzureAiSearchServiceOverview.Console.Seed;

namespace AzureAiSearchServiceOverview.Console.Services;

public class VectorSearchService(string endpoint, string apiKey, EmbeddingsService embeddings)
{
    private readonly AzureKeyCredential _credential = new(apiKey);
    private const string IndexName = "jobs";

    private SearchIndexClient CreateIndexClient() => new(new Uri(endpoint), _credential);
    private SearchClient CreateSearchClient() => new(new Uri(endpoint), IndexName, _credential);

    public async Task InitializeAsync(CancellationToken ct = default)
    {
        var indexClient = CreateIndexClient();
        await CreateOrUpdateIndexAsync(indexClient, ct);
        await SeedAsync(CreateSearchClient(), ct);
    }

    private async Task CreateOrUpdateIndexAsync(SearchIndexClient indexClient, CancellationToken ct)
    {
        // Build from attributes
        var builder = new FieldBuilder();
        var fields = builder.Build(typeof(Job));

        var index = new SearchIndex(IndexName)
        {
            Fields = fields,
            Suggesters = { new SearchSuggester("sg", nameof(Job.Name)) },
            VectorSearch = new VectorSearch
            {
                Algorithms = { new HnswAlgorithmConfiguration("hnsw") },
                Profiles = { new VectorSearchProfile("v1-hnsw", "hnsw") }
            }
        };

        await indexClient.CreateOrUpdateIndexAsync(index, cancellationToken: ct);
    }

    private async Task SeedAsync(SearchClient client, CancellationToken ct)
    {
        var jobs = JobData.GetSampleJobs();

        // Generate vectors externally
        foreach (var j in jobs)
            j.DescriptionVector = (await embeddings.EmbedAsync(j.Description, ct));

        var batch = IndexDocumentsBatch.Upload(jobs);
        await client.IndexDocumentsAsync(batch, cancellationToken: ct);
    }

    // Pure vector search
    public async Task<List<(double? Score, Job Document)>> VectorSearchAsync(string queryText, int k = 5, string? filter = null,
        CancellationToken ct = default)
    {
        var client = CreateSearchClient();
        var vector = (await embeddings.EmbedAsync(queryText, ct)).AsMemory();

        var options = new SearchOptions
        {
            Size = k,
            Filter = string.IsNullOrWhiteSpace(filter) ? null : filter,
            VectorSearch = new()
            {
                Queries =
                {
                    new VectorizedQuery(vector)
                        { KNearestNeighborsCount = k, Fields = { nameof(Job.DescriptionVector) } }
                }
            }
        };

        var results = new List<(double? Score, Job Document)>();
        var response = await client.SearchAsync<Job>(null, options, ct);

        await foreach (var r in response.Value.GetResultsAsync())
        {
            results.Add((r.Score, r.Document));
        }


        return results;
    }
    
    public async Task<List<(double? Score, Job Document)>> HybridSearchAsync(
        string keywordText, 
        string vectorText, 
        int k = 5,
        string? filter = null, CancellationToken ct = default)
    {
        var client = CreateSearchClient();
        var vector = (await embeddings.EmbedAsync(vectorText, ct)).AsMemory();

        var options = new SearchOptions
        {
            QueryType = SearchQueryType.Simple,
            Size = k,
            Filter = string.IsNullOrWhiteSpace(filter) ? null : filter,
            VectorSearch = new()
            {
                Queries =
                {
                    new VectorizedQuery(vector)
                        { KNearestNeighborsCount = k, Fields = { nameof(Job.DescriptionVector) } }
                }
            }
        };

        var results = new List<(double? Score, Job Document)>();
        var response = await client.SearchAsync<Job>(keywordText, options, ct);

        await foreach (var r in response.Value.GetResultsAsync())
            results.Add((r.Score, r.Document));

        return results;
    }
}
