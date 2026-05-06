from pydantic import BaseModel
from typing import Optional

class SimulationStepRequest(BaseModel):
    text: str

class SimulationStepResponse(BaseModel):
    score: float
    error_type: Optional[str] = None
    feedback: str
    translated_text: Optional[str] = None
