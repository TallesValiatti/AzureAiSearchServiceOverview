using AzureAiSearchServiceOverview.Console.Models;
using AzureAiSearchServiceOverview.Console.Services;

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