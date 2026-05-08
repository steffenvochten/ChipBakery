using OllamaSharp;

namespace Agents.Service.Brain;

public class OllamaAgentBrain : IAgentBrain
{
    private const string Model = "llama3.2:3b";
    private readonly OllamaApiClient _ollama;
    private readonly ILogger<OllamaAgentBrain> _logger;
    private bool _modelChecked = false;

    public OllamaAgentBrain(IConfiguration cfg, ILogger<OllamaAgentBrain> logger)
    {
        _logger = logger;
        var baseUrl = cfg["OLLAMA_BASE_URL"] ?? cfg["services:ollama:http:0"] ?? "http://localhost:11434";
        _ollama = new OllamaApiClient(baseUrl)
        {
            SelectedModel = Model
        };
    }

    public async Task<string> ThinkAsync(string systemPrompt, string context, CancellationToken ct)
    {
        try
        {
            await EnsureModelExistsAsync(ct);

            var chat = new Chat(_ollama, systemPrompt);
            var sb = new System.Text.StringBuilder();
            await foreach (var token in chat.SendAsync(context, cancellationToken: ct))
                sb.Append(token);
            return sb.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Ollama request failed — agents will use fallback logic");
            return string.Empty;
        }
    }

    private async Task EnsureModelExistsAsync(CancellationToken ct)
    {
        if (_modelChecked) return;

        try
        {
            _logger.LogInformation("Checking if Ollama model {Model} is available...", Model);
            var models = await _ollama.ListLocalModelsAsync(ct);
            if (models.Any(m => m.Name.Contains(Model, StringComparison.OrdinalIgnoreCase)))
            {
                _modelChecked = true;
                return;
            }

            _logger.LogInformation("Model {Model} not found. Pulling from Ollama registry (this may take a while)...", Model);
            await foreach (var status in _ollama.PullModelAsync(Model, ct))
            {
                if (!string.IsNullOrEmpty(status?.Status))
                {
                    _logger.LogDebug("Pulling {Model}: {Status}", Model, status.Status);
                }
            }
            
            _modelChecked = true;
            _logger.LogInformation("Model {Model} pulled successfully.", Model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to verify or pull Ollama model {Model}. Ensure Ollama is running and accessible.", Model);
            // We don't throw here to allow fallback logic in ThinkAsync
            _modelChecked = true; // Don't keep retrying if it's fundamentally broken
        }
    }
}
