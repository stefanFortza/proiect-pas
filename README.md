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

# Plan de Dezvoltare: Simulare Multi-Agent "Barca Supraviețuirii" (Conversație și Flotabilitate)

**Durată:** 72 de ore (Sprint)
**Echipă:** 3 persoane
**Arhitectură:** Closed-Loop Pipeline. Backend-ul evaluează calitatea semantică a conversației, iar Frontend-ul traduce scorul într-o mecanică de *Buoyancy* (flotabilitate) bazată pe masă.

---

## 1. Contractul de Date și Evaluarea NLP

Modelul M3 (Inspectorul)  evaluează răspunsul Elevului (M2) pe baza a trei metrici: *Coerență*, *Relevanță la subiect* și *Corectitudine gramaticală*. Scorul dictând greutatea balastului.

**JSON generat de API pentru fiecare din cele 15 iterații:**

```json
{
  "iteratie": 4,
  "profesor": {
    "text": "Explică-mi cum funcționează un Garbage Collector în programare."
  },
  "elev": {
    "text": "Garbage collector este o mașină de gunoi care curăță memoria când calculatorul se umple de praf."
  },
  "inspector": {
    "scor_calitate": 0.2,
    "verdict": "Răspuns ilogic, halucinație semantică.",
    "actiune_fizica": {
      "tip": "adauga_cutie",
      "masa_kg": 50.0
    }
  }
}

```

---

## 2. Metodologia de Prezentare (Evoluția pe Epoci)

Pentru prezentarea academică, demonstrația evoluției limbajului se va face încărcând 3 seturi de date (snapshot-uri) care reprezintă stadii diferite de antrenament/prompting ale M2:

* **Epoca 1 (Model Incoerent):** Elevul dă răspunsuri aberante sau pe lângă subiect. *Rezultat vizual:* Inspectorul aruncă constant cutii grele în barcă. La iterația 7 sau 8, masa totală depășește forța arhimedică, iar barca se scufundă complet.
* **Epoca 20 (Model Mediu):** Elevul are noțiuni de bază, dar formulează greoi. *Rezultat vizual:* Inspectorul adaugă cutii medii, dar mai și aruncă afară când elevul nimerește un răspuns bun. Barca ajunge la iterația 15, dar plutește periculos de jos.
* **Epoca 50 (Model Optimizat):** Elevul susține o conversație tehnică impecabilă. *Rezultat vizual:* Inspectorul extrage balastul inițial. Barca termină cele 15 dialoguri goală, plutind sus pe valuri.

---

## 3. Distribuția Sarcinilor Tehnice

### Membrul 1: AI & Backend Engineer (Python)

* **Sarcini:**
1. Configurarea pipeline-ului FastAPI și integrarea modelelor locale.
2. Proiectarea prompt-ului pentru M3 (Inspector). Acesta trebuie instruit să acționeze ca un evaluator academic sever, care penalizează devierile de la subiect.
3. Transformarea scorului NLP (0.0 - 1.0) într-o valoare fizică (`masa_kg`). Exemplu de logică: Dacă `scor > 0.7` -> `actiune: elimina_cutie`. Dacă `scor < 0.5` -> `actiune: adauga_cutie`, unde greutatea crește invers proporțional cu scorul.
4. Generarea fișierelor JSON statice pentru Epoca 1, 20 și 50.


### Membrul 2 (Sisteme Nucleu & Fizică)

*Responsabilitate: Să se asigure că barca plutește, shaderul funcționează și cutiile au impact fizic.*

1. **Sistemul de Plutire (Buoyancy):** Scrii scriptul C# care aplică forțe pe barcă în funcție de adâncimea apei și masa curentă. Tu calibrezi masa bărcii astfel încât să nu "decoleze" din greșeală.
2. **Integrare Shader:** Tu pui shaderul de apă pe care l-am discutat. Te asiguri că `MeshInstance3D` (Plane) are destule subdiviziuni ca să arate bine.
3. **Mecanica de "Aruncare":** Faci logica prin care Inspectorul instanțiază cutia și îi aplică un `Impulse` către barcă. Ești responsabil de coliziunile dintre cutie și barcă.
4. **Orchestratorul (The Game Loop):** Tu scrii mașina de stări principală:
* Așteaptă semnal de la UI -> Apelează API-ul -> Trimite datele către Membrul 3 (pentru text) -> Declanșează animația Inspectorului (aruncarea).

### Epic: UI, Data Parsing & Set Dressing (Member 3)

**Responsabilitate principală:** Implementarea feedback-ului vizual, deserializarea datelor primite de la backend și amenajarea mediului 3D.

---

#### 📌 Task 1: JSON Parsing & Data Contracts

**Obiectiv:** Transformarea string-ului JSON primit de la API într-o structură de date utilizabilă în C#. Folosim clasa `System.Text.Json` pentru mapare automată.

**Pași de execuție:**

