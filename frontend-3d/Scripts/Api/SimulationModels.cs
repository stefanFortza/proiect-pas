using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ProiectSimulareLimbaj.Api;

public class PhysicsModifiers
{
    [JsonPropertyName("stability")]
    public float Stability { get; set; }

    [JsonPropertyName("thrust_multiplier")]
    public float ThrustMultiplier { get; set; }

    [JsonPropertyName("lateral_noise")]
    public float LateralNoise { get; set; }
}

public class SimulationResponse
{
    [JsonPropertyName("iteration_id")]
    public string IterationId { get; set; }

    [JsonPropertyName("source_text")]
    public string SourceText { get; set; }

    [JsonPropertyName("translated_text")]
    public string TranslatedText { get; set; }

    [JsonPropertyName("evaluation_score")]
    public float EvaluationScore { get; set; }

    [JsonPropertyName("error_category")]
    public string ErrorCategory { get; set; }

    [JsonPropertyName("physics_modifiers")]
    public PhysicsModifiers PhysicsModifiers { get; set; }
}

public class HealthResponse
{
    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("ollama_ready")]
    public bool OllamaReady { get; set; }

    [JsonPropertyName("models_loaded")]
    public List<string> ModelsLoaded { get; set; }
}

public class CustomTextRequest
{
    [JsonPropertyName("custom_source_text")]
    public string CustomSourceText { get; set; }

    [JsonPropertyName("target_language")]
    public string TargetLanguage { get; set; } = "en";
}
