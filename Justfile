up:
    just api 

# Instaleaza dependintele pentru backend si frontend
install:
    cd backend-ai && uv sync
    cd frontend-3d && dotnet restore

# Ruleaza doar backend-ul FastAPI cu hot-reload
api:
    cd backend-ai && uv run uvicorn main:app --reload

# Curata cache-urile de build si fisierele temporare
clean:
    find . -type d -name "__pycache__" -exec rm -rf {} +
    find . -type d -name "bin" -exec rm -rf {} +
    find . -type d -name "obj" -exec rm -rf {} +
    rm -rf backend-ai/.pytest_cache
    rm -rf frontend-3d/.godot
