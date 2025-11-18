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

    // Query 1: Simple query targeting the Model field
    // Expected: Should find the Tesla Model S Plaid and describe its features
    // Demonstrates: Basic semantic search on model names
    var result1 = await service.AgenticRetrievalAsync("Tell me about the Tesla Model S");
    Console.WriteLine("\n-- Query 1: Simple model search (Tesla Model S) --");
    Print(result1);

    // Query 2: Simple query targeting the Price field with a filter concept
    // Expected: Should return cars under $80,000 like BMW M3 Competition and Ford Mustang Mach-E GT
    // Demonstrates: Semantic understanding of price ranges
    var result2 = await service.AgenticRetrievalAsync("What cars are available under $80,000?");
    Console.WriteLine("\n-- Query 2: Price range search (under $80,000) --");
    Print(result2);

    // Query 3: Simple query targeting the Description field for specific features
    // Expected: Should find electric vehicles like Tesla, Porsche Taycan, Lucid Air, Ford Mach-E, Rivian
    // Demonstrates: Semantic search on technical specifications in descriptions
    var result3 = await service.AgenticRetrievalAsync("Which cars are fully electric?");
    Console.WriteLine("\n-- Query 3: Feature-based search (electric vehicles) --");
    Print(result3);

    // Query 4: Complex multi-part query combining Model, Price, and Description
    // Expected: Should identify Porsche Taycan Turbo S and Mercedes-AMG GT 63 S, comparing their
    //           performance specs (horsepower, acceleration), luxury features, and prices
    // Demonstrates: Agent's ability to decompose complex queries, search multiple fields,
    //               synthesize results, and provide comparative analysis
    var result4 = await service.AgenticRetrievalAsync(
        "Compare the most expensive luxury performance cars. What are their horsepower ratings, " +
        "acceleration times, and key luxury features? Which one offers the best performance per dollar?");
    Console.WriteLine("\n-- Query 4: Complex multi-field comparison query --");
    Print(result4);
    return;

    static void Print(KnowledgeAgentRetrievalResponse response)
    {
        // Build a mapping of ref_id to Model name
        var refIdToModel = new Dictionary<string, string>();
        foreach (var reference in response.References)
        {
            if (reference is KnowledgeAgentSearchIndexReference searchRef)
            {
                if (searchRef.SourceData != null)
                {
                    var sourceDict = searchRef.SourceData as System.Collections.Generic.IDictionary<string, object>;
                    if (sourceDict != null && sourceDict.ContainsKey("Model"))
                    {
                        refIdToModel[searchRef.Id] = sourceDict["Model"]?.ToString() ?? "";
                    }
                }
            }
        }

        // Get the answer text and replace ref_id with Model names
        var answerText = (response.Response[0].Content[0] as KnowledgeAgentMessageTextContent)?.Text ?? "";
        
        // Replace all [ref_id:X] with the actual Model name
        foreach (var kvp in refIdToModel)
        {
            answerText = answerText.Replace($"[ref_id:{kvp.Key}]", $"({kvp.Value})");
        }
        
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
                        Console.WriteLine($"    Model: {sourceDict["Model"]}");
                    }
                }
            }
        }
        Console.WriteLine();
    }
}

