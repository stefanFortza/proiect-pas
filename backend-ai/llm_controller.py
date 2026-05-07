import ollama
import uuid
import random
from models import SimulationResponse, GroundTruth, TeacherData, StudentData, InspectorData
from typing import List

async def check_ollama_ready() -> bool:
    try:
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
    # Placeholder implementation following the new plan
    
    # Ground Truth
    gt = GroundTruth(obiect_corect="mar", locatie_corecta="masa")
    
    # Teacher
    teacher = TeacherData(text="Elevule, adu-mi te rog mărul de pe masa rotundă.")
    
    # Student (Simulated hallucination)
    student = StudentData(
        traducere_interna="Bring the apple from the desk.",
        intentie_obiect="mar",
        intentie_locatie="birou"
    )
    
    # Inspector
    inspector = InspectorData(
        scor=5,
        verdict="Nota 5. Elevul a identificat corect obiectul (mar), dar a greșit locația (Birou în loc de Masă)."
    )
    
    return SimulationResponse(
        adevar_absolut=gt,
        profesor=teacher,
        elev=student,
        inspector=inspector
    )
