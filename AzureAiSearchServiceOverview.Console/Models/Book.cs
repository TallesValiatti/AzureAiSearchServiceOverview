using Azure.Search.Documents.Indexes;

namespace AzureAiSearchServiceOverview.Console.Models;

public class Book
{
    [SimpleField(IsKey = true, IsFilterable = true)]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [SearchableField(IsFilterable = true, IsSortable = true)]
    public string Name { get; set; } = string.Empty;
    
    [SearchableField]
    public string Description { get; set; } = string.Empty;
    
    [SearchableField(IsFilterable = true, IsSortable = true)]
    public string Author { get; set; } = string.Empty;
    
    [SimpleField(IsFilterable = true, IsSortable = true, IsFacetable = true)]
    public int PageCount { get; set; }
    
    [SearchableField(IsFilterable = true, IsFacetable = true)]
    public string Genres { get; set; } = string.Empty; // Comma-separated genres
}