using Azure;
using Azure.Identity;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using AzureAiSearchServiceOverview.Console.Models;
using AzureAiSearchServiceOverview.Console.Seed;

namespace AzureAiSearchServiceOverview.Console.Services;

public class SearchService(string endpoint, string apiKey)
{
    private readonly AzureKeyCredential _credential = new(apiKey);
    private const string IndexName = "books";
    
    private SearchIndexClient CreateIndexClient() => new(new Uri(endpoint), _credential);
    private SearchClient CreateSearchClient() => new(new Uri(endpoint), IndexName, _credential);
    
    public async Task InitializeAsync()
    {
        var indexClient = CreateIndexClient();
        var searchClient = CreateSearchClient();
        
        await CreateOrUpdateIndexAsync(indexClient);
        await SeedDataAsync(searchClient);
    }

    private async Task CreateOrUpdateIndexAsync(SearchIndexClient indexClient)
    {
        var fieldBuilder = new FieldBuilder();
        var fields = fieldBuilder.Build(typeof(Book));
        
        var index = new SearchIndex(IndexName)
        {
            Fields = fields,
            Suggesters = { new SearchSuggester("sg", nameof(Book.Name), nameof(Book.Author)) }
        };
        
        await indexClient.CreateOrUpdateIndexAsync(index);
    }
    
    private async Task SeedDataAsync(SearchClient searchClient)
    {
        var docs = BookData.GetSampleBooks();
        var batch = IndexDocumentsBatch.Upload(docs);
        var result = await searchClient.IndexDocumentsAsync(batch);
    }
    
    public async Task<List<Book>> FullTextSearchAsync(
        string searchText, 
        string? filter = null, 
        string? orderBy = null, 
        int size = 5)
    {
        var client = CreateSearchClient();
        var options = new SearchOptions
        {
            QueryType = SearchQueryType.Simple,
            Size = size
        };
        
        if (!string.IsNullOrWhiteSpace(filter))
            options.Filter = filter;
        
        if (!string.IsNullOrWhiteSpace(orderBy))
            options.OrderBy.Add(orderBy);
        
        var results = new List<Book>();
        var response = await client.SearchAsync<Book>(searchText, options);
        
        await foreach (var result in response.Value.GetResultsAsync())
        {
            results.Add(result.Document);
        }
        
        return results;
    }
}