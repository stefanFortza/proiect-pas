from fastapi import FastAPI
from models import SimulationStepRequest, SimulationStepResponse
import llm_controller

app = FastAPI(title="Language Simulation API")

@app.get("/")
async def root():
    return {"message": "Language Simulation API is running"}

@app.post("/simulate_step", response_model=SimulationStepResponse)
async def simulate_step(request: SimulationStepRequest):
    # This is a placeholder for the logic that will be implemented in Day 1
    result = await llm_controller.process_language_step(request.text)
    return result

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000)