1. Creează un script C# nou numit `DataContracts.cs`. Acest fișier nu trebuie atașat niciunui nod, va conține doar definițiile claselor.
2. Definește structura claselor care oglindește exact cheile din JSON.

**Cod de implementare (`DataContracts.cs`):**

```csharp
using System.Text.Json.Serialization;

public class StudentData
{
    [JsonPropertyName("text")]
    public string Text { get; set; }
}

public class TeacherData
{
    [JsonPropertyName("text")]
    public string Text { get; set; }
}

public class InspectorResult
{
    [JsonPropertyName("quality_score")]
    public float QualityScore { get; set; }
    
    [JsonPropertyName("verdict")]
    public string Verdict { get; set; }
}

public class DialogRound
{
    [JsonPropertyName("iteration")]
    public int Iteration { get; set; }
    
    [JsonPropertyName("teacher")]
    public TeacherData Teacher { get; set; }
    
    [JsonPropertyName("student")]
    public StudentData Student { get; set; }
    
    [JsonPropertyName("inspector")]
    public InspectorResult Inspector { get; set; }
}

```

---

#### 📌 Task 2: 3D Dialog Bubbles (Label3D)

**Obiectiv:** Afișarea textului în spațiul 3D, deasupra personajelor, folosind un nod dedicat textului spațial.

**Pași de execuție (Configurare Noduri):**

1. Deschide scena fiecărui personaj (`Teacher.tscn`, `Student.tscn`, `Inspector.tscn`).
2. Adaugă un nod copil de tip **`Label3D`**. Numește-l `DialogBubble`.
3. Mută nodul pe axa Y (în sus) până ajunge deasupra capului personajului.
4. În panoul *Inspector*, configurează următoarele:
* **Billboard:** Setează pe `Enabled` (astfel textul privește mereu spre cameră).
* **Fixed Size:** Bifează (pentru a nu se micșora când camera se îndepărtează).
* **Outline Size:** Pune valoarea `4` și alege culoarea neagră la *Outline Color*.
* **Text:** Lasă-l gol implicit.

**Cod de implementare (Atașat pe rădăcina personajului, ex: `ActorUI.cs`):**

```csharp
using Godot;

public partial class ActorUI : Node3D
{
    private Label3D _dialogBubble;

    public override void _Ready()
    {
        // Preluăm referința către nodul creat în editor
        _dialogBubble = GetNode<Label3D>("DialogBubble");
        _dialogBubble.Hide(); // Ascundem textul la pornire
    }

    public void ShowText(string message)
    {
        _dialogBubble.Text = message;
        _dialogBubble.Show();
        
        // Opțional: Ascunde textul după 4 secunde
        GetTree().CreateTimer(4.0f).Timeout += () => _dialogBubble.Hide();
    }
}

```

---

#### 📌 Task 3: Epoch Selection Menu (UI 2D)

**Obiectiv:** Crearea interfeței 2D suprapuse peste mediul 3D pentru a permite selecția modelului AI (Epoch 1, 20, 50).

**Pași de execuție (Configurare Noduri):**

1. Adaugă un nod **`CanvasLayer`** în scena principală (acesta randează UI-ul independent de camera 3D). Numește-l `UIManager`.
2. Adaugă un nod **`MarginContainer`** ca fiu. În *Inspector* -> *Layout*, setează *Anchors Preset* pe `Top Wide`.
3. Adaugă un nod **`HBoxContainer`** ca fiu (aliniază butoanele orizontal).
4. Adaugă 3 noduri **`Button`** și redenumește-le: `BtnEpoch1`, `BtnEpoch20`, `BtnEpoch50`.

**Cod de implementare (Atașat pe nodul `UIManager.cs`):**

```csharp
using Godot;

public partial class UIManager : CanvasLayer
{
    // Definim un delegat (semnal) pe care îl va asculta Membrul 2 (Expertul)
    [Signal]
    public delegate void EpochSelectedEventHandler(int epochNumber);

    public override void _Ready()
    {
        // Conectăm butoanele la metoda de selecție
        GetNode<Button>("MarginContainer/HBoxContainer/BtnEpoch1").Pressed += () => SelectEpoch(1);
        GetNode<Button>("MarginContainer/HBoxContainer/BtnEpoch20").Pressed += () => SelectEpoch(20);
        GetNode<Button>("MarginContainer/HBoxContainer/BtnEpoch50").Pressed += () => SelectEpoch(50);
    }

    private void SelectEpoch(int epoch)
    {
        GD.Print($"Selected Epoch: {epoch}");
        // Emitem semnalul către GameLoop-ul principal
        EmitSignal(SignalName.EpochSelected, epoch);
    }
}

```

---

#### 📌 Task 4: Scene Decoration

## 🛠️ Tech Stack

* **AI:** Ollama, Llama 3 / Gemma.
* **Backend:** FastAPI, Python, Pydantic.
* **Frontend:** Godot 4 (C#), Refit, Kenney Assets (Food & Furniture kits).
