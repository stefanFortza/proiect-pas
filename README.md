# Proiect Simulare "Barca Supraviețuirii" (Conversație și Flotabilitate) ⛵

## 📌 Elevator Pitch

Construim o **Simulare 3D Multi-Agent (Closed-Loop Pipeline)** unde supraviețuirea unei bărci depinde direct de calitatea semantică a dialogului dintre agenții AI:
1. **Profesorul (AI 1)**: Pune întrebări tehnice sau academice.
2. **Elevul (AI 2)**: Încearcă să răspundă coerent (evoluând prin 3 etape de antrenament).
3. **Inspectorul (AI 3)**: Evaluează calitatea răspunsului (Coerență, Relevanță, Gramatică) și dictează o acțiune fizică: adăugarea sau eliminarea de balast (cutii) în barcă.

**Mecanica principală:** Masa bărcii crește cu fiecare răspuns slab, forțând-o să se scufunde conform legilor fizicii hidrodinamice.

## 🏗️ Arhitectură
* **Backend (Python/FastAPI):** Evaluare NLP, prompt engineering pentru Inspector, generare snapshot-uri de date pentru cele 3 "Epoci".
* **Frontend (Godot 4 C#):** Simulare fizică `RigidBody3D`, script de *Buoyancy* (flotabilitate), instanțiere dinamică de obiecte via HTTP (Refit).

---

## 🎭 Metodologia de Prezentare (Evoluția pe Epoci)
Demonstrația se bazează pe 3 stadii de evoluție a Elevului:
* **Epoca 1 (Incoerent):** Răspunsuri aberante -> Barca se scufundă rapid sub greutatea balastului.
* **Epoca 20 (Mediu):** Noțiuni de bază -> Barca plutește periculos de jos, la limita submersiei.
* **Epoca 50 (Optimizat):** Dialog impecabil -> Inspectorul elimină balastul, barca plutește sus pe valuri.

---

## 🚀 Getting Started

### 1. Prerechizite
* **Python 3.12+** & **uv** (`curl -LsSf https://astral.sh/uv/install.sh | sh`)
* **Ollama** (cu modele locale descărcate)
* **Godot Engine 4.x (.NET Edition)** & **.NET 8 SDK**
* **just** (pentru automatizare)

### 2. Rulare
* Backend: `cd backend-ai && uv run uvicorn main:app --reload` (sau `just up`)
* Frontend: Deschide proiectul în Godot și apasă F5.

---

## 📅 Plan de Dezvoltare (72h)
* **Ziua 1 (Infrastructură):** Backend API (JSON snapshots) & Godot setup (Barca pe apă + client API).
* **Ziua 2 (Mecanică):** Integrare fizică hidrodinamică, calibrare mase și logica de adăugare/eliminare cutii.
* **Ziua 3 (Polish & UI):** Bule de dialog 3D, Water Shader, sistem de particule și selector de Epoci.

---

## 🛠️ Tech Stack
* **AI:** Ollama, Llama 3 / Gemma.
* **Backend:** FastAPI, Python, Pydantic.
* **Frontend:** Godot 4 (C#), Refit, Kenney Assets (Food & Furniture kits).
