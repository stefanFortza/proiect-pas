using System.Text.Json.Serialization;

namespace ProiectSimulareLimbaj.Api;

public class GroundTruth
{
    [JsonPropertyName("obiect_corect")]
    public string ObiectCorect { get; set; }

    [JsonPropertyName("locatie_corecta")]
    public string LocatieCorecta { get; set; }
}

public class TeacherData
{
    [JsonPropertyName("text")]
    public string Text { get; set; }
}

public class StudentData
{
    [JsonPropertyName("traducere_interna")]
    public string TraducereInterna { get; set; }

    [JsonPropertyName("intentie_obiect")]
    public string IntentieObiect { get; set; }

    [JsonPropertyName("intentie_locatie")]
    public string IntentieLocatie { get; set; }
}

public class InspectorData
{
    [JsonPropertyName("scor")]
    public int Scor { get; set; }

    [JsonPropertyName("verdict")]
    public string Verdict { get; set; }
}

public class SimulationResponse
{
    [JsonPropertyName("adevar_absolut")]
    public GroundTruth AdevarAbsolut { get; set; }

    [JsonPropertyName("profesor")]
    public TeacherData Profesor { get; set; }

    [JsonPropertyName("elev")]
    public StudentData Elev { get; set; }

    [JsonPropertyName("inspector")]
    public InspectorData Inspector { get; set; }
}

public class HealthResponse
{
    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("ollama_ready")]
    public bool OllamaReady { get; set; }

    [JsonPropertyName("models_loaded")]
    public System.Collections.Generic.List<string> ModelsLoaded { get; set; }
}
