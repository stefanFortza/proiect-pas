
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



### Membrul 3 (Date, UI & Decor)

*Responsabilitate: Să facă totul să arate a joc și să gestioneze textele.*

1. **Parsarea JSON:** În Godot, e super simplu. Îi poți arăta cum să folosească `Json.ParseString(jsonString)`. El trebuie să transforme textul de la Membrul 1 într-un dicționar sau un obiect C#.
2. **Bulele de Dialog (Label3D):**
* **Task:** Să pună un nod `Label3D` deasupra capului fiecărui personaj.
* **De ce:** `Label3D` funcționează ca un text normal, dar în spațiu 3D. Nu trebuie să se complice cu Viewport-uri sau UI 2D complex.
* **Logic:** Face o funcție simplă `AfiseazaText(string mesaj)` care schimbă proprietatea `Text` a nodului și îl face vizibil/invizibil.

3. **Meniul de Epoci (CanvasLayer):** Face 3 butoane (Epoca 1, 20, 50). Când apeși un buton, trimite un semnal către tine (Expertul) să încarci setul respectiv de date.
4. **Decorarea Scenei (Diorama):** 
