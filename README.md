# AR Machine Inspector

Applicazione di realtà aumentata per l'ispezione e la gestione di macchinari industriali, costruita con Unity AR Foundation e XR Interaction Toolkit.

---

## 🛠️ Versione Unity Editor

Questo progetto è sviluppato con:

> **Unity 6000.4.4f1** (Unity 6 LTS)

Assicurati di aprire il progetto esattamente con questa versione per evitare problemi di compatibilità con i package AR Foundation e XR Interaction Toolkit.

---

## 📷 Autorizzazione Telecamera (Importante)

L'applicazione utilizza la **fotocamera del dispositivo** per il tracciamento AR.

### Android
Al primo avvio, il sistema mostrerà una finestra di dialogo che richiede l'accesso alla fotocamera. **Devi premere "Consenti"** per poter utilizzare le funzionalità AR.

Se hai negato l'autorizzazione per errore:
1. Vai su **Impostazioni** → **App**
2. Trova l'app nell'elenco
3. Vai su **Autorizzazioni** → **Fotocamera**
4. Imposta su **Consenti**


> ⚠️ **Senza il permesso fotocamera l'app non funzionerà.**

---

## 📖 Tutorial — Funzionalità Implementate

### 1. 🤖 Menu Macchine Dinamico

Il menu di creazione oggetti è stato implementato per supportare macchine multiple.

**Come si usa:**
1. Premi il pulsante **Crea** in basso al centro per aprire il menu.
2. Nel pannello scorrevole trovi i bottoni delle macchine già salvate e un pulsante **"+"** per aggiungerne una nuova.
3. Tocca il pulsante **"+"** per aggiungere una nuova macchina alla lista.
4. Seleziona una macchina dalla lista per attivarla: il sistema prepara lo spawner.
5. Tocca una superficie AR rilevata per **posizionare la macchina** nella scena.

> Una macchina già posizionata disabilita il suo pulsante nel menu finché non viene eliminata dalla scena.

---

### 2. ✏️ Rinominare una Macchina

Ogni voce del menu ha un pulsante **Rinomina** (icona matita).

**Come si usa:**
1. Apri il menu e premi il pulsante **Rinomina** accanto alla macchina desiderata.
2. Si apre la **tastiera di sistema**: digita il nuovo nome.
3. Il nome sul bottone si aggiorna **in tempo reale** mentre scrivi.
4. Conferma con il tasto **"Invio"** o chiudi la tastiera: il nome viene salvato automaticamente.

> Se lasci il campo vuoto e chiudi la tastiera, viene ripristinato il nome predefinito (es. "Macchina 1").


### 2.1 Evidenziazione dinamica della macchina selezionata

Quando una macchina viene selezionata nella scena AR, viene applicato automaticamente un **effetto di evidenziazione visiva** (Rim Lighting / overlay ologramma).

