using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ProiectSimulareLimbaj.Api;

public class GenerateRequest
{
    [JsonPropertyName("prompt")]
    public string Prompt { get; set; }

    [JsonPropertyName("model_name")]
    public string ModelName { get; set; }

    [JsonPropertyName("epoch")]
    public int Epoch { get; set; }

    [JsonPropertyName("temperature")]
    public float Temperature { get; set; } = 0.7f;

    [JsonPropertyName("max_tokens")]
    public int MaxTokens { get; set; } = 40;
}

public class EvaluationScores
{
    [JsonPropertyName("grammatical_structure")]
    public int GrammaticalStructure { get; set; }

    [JsonPropertyName("syntactical_structure")]
    public int SyntacticalStructure { get; set; }

    [JsonPropertyName("meaning")]
    public int Meaning { get; set; }

    [JsonPropertyName("relevance")]
    public int Relevance { get; set; }

    [JsonPropertyName("error")]
    public string Error { get; set; }
}

public class GenerateResponse
{
    [JsonPropertyName("model_used")]
    public string ModelUsed { get; set; }

    [JsonPropertyName("epoch_used")]
    public int EpochUsed { get; set; }

    [JsonPropertyName("prompt")]
    public string Prompt { get; set; }

    [JsonPropertyName("response")]
    public string Response { get; set; }

    [JsonPropertyName("evaluation_scores")]
    public EvaluationScores EvaluationScores { get; set; }
}

public class HealthResponse
{
    [JsonPropertyName("status")]
    public string Status { get; set; }
}
