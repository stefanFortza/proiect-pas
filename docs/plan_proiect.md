
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



### Membrul 2: Systems Programmer (Godot C#)

* **Sarcini:**
1. Implementarea fizicii hidrodinamice. Barca este un `RigidBody3D`. Crearea unui script `Buoyancy.cs` care aplică o forță `Vector3.Up` proporțională cu adâncimea sub nivelul mărcii de plutire.
2. Implementarea logicii de instanțiere via HTTP Refit.
3. **Controlul Masei:** La comanda `adauga_cutie`, scriptul instanțiază un Prefab (`Cutie.tscn`), îl plasează în barcă și crește `RigidBody3D.Mass`. La `elimina_cutie`, distruge cel mai vechi copil-cutie și scade masa.



```csharp
public void AplicaDecizieInspector(ActiuneFizica actiune)
{
    if (actiune.Tip == "adauga_cutie")
    {
        Node3D cutie = _prefabCutie.Instantiate<Node3D>();
        _barca.AddChild(cutie);
        _barca.Mass += actiune.MasaKg;
    }
    else if (actiune.Tip == "elimina_cutie" && _barca.GetChildCount() > 0)
    {
        Node3D cutieVeche = _barca.GetChild(0);
        cutieVeche.QueueFree();
        _barca.Mass -= actiune.MasaKg; // Scădem masa corespunzătoare
    }
}

```

### Membrul 3: Technical Artist & UI (Godot)

* **Sarcini:**
1. Designul scenei: Barca detaliată (modele `.glb`), un ponton pentru Inspector, setările globale de mediu.
2. Implementarea sistemului de text UI: Bule de dialog 3D care se actualizează secvențial pentru a afișa textul Profesorului și răspunsul Elevului. O interfață 2D simplă pentru a schimba între Epoci.
3. Crearea Shader-ului de apă. Acesta trebuie să ofere feedback vizual convingător când barca coboară pe axa Y. Adăugarea unui sistem de particule (`GPUParticles3D`) la momentul în care o cutie lovește barca.



---

## 4. Fluxul de Execuție (Sprint)

* **Ziua 1 (Infrastructură):** Python expune JSON-urile. Godot randează barca pe apă și apelează API-ul.
* **Ziua 2 (Mecanică):** Se integrează adăugarea/eliminarea cutiilor și se calibrează masele pentru ca barca să se scufunde realist, fără a fi proiectată în afara scenei din cauza coliziunilor eronate.
* **Ziua 3 (Polish & UI):** Se afișează dialogurile din JSON în Godot și se testează trecerea prin cele 3 "Epoci" pentru validarea vizuală a evoluției M2.