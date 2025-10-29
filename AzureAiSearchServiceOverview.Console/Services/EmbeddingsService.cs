using System.Runtime.InteropServices;
using System.Text.Json;
using Azure;
using Azure.AI.Inference;

namespace AzureAiSearchServiceOverview.Console.Services;

public class EmbeddingsService(Uri endpoint, string apiKey, string model)
{
    private readonly EmbeddingsClient _client = new(endpoint, new AzureKeyCredential(apiKey));

    public async Task<float[]> EmbedAsync(string text, CancellationToken ct = default)
    {
        return (await EmbedAsync([text], ct)).First();
    }
    
    public async Task<List<float[]>> EmbedAsync(IEnumerable<string> texts, CancellationToken ct = default)
    {
        var options = new EmbeddingsOptions(texts)
        {
            Model = model
        };

        Response<EmbeddingsResult> response = await _client.EmbedAsync(options, ct);

        return response.Value.Data.Select(d => 
            ConvertToFloatArray(d.Embedding)).ToList();
    }

    private static float[] ConvertToFloatArray(BinaryData embedding)
    {
        return JsonSerializer.Deserialize<float[]>(embedding.ToArray())!;

    }
}