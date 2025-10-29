using Azure.Search.Documents.Indexes;

namespace AzureAiSearchServiceOverview.Console.Models;

public class Job
{
    [SimpleField(IsKey = true, IsFilterable = true)]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [SearchableField(IsFilterable = true, IsSortable = true)]
    public string Name { get; set; } = string.Empty;

    [SimpleField(IsFilterable = true, IsSortable = true, IsFacetable = true)]
    public double Salary { get; set; }

    [SearchableField]
    public string Description { get; set; } = string.Empty;
    
    [VectorSearchField(
        VectorSearchDimensions = 1536,
        VectorSearchProfileName = "v1-hnsw"
    )]
    public float[] DescriptionVector { get; set; } = [];
}