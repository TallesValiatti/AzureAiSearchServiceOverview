using AzureAiSearchServiceOverview.Console.Models;
using AzureAiSearchServiceOverview.Console.Services;
using System.Text.Json;
using Azure.Search.Documents.Agents.Models;

// Full-text search
// await RunFullTextSearchExamplesAsync();

// Vector search
// await RunVectorSearchExamplesAsync();

// Agent search
await RunAgentSearchExamplesAsync();

return;

static async Task RunFullTextSearchExamplesAsync()
{
    var endpoint = "<AZURE-SEARCH-ENDPOINT>";
    string key = "<AZURE-SEARCH-KEY>";

    var service = new SearchService(endpoint, key);
    await service.InitializeAsync();

    // query 1: full-text search for keywords
    var q1 = await service.FullTextSearchAsync("ring OR desert OR dragon");
    Console.WriteLine("\n-- Query 1: Full-text search for ring, desert or dragon --");
    Print(q1);

    // query 2: filter by Fantasy genre, ordered by PageCount desc
    var q2 = await service.FullTextSearchAsync(
        "*",
        filter: "search.ismatch('Fantasy','Genres')",
        orderBy: "PageCount desc", size: 10);

    Console.WriteLine("\n-- Query 2: Fantasy books ordered by PageCount desc (top 10) --");
    Print(q2);

    // query 3: search for Arrakis or Middle-earth
    var q3 = await service.FullTextSearchAsync("Arrakis OR Middle-earth");
    Console.WriteLine("\n-- Query 3: Keyword search for Arrakis OR Middle-earth --");
    Print(q3);
    return;

    static void Print(IEnumerable<Book> books)
    {
        foreach (var b in books)
        {
            Console.WriteLine($"- {b.Name} | {b.Author} | {b.PageCount} pages | [genres: {b.Genres}]");
        }
    }
}

static async Task RunVectorSearchExamplesAsync()
{
    var searchEndpoint = "<AZURE-SEARCH-ENDPOINT>";
    var searchKey = "<AZURE-SEARCH-ADMIN-KEY>";
    var aiEndpoint = new Uri("<AZURE-AI-INFERENCE-ENDPOINT>");
    var aiApiKey = "<AZURE-AI-FOUNDATION-API-KEY>";
    var embeddingModel = "text-embedding-3-small";

    var embeddings = new EmbeddingsService(aiEndpoint, aiApiKey, embeddingModel);
    var service = new VectorSearchService(searchEndpoint, searchKey, embeddings);

    await service.InitializeAsync();

    //  query 1: Vector search example
    var vectorResults = await service.VectorSearchAsync(
        "Build and scale API with ASP.NET Core, Azure Functions, and SQL. Work on high throughput services.",
        3);

    Print("Vector Search (top 3)", vectorResults);

    // query 2: Vector search with filter 
    var vectorWithFilterResults = await service.VectorSearchAsync(
        "RAG and multi-agent solutions",
        3,
        "Salary gt 125000");

    Print("Vector Search with Salary Filter > $125K (top 3)", vectorWithFilterResults);

    // query 3: Hybrid search example
    var hybridResults = await service.HybridSearchAsync(
        "Azure Engineer",
        "multi-agent AI systems, RAG and vector retrieval",
        5);

    Print("Hybrid Search (top 5)", hybridResults);
    return;

    static void Print(string title, List<(double? Score, Job Document)> jobs)
    {
        Console.WriteLine($"\n-- {title} --");
        foreach (var job in jobs)
            Console.WriteLine(
                $"- {job.Score} | {job.Document.Name} | ${job.Document.Salary:N0} | {job.Document.Description}");
    }
}

static async Task RunAgentSearchExamplesAsync()
{
    var searchEndpoint = "<AZURE-SEARCH-ENDPOINT>";
    var searchKey = "<AZURE-SEARCH-ADMIN-KEY>";
    var aiEndpoint = new Uri("<AZURE-AI-INFERENCE-ENDPOINT>");
    var aiApiKey = "<AZURE-AI-FOUNDATION-API-KEY>";
    var aiFoundryEndpoint = "<AZURE-AI-FOUNDRY-ENDPOINT>";
    var aiFoundryEmbeddingsDeployment = "text-embedding-3-small";
    var aiFoundryModelDeployment = "gpt-4o-mini";
    var embeddingModel = "text-embedding-3-small";

    var embeddings = new EmbeddingsService(aiEndpoint, aiApiKey, embeddingModel);
    
    var service = new AgentSearchService(
        searchEndpoint, 
        searchKey, 
        embeddings, 
        aiFoundryEndpoint, 
        aiFoundryEmbeddingsDeployment, 
        aiFoundryModelDeployment);

    await service.InitializeAsync();

    // Query: Agent search example
    var agentResults = await service.AgentSearchAsync(
        "What are the best paying jobs related to Azure and AI?");

    Console.WriteLine("\n-- Agent Search Results --");
    Console.WriteLine("\nResponse:");
    Console.WriteLine((agentResults.Response[0].Content[0] as KnowledgeAgentMessageTextContent)?.Text);
    
    Console.WriteLine("\nActivity:");
    foreach (var activity in agentResults.Activity)
    {
        Console.WriteLine($"Activity Type: {activity.GetType().Name}");
        string activityJson = JsonSerializer.Serialize(
            activity,
            activity.GetType(),
            new JsonSerializerOptions { WriteIndented = true }
        );
        Console.WriteLine(activityJson);
    }

    Console.WriteLine("\nResults:");
    foreach (var reference in agentResults.References)
    {
        Console.WriteLine($"Reference Type: {reference.GetType().Name}");
        string referenceJson = JsonSerializer.Serialize(
            reference,
            reference.GetType(),
            new JsonSerializerOptions { WriteIndented = true }
        );
        Console.WriteLine(referenceJson);
    }
}
