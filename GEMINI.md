# Specificație Tehnică API (Hackathon Mode)

Arhitectura API-ului este strict funcțională, folosind un tipar **REST minimalist**. Frontend-ul (Godot) acționează ca un client care trage datele (*polling*) sau trimite evenimente specifice.

## Rute Esențiale

### 1. Ruta de Diagnoză (Health Check)
Obligatorie pentru a preveni crash-urile în Godot. Verifică disponibilitatea serverului Python și conexiunea locală cu Ollama.

*   **Endpoint:** `GET /health`
*   **Response (200 OK):**
    ```json
    {
      "status": "online",
      "ollama_ready": true,
      "models_loaded": ["llama3", "gemma:2b"]
    }
    ```

### 2. Ruta Principală de Simulare (Automată)
Declanșează fluxul complet (Model 1 alege fraza $\rightarrow$ Model 2 traduce $\rightarrow$ Model 3 evaluează).

*   **Endpoint:** `GET /api/v1/simulation/step`
*   **Response (200 OK):**
    ```json
    {
      "iteration_id": "uuid-v4",
      "source_text": "string",
      "translated_text": "string",
      "evaluation_score": 0.85, 
      "error_category": "none",
      "physics_modifiers": {
        "stability": 1.0,
        "thrust_multiplier": 1.2,
        "lateral_noise": 0.0
      }
    }
    ```

### 3. Ruta de Simulare (Manuală / Debug)
Permite introducerea unui text specific pentru a forța Modelul 2 să greșească.

*   **Endpoint:** `POST /api/v1/simulation/custom`
*   **Request Body (JSON):**
    ```json
    {
      "custom_source_text": "string",
      "target_language": "en"
    }
    ```
*   **Response (200 OK):** Returnează același format JSON ca ruta automată.

## Contracte de Date (Pydantic)

```python
from pydantic import BaseModel
from typing import List

class PhysicsModifiers(BaseModel):
    stability: float
    thrust_multiplier: float
    lateral_noise: float

class SimulationResponse(BaseModel):
    iteration_id: str
    source_text: str
    translated_text: str
    evaluation_score: float
    error_category: str
    physics_modifiers: PhysicsModifiers

class HealthResponse(BaseModel):
    status: str
    ollama_ready: bool
    models_loaded: List[str]

class CustomTextRequest(BaseModel):
    custom_source_text: str
    target_language: str = "en"
```

## Maparea în Godot (Refit C#)

```csharp
using Refit;
using System.Threading.Tasks;

public interface ISimulationApi
{
    [Get("/health")]
    Task<HealthResponse> CheckHealthAsync();

    [Get("/api/v1/simulation/step")]
    Task<SimulationResponse> GetNextStepAsync();

    [Post("/api/v1/simulation/custom")]
    Task<SimulationResponse> EvaluateCustomTextAsync([Body] CustomTextRequest request);
}
```
