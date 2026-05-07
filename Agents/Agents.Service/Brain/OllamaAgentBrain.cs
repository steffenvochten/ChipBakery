using OllamaSharp;

namespace Agents.Service.Brain;

public class OllamaAgentBrain(IConfiguration cfg, ILogger<OllamaAgentBrain> logger) : IAgentBrain
{
    private const string Model = "llama3.2:3b";

    private readonly OllamaApiClient _ollama = new(
        cfg["OLLAMA_BASE_URL"] ?? cfg["services:ollama:http:0"] ?? "http://localhost:11434")
    {
        SelectedModel = Model
    };

    public async Task<string> ThinkAsync(string systemPrompt, string context, CancellationToken ct)
    {
        try
        {
            var chat = new Chat(_ollama, systemPrompt);
            var sb = new System.Text.StringBuilder();
            await foreach (var token in chat.SendAsync(context, cancellationToken: ct))
                sb.Append(token);
            return sb.ToString();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Ollama request failed — agents will use fallback logic");
            return string.Empty;
        }
    }
}
