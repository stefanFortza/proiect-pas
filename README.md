# Proiect Simulare Limbaj (Hackathon Mode 🚀)

Acest proiect este o simulare 3D în Godot unde o rachetă este controlată prin comenzi de limbaj natural procesate de un backend AI (Ollama + FastAPI).

## Structura Proiectului

- `backend-ai/`: Logică Python, API FastAPI și integrare LLM via Ollama.
- `frontend-3d/`: Aplicație Godot C# pentru vizualizare și fizică.

---

## Getting Started

### 1. Prerechizite (Ce trebuie instalat)

#### Backend (Python)
- **uv**: Manager de pachete ultra-rapid pentru Python.
  - *Instalare (Linux/macOS):* 
    - `curl -LsSf https://astral.sh/uv/install.sh | sh`
    - *Sau (dacă nu ai curl):* `wget -qO- https://astral.sh/uv/install.sh | sh`
  - *Instalare (Windows):* 
    - `powershell -ExecutionPolicy ByPass -c "irm https://astral.sh/uv/install.ps1 | iex"`
- **Ollama**: Pentru a rula LLM-uri local.
  - Descarcă de pe [ollama.com](https://ollama.com/).
  - După instalare, rulează: `ollama run llama3`

#### Frontend (Godot)
- **Godot Engine (v4.x - .NET Edition)**: Asigură-te că descarci versiunea care suportă C# (.NET).
- **.NET SDK**: Necesar pentru compilarea scripturilor C# din Godot.

---

### 2. Instalare și Rulare

#### Backend
1. Navighează în folderul backend: `cd backend-ai`
2. Instalează dependențele: `uv sync`
3. Pornește serverul: `uv run main.py`
   - API-ul va fi disponibil la `http://localhost:8000`

#### Frontend
1. Deschide Godot Engine.
2. Importă proiectul din folderul `frontend-3d/`.
3. Godot va restaura automat pachetele NuGet la prima rulare/compilare.

---

## Planul de Atac (3 Zile)

- **Ziua 1**: Backend-ul API gata + integrare de bază cu Ollama.
- **Ziua 2**: Scena 3D în Godot, fizica rachetei și clientul API (Refit).
- **Ziua 3**: VFX, Shaders, UI și polish final.

---

## Tech Stack
- **Backend**: FastAPI, Pydantic, Ollama, uv.
- **Frontend**: Godot 4 (C#), Refit (pentru API calls), Kenney Assets.
