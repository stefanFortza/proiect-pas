# Proiect Simulare Multi-Agent "Profesor - Elev - Inspector" (Hackathon Mode 🚀)

## 📌 Elevator Pitch

Construim o **Simulare 3D Multi-Agent (Embodied AI)** unde 3 modele de Inteligență Artificială comunică pentru a rezolva un task fizic:
1. **Profesorul (AI 1)** dă o comandă text (ex: *"Adu-mi mărul de pe masă"*).
2. **Elevul (AI 2)** extrage intenția (obiect și locație) și se mișcă în mediul 3D.
3. **Inspectorul (AI 3)** evaluează dacă acțiunea elevului corespunde cu "Adevărul Absolut".

## 🏗️ Arhitectură
* **Backend (Python/FastAPI):** Logica AI secvențială, orchestrare Ollama.
* **Frontend (Godot 4 C#):** Vizualizare cinematică, navigație 3D (`NavigationAgent3D`), VFX.

---

## 🚀 Getting Started

### 1. Prerechizite
* **Python 3.12+** & **uv** (`curl -LsSf https://astral.sh/uv/install.sh | sh`)
* **Ollama** (cu `llama3` descărcat)
* **Godot Engine 4.x (.NET Edition)** & **.NET 8 SDK**
* **just** (opțional, pentru automatizare)

### 2. Rulare
* `just up` (sau `uv run uvicorn main:app --reload` în `backend-ai`)
* API-ul rulează la `http://localhost:8000`

---

## 📅 Plan de Dezvoltare (72h)
* **Ziua 1**: Backend API & Contract de Date.
* **Ziua 2**: Navigație Godot & Client API.
* **Ziua 3**: VFX, UI & Final Polish.

---

## 🛠️ Tech Stack
* **AI:** Ollama, Llama 3 / Gemma 2b.
* **Backend:** FastAPI, Pydantic, uv.
* **Frontend:** Godot 4 (C#), Refit, Kenney Assets.
