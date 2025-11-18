﻿using AzureAiSearchServiceOverview.Console.Models;
using AzureAiSearchServiceOverview.Console.Services;
using Azure.Search.Documents.Agents.Models;

// Full-text search
// await RunFullTextSearchExamplesAsync();

// Vector search
// await RunVectorSearchExamplesAsync();

// Agentic search
await RunAgenticSearchExamplesAsync();

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

static async Task RunAgenticSearchExamplesAsync()
{
    var searchEndpoint = "<AZURE-SEARCH-ENDPOINT>";
    var searchKey = "<AZURE-SEARCH-ADMIN-KEY>";
    var aoaiEndpoint = "<AZURE-OPENAI-ENDPOINT>";
    var aoaiApiKey = "<AZURE-OPENAI-API-KEY>";
    var aoaiModel = "gpt-4o";
    var aoaiDeployment = "gpt-4o";
    var aiEndpoint = new Uri("<AZURE-AI-INFERENCE-ENDPOINT>");
    var aiApiKey = "<AZURE-AI-FOUNDATION-API-KEY>";
    var embeddingModel = "text-embedding-3-small";

    var embeddings = new EmbeddingsService(aiEndpoint, aiApiKey, embeddingModel);
    var service = new AgenticSearchService(
        searchEndpoint,
        searchKey,
        aoaiEndpoint,
        aoaiApiKey,
        aoaiModel,
        aoaiDeployment,
        embeddings);

    await service.InitializeAsync();

    // query 1: Complex multi-part question about electric cars
    var result1 = await service.AgenticRetrievalAsync(
        "What are the best electric cars for performance and range? Compare their acceleration times and battery ranges.");
    Console.WriteLine("\n-- Query 1: Best electric cars for performance and range --");
    Print(result1);

    // query 2: Price comparison question
    var result2 = await service.AgenticRetrievalAsync(
        "Which luxury performance cars are available under $100,000? Include their key features and performance specs.");
    Console.WriteLine("\n-- Query 2: Luxury performance cars under $100,000 --");
    Print(result2);

    // query 3: Specific feature query
    var result3 = await service.AgenticRetrievalAsync(
        "Which cars offer all-wheel drive with more than 600 horsepower? What makes them special?");
    Console.WriteLine("\n-- Query 3: AWD cars with 600+ horsepower --");
    Print(result3);

    // query 4: Brand-specific comparison
    var result4 = await service.AgenticRetrievalAsync(
        "Compare the German performance vehicles in terms of power, technology, and price. Which one offers the best value?");
    Console.WriteLine("\n-- Query 4: German performance vehicles comparison --");
    Print(result4);
    return;

    static void Print(KnowledgeAgentRetrievalResponse response)
    {
        // Print the response
        var answerText = (response.Response[0].Content[0] as KnowledgeAgentMessageTextContent)?.Text;
        Console.WriteLine($"Answer: {answerText}\n");

        // Print references
        Console.WriteLine($"References ({response.References.Count}):");
        foreach (var reference in response.References)
        {
            if (reference is KnowledgeAgentSearchIndexReference searchRef)
            {
                Console.WriteLine($"  - DocKey: {searchRef.DocKey} | Score: {searchRef.RerankerScore:F2}");
                if (searchRef.SourceData != null)
                {
                    var sourceDict = searchRef.SourceData as System.Collections.Generic.IDictionary<string, object>;
                    if (sourceDict != null && sourceDict.ContainsKey("Model"))
                    {
                        Console.WriteLine($"    Model: {sourceDict["Model"]} | Price: {sourceDict["Price"]}");
                    }
                }
            }
        }
        Console.WriteLine();
    }
}

