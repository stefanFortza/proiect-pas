import ollama
from models import SimulationStepResponse

async def process_language_step(text: str) -> SimulationStepResponse:
    # Placeholder logic for Day 1
    # In a real scenario, this would call Ollama
    # For now, let's return a mock response
    
    # Example: Simple check for demo purposes
    if "racheta" in text.lower() or "rocket" in text.lower():
        return SimulationStepResponse(
            score=0.9,
            feedback="Great job! The rocket is ready to launch.",
            translated_text="The rocket is moving."
        )
    else:
        return SimulationStepResponse(
            score=0.3,
            error_type="vocabulary",
            feedback="Try using words related to the simulation, like 'rocket' or 'launch'.",
            translated_text="Unknown command."
        )
