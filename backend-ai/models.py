from pydantic import BaseModel
from typing import List, Optional

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
