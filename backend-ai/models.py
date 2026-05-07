from pydantic import BaseModel
from typing import List

class GroundTruth(BaseModel):
    obiect_corect: str
    locatie_corecta: str

class TeacherData(BaseModel):
    text: str

class StudentData(BaseModel):
    traducere_interna: str
    intentie_obiect: str
    intentie_locatie: str

class InspectorData(BaseModel):
    scor: int
    verdict: str

class SimulationResponse(BaseModel):
    adevar_absolut: GroundTruth
    profesor: TeacherData
    elev: StudentData
    inspector: InspectorData

class HealthResponse(BaseModel):
    status: str
    ollama_ready: bool
    models_loaded: List[str]