**Come funziona:**
- Seleziona una macchina toccandola: appare un **alone luminoso** attorno al modello 3D.
- L'effetto è gestito dal componente `MachineHighlighter` presente sul prefab della macchina.
- Funziona su modelli **multi-materiale** e **multi-mesh** complessi.
- Quando deselezioni (tocchi un'altra area), l'effetto si **spegne automaticamente**.

> Il materiale di highlight è configurabile nell'Inspector del prefab tramite il campo `Highlight Material`.

---

### 3. 🗑️ Eliminare una Macchina

Ogni voce del menu ha un pulsante **Elimina** (icona cestino).

**Come si usa:**
1. Apri il menu e premi il pulsante **Elimina** accanto alla macchina.
2. La macchina viene rimossa dalla lista e, se era già stata posizionata nella scena AR, viene anche **distrutta dalla scena**.
3. I dati della checklist associati a quella macchina vengono **cancellati dal disco**.
4. Le macchine successive vengono automaticamente **re-indicizzate** senza perdere i propri dati.

---

### 4. 💾 Persistenza dei Dati

I nomi delle macchine vengono **salvati automaticamente** sul dispositivo tramite `PlayerPrefs`.

- Al prossimo avvio dell'app, le macchine precedentemente create **vengono ripristinate** con i loro nomi personalizzati.
- Non è necessaria nessuna azione da parte dell'utente: il salvataggio avviene in background ad ogni modifica.

---

### 5. ✅ Checklist di Ispezione (ReportManager)

Quando una macchina è selezionata nella scena AR, compare il pulsante **Checklist**.

**Come si usa:**
1. Posiziona e **seleziona una macchina** nella scena (toccala).
2. Premi il pulsante **Checklist** che appare nell'interfaccia.
3. Si apre un pannello con la **scheda di ispezione** della macchina, generata automaticamente dal file CSV `Scheda_01`.
4. Spunta i parametri controllati e premi **Salva** per salvare i dati.
5. La checklist di ogni macchina è **indipendente**: ogni istanza ha il proprio file di salvataggio.

> I dati vengono salvati in formato JSON nella cartella `persistentDataPath` del dispositivo e sopravvivono ai riavvii dell'app.

---

### 6. 🔆 Evidenziazione Visiva della Macchina Selezionata (Highlight)

Quando una macchina viene selezionata nella scena AR, viene applicato automaticamente un **effetto di evidenziazione visiva** (Rim Lighting / overlay ologramma).

**Come funziona:**
- Seleziona una macchina toccandola: appare un **alone luminoso** attorno al modello 3D.
- L'effetto è gestito dal componente `MachineHighlighter` presente sul prefab della macchina.
- Funziona su modelli **multi-materiale** e **multi-mesh** complessi.
- Quando deselezioni (tocchi un'altra area), l'effetto si **spegne automaticamente**.

> Il materiale di highlight è configurabile nell'Inspector del prefab tramite il campo `Highlight Material`.

---

## 📁 Struttura Script Principali

| File | Descrizione |
|---|---|
| `ARTemplateMenuManager.cs` | Gestore principale: menu dinamico, highlight, spawn, checklist |
| `ReportManager.cs` | Caricamento CSV, generazione UI checklist, salvataggio/caricamento JSON |
| `MachineHighlighter.cs` | Effetto visivo di selezione (overlay ologramma) |

---

## 🎨 Ottimizzazione Modelli 3D con Blender

I modelli 3D dei macchinari utilizzati nell'applicazione sono stati **ottimizzati manualmente in Blender** prima dell'importazione in Unity, al fine di garantire prestazioni fluide su dispositivi mobili.

Le principali operazioni effettuate includono:

- **Riduzione del numero di poligoni (Decimate)** — la geometria è stata semplificata eliminando i dettagli non visibili in AR, mantenendo la fedeltà visiva del modello.
- **Pulizia della mesh** — rimozione di vertici doppi, facce interne non necessarie e geometria non manifold che potrebbe causare artefatti visivi o rallentamenti.
- **Ottimizzazione dei materiali** — riduzione del numero di slot materiale per minimizzare i draw call su mobile, fondamentale per mantenere un frame rate stabile.
- **Correzione della scala e delle rotazioni** — i modelli sono stati allineati agli assi corretti e la scala applicata (`Apply Scale/Rotation`) prima dell'esportazione in formato `.fbx` per Unity.

> Queste ottimizzazioni sono fondamentali su dispositivi AR: un modello non ottimizzato può causare cali di frame rate significativi, specialmente in combinazione con il tracciamento AR in tempo reale.

---

## 📝 Note di Build

- Target platform: **Android** (API Level 24+) (in realtà ho testato anche su versioni più vecchie di android, sia di OS che di CPU. Dovrebbe funzionare lo stesso.)
- Richiede dispositivo con supporto **ARCore** (Android) 
- Il permesso `CAMERA` viene richiesto automaticamente al primo avvio
