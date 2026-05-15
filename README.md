# Barca Supraviețuirii - Simulare Multi-Agent

Proiectul reprezintă o simulare 3D în care integritatea unei ambarcațiuni depinde de calitatea dialogului dintre agenți inteligenți. Sistemul utilizează un pipeline de tip closed-loop între un motor de fizică (Godot) și modele de limbaj natural (LLM).


Mecanica principală: Răspunsurile slabe cresc masa bărcii, ducând în final la scufundarea acesteia conform legilor fizicii hidrodinamice.

## Structură Proiect
* **Backend (Python/FastAPI)**: Gestionează evaluarea NLP, prompt engineering-ul pentru Inspector și servirea datelor pentru simulare.
* **Frontend (Godot 4 C#)**: Gestionează simularea fizică (RigidBody3D), sistemul de flotabilitate (buoyancy) și vizualizarea interacțiunilor.

## Instalare și Rulare

### Prerechizite
* Python 3.12+ (recomandat managerul `uv`)
* Ollama (cu modelele locale descărcate)
* Godot Engine 4.x (Ediția .NET) și SDK .NET 8

### Instrucțiuni
1. **Backend**:
   ```bash
   cd backend-ai
   uv run uvicorn main:app --reload
   ```
2. **Frontend**:
   Deschideți folderul `frontend-3d` în Godot și lansați scena principală (F5).

## Tehnologii Folosite
* **AI**: Ollama (Llama 3 / Gemma)
* **Backend**: FastAPI, Python, Pydantic
* **Frontend**: Godot 4 (C#), Refit, Kenney Assets
