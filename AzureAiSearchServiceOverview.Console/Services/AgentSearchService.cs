using Azure;
using Azure.Identity;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using AzureAiSearchServiceOverview.Console.Models;
using AzureAiSearchServiceOverview.Console.Seed;
using Azure.Search.Documents.Agents;
using Azure.Search.Documents.Agents.Models;

namespace AzureAiSearchServiceOverview.Console.Services;

public class AgentSearchService(
    string aiSearchEndpoint,
    string aiSearchKey,
    EmbeddingsService embeddings,
    string aiFoundryEndpoint,
    string aiFoundryEmbeddingsDeployment,
    string aiFoundryModelDeployment)
{
    private readonly AzureKeyCredential _aiSearchCredential = new(aiSearchKey);
    private const string IndexName = "jobs_agent_index";
    private const string KnowledgeSourceName = "jobs-knowledge-source";
    private const string KnowledgeAgentName = "jobs-knowledge-agent";

    private SearchIndexClient CreateIndexClient() => new(new Uri(aiSearchEndpoint), _aiSearchCredential);

    private SearchClient CreateSearchClient() => new(new Uri(aiSearchEndpoint), IndexName, _aiSearchCredential);

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

        // Define a vectorizer
        var vectorizer = new AzureOpenAIVectorizer(vectorizerName: "azure_openai_text_3_small")
        {
            Parameters = new AzureOpenAIVectorizerParameters
            {
                ResourceUri = new Uri(aiFoundryEndpoint),
                DeploymentName = aiFoundryEmbeddingsDeployment,
                ModelName = aiFoundryEmbeddingsDeployment
            }
        };

        // Define a vector search profile and algorithm
        var vectorSearch = new VectorSearch
        {
            Profiles =
            {
                new VectorSearchProfile(
                    name: "v1-hnsw",
                    algorithmConfigurationName: "hnsw"
                )
                {
                    VectorizerName = "azure_openai_text_3_small"
                }
            },
            Algorithms =
            {
                new HnswAlgorithmConfiguration(name: "hnsw")
            },
            Vectorizers =
            {
                vectorizer
            }
        };

        // Define a semantic configuration
        var semanticConfig = new SemanticConfiguration(
            name: "semantic_config",
            prioritizedFields: new SemanticPrioritizedFields
            {
                ContentFields = { new SemanticField(nameof(Job.DescriptionVector)) }
            }
        );

        var semanticSearch = new SemanticSearch()
        {
            DefaultConfigurationName = "semantic_config",
            Configurations = { semanticConfig }
        };

        // Create the index
        var index = new SearchIndex(IndexName)
        {
            Fields = fields,
            VectorSearch = vectorSearch,
            SemanticSearch = semanticSearch
        };

        await indexClient.CreateOrUpdateIndexAsync(index, cancellationToken: ct);


        // Create knowledge source
        var indexKnowledgeSource = new SearchIndexKnowledgeSource(
            name: KnowledgeSourceName,
            searchIndexParameters: new SearchIndexKnowledgeSourceParameters(searchIndexName: IndexName)
            {
                SourceDataSelect = "Id,Name,Description,Salary"
            }
        );

        // Create or update the knowledge source
        await indexClient.CreateOrUpdateKnowledgeSourceAsync(indexKnowledgeSource, cancellationToken: ct);
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
    public async Task<KnowledgeAgentRetrievalResponse> AgentSearchAsync(string queryText,  CancellationToken ct = default)
    {
        var indexClient = CreateIndexClient();

        // Create a knowledge agent
        var openAiParameters = new AzureOpenAIVectorizerParameters
        {
            ResourceUri = new Uri(aiFoundryEndpoint),
            DeploymentName = aiFoundryModelDeployment,
            ModelName = aiFoundryModelDeployment
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
                    RerankerThreshold = (float?)2.5
                }
            ]
        )

        {
            OutputConfiguration = outputConfig
        };

        await indexClient.CreateOrUpdateKnowledgeAgentAsync(agent, cancellationToken: ct);

        // Set up messages
        var instructions = @"A Q&A agent that can answer questions about the jobs. If you don't have the answer, respond with ""I don't know"".";

        var messages = new List<Dictionary<string, string>>
        {
            new()
            {
                { "role", "system" },
                { "content", instructions }
            }
        };

        // Use agentic retrieval to fetch results on behalf of the user
        var agentClient = new KnowledgeAgentRetrievalClient(
            endpoint: new Uri(aiFoundryEndpoint),
            agentName: KnowledgeAgentName,
            tokenCredential: new DefaultAzureCredential()
        );

        messages.Add(new Dictionary<string, string>
        {
            { "role", "user" },
            {
                "content",
                queryText
            }
        });

        var retrievalResult = await agentClient.RetrieveAsync(
            retrievalRequest: new KnowledgeAgentRetrievalRequest(
                messages: messages
                    .Where(message => message["role"] != "system")
                    .Select(message =>
                        new KnowledgeAgentMessage(content: [new KnowledgeAgentMessageTextContent(message["content"])]) { Role = message["role"] }
                    )
                    .ToList()
            )
        );

        return retrievalResult.Value;
    }
}