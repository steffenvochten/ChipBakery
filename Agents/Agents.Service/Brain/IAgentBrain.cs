namespace Agents.Service.Brain;

public interface IAgentBrain
{
    /// <summary>
    /// Ask the LLM to make a decision. Returns an empty string when Ollama is
    /// unavailable so callers can fall back to rule-based behaviour.
    /// </summary>
    Task<string> ThinkAsync(string systemPrompt, string context, CancellationToken ct = default);
}
