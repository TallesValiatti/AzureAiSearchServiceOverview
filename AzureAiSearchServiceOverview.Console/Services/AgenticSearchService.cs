using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Agents;
using Azure.Search.Documents.Agents.Models;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using AzureAiSearchServiceOverview.Console.Models;
using AzureAiSearchServiceOverview.Console.Seed;

namespace AzureAiSearchServiceOverview.Console.Services;

public class AgenticSearchService(
    string searchEndpoint,
    string searchApiKey,
    string aoaiEndpoint,
    string aoaiApiKey,
    string aoaiModel,
    string aoaiDeployment,
    EmbeddingsService embeddings)
{
    private readonly AzureKeyCredential _searchCredential = new(searchApiKey);
    private const string IndexName = "cars-info";
    private const string KnowledgeSourceName = "car-info-knowledge-source";
    private const string KnowledgeAgentName = "car-info-knowledge-agent";

    private SearchIndexClient CreateIndexClient() => new(new Uri(searchEndpoint), _searchCredential);
    private SearchClient CreateSearchClient() => new(new Uri(searchEndpoint), IndexName, _searchCredential);

    public async Task InitializeAsync(CancellationToken ct = default)
    {
        var indexClient = CreateIndexClient();
        await CreateOrUpdateIndexAsync(indexClient, ct);
        await SeedAsync(CreateSearchClient(), ct);
        await EnsureKnowledgeInfrastructureAsync(indexClient, ct);
    }
    private async Task CreateOrUpdateIndexAsync(SearchIndexClient indexClient, CancellationToken ct)
    {
        // Build from attributes
        var builder = new FieldBuilder();
        var fields = builder.Build(typeof(Car));

        var index = new SearchIndex(IndexName)
        {
            Fields = fields,
            Suggesters = { new SearchSuggester("sg", nameof(Car.Model)) },
            VectorSearch = new VectorSearch
            {
                Algorithms = { new HnswAlgorithmConfiguration("hnsw") },
                Profiles = { new VectorSearchProfile("v1-hnsw", "hnsw") }
            },
            SemanticSearch = new SemanticSearch
            {
                DefaultConfigurationName = "semantic_config",
                Configurations =
                {
                    new SemanticConfiguration(
                        name: "semantic_config",
                        prioritizedFields: new SemanticPrioritizedFields
                        {
                            TitleField = new SemanticField(nameof(Car.Model)),
                            ContentFields =
                            {
                                new SemanticField(nameof(Car.Description)),
                                new SemanticField(nameof(Car.Model))
                            }
                        })
                }
            }
        };

        await indexClient.CreateOrUpdateIndexAsync(index, cancellationToken: ct);
        System.Console.WriteLine($"Index '{IndexName}' created or updated successfully.");
    }

    private async Task SeedAsync(SearchClient client, CancellationToken ct)
    {
        var cars = CarData.GetSampleCars();

        // Generate vectors externally
        foreach (var car in cars)
            car.DescriptionVector = await embeddings.EmbedAsync(car.Description, ct);

        var batch = IndexDocumentsBatch.Upload(cars);
        await client.IndexDocumentsAsync(batch, cancellationToken: ct);
        System.Console.WriteLine($"Documents uploaded to index '{IndexName}' successfully.");
    }

    private async Task EnsureKnowledgeInfrastructureAsync(SearchIndexClient indexClient, CancellationToken ct)
    {
        // Create or update knowledge source
        var indexKnowledgeSource = new SearchIndexKnowledgeSource(
            name: KnowledgeSourceName,
            searchIndexParameters: new SearchIndexKnowledgeSourceParameters(searchIndexName: IndexName)
            {
                SourceDataSelect = $"{nameof(Car.Id)},{nameof(Car.Model)},{nameof(Car.Price)},{nameof(Car.Description)}",
            }
        );

        await indexClient.CreateOrUpdateKnowledgeSourceAsync(indexKnowledgeSource, cancellationToken: ct);
        System.Console.WriteLine($"Knowledge source '{KnowledgeSourceName}' created or updated successfully.");

        // Create or update knowledge agent
        var openAiParameters = new AzureOpenAIVectorizerParameters
        {
            ResourceUri = new Uri(aoaiEndpoint),
            DeploymentName = aoaiDeployment,
            ModelName = aoaiModel,
            ApiKey = aoaiApiKey
        };

        var agentModel = new KnowledgeAgentAzureOpenAIModel(azureOpenAIParameters: openAiParameters);
        var outputConfig = new KnowledgeAgentOutputConfiguration
        {
            Modality = KnowledgeAgentOutputConfigurationModality.AnswerSynthesis,
            IncludeActivity = true
        };

        var agent = new KnowledgeAgent(
            name: KnowledgeAgentName,
            models: [agentModel],
            knowledgeSources:
            [
                new KnowledgeSourceReference(KnowledgeSourceName)
                {
                    IncludeReferences = true,
                    IncludeReferenceSourceData = true,
                    RerankerThreshold = 1.8f
                }
            ]
        )
        {
            OutputConfiguration = outputConfig
        };

        await indexClient.CreateOrUpdateKnowledgeAgentAsync(agent, cancellationToken: ct);
        System.Console.WriteLine($"Knowledge agent '{KnowledgeAgentName}' created or updated successfully.");
    }

    public async Task<KnowledgeAgentRetrievalResponse> AgenticRetrievalAsync(
        string query,
        CancellationToken ct = default)
    {
        var agentClient = new KnowledgeAgentRetrievalClient(
            endpoint: new Uri(searchEndpoint),
            agentName: KnowledgeAgentName,
            credential: _searchCredential
        );

        // Note: System message is not included in the retrieval request
        // It would be used for conversation context in a full chat implementation
        var messages = new List<KnowledgeAgentMessage>
        {
            new(content: [new KnowledgeAgentMessageTextContent(query)])
            {
                Role = "user"
            }
        };

        var retrievalRequest = new KnowledgeAgentRetrievalRequest(messages: messages);
        var retrievalResult = await agentClient.RetrieveAsync(retrievalRequest, cancellationToken: ct);

        return retrievalResult.Value;
    }
}

