import ollama
import uuid
import random
from models import SimulationResponse, PhysicsModifiers
from typing import List

async def check_ollama_ready() -> bool:
    try:
        # Simple list call to check connectivity
        ollama.list()
        return True
    except Exception:
        return False

async def get_loaded_models() -> List[str]:
    try:
        response = ollama.list()
        return [model['name'] for model in response.get('models', [])]
    except Exception:
        return []

async def run_full_simulation_step() -> SimulationResponse:
    # Model 1: Generate a random phrase (Placeholder)
    phrases = [
        "Salut, cum te simți astăzi?",
        "Racheta este pregătită pentru lansare.",
        "Vremea este frumoasă pentru un zbor.",
        "Sistemele sunt nominale.",
        "Avem o mică problemă la motorul stâng."
    ]
    source_text = random.choice(phrases)
    return await run_custom_simulation(source_text, "en")

async def run_custom_simulation(source_text: str, target_language: str) -> SimulationResponse:
    # Model 2: Translation (Placeholder - would use Ollama in real scenario)
    # For now, let's pretend we translate
    translated_text = f"[Translated to {target_language}] {source_text}"
    
    # Model 3: Evaluation (Placeholder)
    # Simple score based on length or presence of keywords
    score = random.uniform(0.5, 1.0)
    category = "none" if score > 0.7 else "translation_error"
    
    # Physics modifiers based on score
    modifiers = PhysicsModifiers(
        stability=1.0 if score > 0.8 else 0.8,
        thrust_multiplier=1.0 + (score - 0.5),
        lateral_noise=0.0 if score > 0.9 else 0.1
    )
    
    return SimulationResponse(
        iteration_id=str(uuid.uuid4()),
        source_text=source_text,
        translated_text=translated_text,
        evaluation_score=score,
        error_category=category,
        physics_modifiers=modifiers
    )
