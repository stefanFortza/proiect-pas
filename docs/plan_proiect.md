# Plan de Dezvoltare: Simulare Multi-Agent "Profesor - Elev - Inspector"

**Durată:** 72 de ore (Sprint)
**Echipă:** 3 persoane
**Arhitectură:** Open-Loop (Backend-ul Python procesează logica AI secvențial; Frontend-ul Godot redă cinematic rezultatele, navigarea spațială și stările de eroare).



## 1. Contractul de Date (Sursa de Adevăr)

Acest JSON reprezintă fundamentul comunicării. Backend-ul calculează totul în avans și trimite rezultatul complet către Godot. Structura expune clar diferența dintre realitatea obiectivă și interpretarea subiectivă a modelului AI.

```json
{
  "adevar_absolut": {
    "obiect_corect": "mar",
    "locatie_corecta": "masa"
  },
  "profesor": {
    "text": "Elevule, adu-mi te rog mărul de pe masa rotundă."
  },
  "elev": {
    "traducere_interna": "Bring the apple from the desk.",
    "intentie_obiect": "mar",
    "intentie_locatie": "birou"
  },
  "inspector": {
    "scor": 5,
    "verdict": "Nota 5. Elevul a identificat corect obiectul (mar), dar a greșit locația (Birou în loc de Masă)."
  }
}
```

---

## 2. Distribuția Sarcinilor

### Membrul 1: Backend & AI Engineer (Python)
Responsabil de orchestrarea modelelor LLM și expunerea datelor structurate.
*   **Tehnologii:** Python, FastAPI, Ollama (Llama 3, Gemma 2b), Pydantic, `uv`.
*   **Ziua 1:** Definirea rutei `GET /api/v1/simulation/step`. Scrierea unui dataset de test intern (*Ground Truth*) care conține perechi predefinite de texte și intenții corecte.
*   **Ziua 2:** Crearea prompt-ului cu *Function Calling* pentru a forța modelul Elevului să returneze strict `intentie_obiect` și `intentie_locatie` în format JSON, fără text adițional care ar invalida parsarea.
*   **Ziua 3:** Configurarea prompt-ului pentru Inspector. Acesta trebuie să primească input-ul de la `adevar_absolut` și `elev`, să le compare și să genereze `scor` și `verdict`. Implementarea tratării excepțiilor (fallback în caz de halucinații severe ale LLM-ului).

### Membrul 2: Systems Programmer (Godot C#)
Responsabil de logica de navigație, parsarea datelor și mașina de stări (State Machine).
*   **Tehnologii:** Godot 4.x, C# (.NET 8), Refit.
*   **Ziua 1:** Instalarea Refit. Generarea claselor C# (DTO) care mapează strict structura JSON. Implementarea unui apel HTTP simplu către backend pentru verificare.
*   **Ziua 2:** Dezvoltarea sistemului de navigație folosind `NavigationAgent3D`. Rezolvarea țintei spațiale direct prin concatenarea datelor: `GetNode($"../NavRegion/{intentie_locatie}/{intentie_obiect}")`.
*   **Ziua 3:** Implementarea mașinii de stări asincrone (`async Task`) pentru redarea temporală secvențială: 
    1. Afișare text Profesor. 
    2. Declanșare deplasare Elev. 
    3. Monitorizare distanță parcursă (`DistanceToTarget()`). 
    4. Afișare text Inspector la finalizarea acțiunii fizice.

### Membrul 3: Technical Artist & Level Designer (Godot)
Responsabil de mediul 3D, interfața utilizator și îndeplinirea cerințelor academice de grafică și fizică.
*   **Tehnologii:** Godot 4.x, Asset-uri Kenney (format `.glb`), Godot Shading Language.
*   **Ziua 1:** Construirea scenei respectând o ierarhie de noduri exactă pentru codul C# (ex: părinte `Masa` -> copil `mar`). Generarea topologiei de navigație (`NavigationRegion3D` - Bake NavMesh) pe suprafața podelei.
*   **Ziua 2:** Crearea elementelor de UI 3D (`Label3D` deasupra capului actorilor) pentru afișarea textelor în timp real. Configurarea camerelor (ex: Phantom Camera) pentru urmărirea Elevului pe traseu.
*   **Ziua 3:** Implementarea elementelor de grafică avansată (Shader custom pe obiecte, post-procesare via `WorldEnvironment` cu Bloom/Color Grading). Crearea unui sistem `GPUParticles3D` care se declanșează dacă codul C# detectează o nepotrivire între `intentie_locatie` și `locatie_corecta`.

---

## 3. Workflow și Reguli Tehnice
*   **Structura Proiectului:** Organizarea sub formă de monorepo cu două directoare izolate: `/backend-ai` și `/frontend-3d`.
*   **Controlul Versiunilor (Git):** Evitarea commit-urilor concurente pe fișierul principal `.tscn` între Membrul 2 și Membrul 3 pentru a preveni conflictele de merge masive. Membrul 2 lucrează exclusiv în fișiere `.cs`, Membrul 3 se ocupă de ierarhia `.tscn`.
*   **Convenții de Denumire:** Numele nodurilor 3D din editorul Godot trebuie să corespundă cu exactitate absolută valorilor extrase de M2 în JSON (ex: `mar`, `minge`, `birou`), fără majuscule inutile, pentru ca funcția de căutare a rutei nodului să nu eșueze.
*   **Automatizare și Debugging:** Se recomandă utilizarea unui *Justfile* pentru pornirea simultană a instanței Uvicorn și a editorului Godot (`just up`). Membrul 2 va folosi instrumente REST (Bruno/Postman) pentru a injecta JSON-uri de test în Godot în timpul dezvoltării, asigurând independența de ritmul de implementare al Membrului 1.