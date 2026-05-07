### 📌 Despre ce este proiectul nostru? (Elevator Pitch)

Construim o **Simulare 3D Multi-Agent (Embodied AI)**. Pe scurt, creăm un "laborator" virtual în care 3 modele de Inteligență Artificială comunică între ele pentru a rezolva un task fizic, iar noi vizualizăm totul într-un motor grafic.

**Scenariul este simplu:**
Avem o cameră 3D cu două mese (Birou și Masă) și câteva obiecte pe ele (măr, carte, etc.). 
1. **Profesorul (AI 1)** dă o comandă text: *"Adu-mi mărul de pe birou."*
2. **Elevul (AI 2)** analizează textul și trebuie să extragă logic intenția (ce obiect trebuie să ia și de unde).
3. **Godot (Motorul 3D)** preia intenția Elevului și mișcă fizic un personaj către obiectul respectiv.
4. **Inspectorul (AI 3)** analizează dacă Elevul s-a dus la locul corect și a luat obiectul corect, dându-i o notă.

**Cum o construim tehnic? (Arhitectură Decuplată)**
* **Creierul (Backend - Python/FastAPI):** Aici rulează modelele locale (Ollama). Python face toată logica NLP, calculează scenariul cap-coadă și împachetează totul într-un singur fișier JSON.
* **Corpul (Frontend - Godot C#):** Godot nu face calcule AI. El doar cere JSON-ul de la Python și îl "joacă" pe ecran ca pe o piesă de teatru. Citește destinația, folosește sistemul de navigație (`NavigationAgent3D`) pentru a mișca robotul și aplică efecte vizuale (particule, shadere) în funcție de cât de mult a greșit AI-ul.

**De ce am ales abordarea asta?**
Este strict tehnică, eficientă și ne garantează că o terminăm în 3 zile. Separând complet logica (Python) de grafică (Godot), eliminăm riscul ca motorul 3D să se blocheze așteptând răspunsul de la AI. În plus, demonstrează concepte academice avansate: *Semantic Parsing* (extragere de date din text), *Sisteme Distribuite* (Client-Server) și *Cinematică/Navigație 3D*.



# Proiect Simulare Limbaj (Hackathon Mode 🚀)

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

#### Utilitare (Workflow)
- **just**: Command runner modern pentru automatizarea task-urilor.
  - *Instalare (Ubuntu):* `sudo apt install just`
  - *Instalare (macOS):* `brew install just`
  - *Instalare (Windows):* `winget install casey.just`

---

### 2. Instalare și Rulare

#### Varianta Rapidă (Recomandat) 🚀
Dacă ai instalat `just`, poți folosi următoarele comenzi din rădăcina proiectului:
- `just install` - Instalează toate dependențele (Python + .NET).
- `just up` - Pornește API-ul și deschide editorul Godot simultan.
- `just api` - Rulează doar backend-ul.
- `just editor` - Deschide doar editorul Godot.
- `just clean` - Șterge cache-urile și fișierele temporare.

#### Varianta Manuală
##### Backend
1. Navighează în folderul backend: `cd backend-ai`
2. Instalează dependențele: `uv sync`
3. Pornește serverul: `uv run uvicorn main:app --reload`
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
