from fastapi import FastAPI, HTTPException
from models import HealthResponse, SimulationResponse, CustomTextRequest, PhysicsModifiers
import llm_controller
import uuid

app = FastAPI(title="Language Simulation API - Hackathon Edition")

@app.get("/health", response_model=HealthResponse)
async def health_check():
    try:
        ollama_status = await llm_controller.check_ollama_ready()
        models = await llm_controller.get_loaded_models()
        return HealthResponse(
            status="online",
            ollama_ready=ollama_status,
            models_loaded=models
        )
    except Exception as e:
        return HealthResponse(
            status="degraded",
            ollama_ready=False,
            models_loaded=[]
        )

@app.get("/api/v1/simulation/step", response_model=SimulationResponse)
async def simulation_step():
    try:
        result = await llm_controller.run_full_simulation_step()
        return result
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

@app.post("/api/v1/simulation/custom", response_model=SimulationResponse)
async def simulation_custom(request: CustomTextRequest):
    try:
        result = await llm_controller.run_custom_simulation(request.custom_source_text, request.target_language)
        return result
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000)
