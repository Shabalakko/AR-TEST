using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ReportManager : MonoBehaviour
{
    [System.Serializable]
    public class ProblemParameter
    {
        public string Name;
        public bool Value;
        public int ColumnIndex; // Optional: To keep track of where it came from
    }

    [System.Serializable]
    public class ProblemCode
    {
        public string Code;
        public string Category;
        public string Description;
        public List<ProblemParameter> Parameters = new List<ProblemParameter>();
    }

    [System.Serializable]
    public class ReportData
    {
        public List<ProblemCode> Problems = new List<ProblemCode>();
    }

    [Header("UI References")]
    public Transform contentPanel;
    public GameObject problemCategoryPrefab; // Prefab with ProblemCategoryUI
    public Button saveButton;

    [Header("File Names")]
    public string csvResourceName = "Scheda_01";
    public string savedFileName = "Scheda_01_Saved.json";

    private ReportData reportData = new ReportData();
    public string CurrentMachineID { get; private set; } = "Default";
    private string SaveFilePath => GetSaveFilePath(CurrentMachineID);

    private string GetSaveFilePath(string machineID) 
    {
        return Path.Combine(Application.persistentDataPath, $"Scheda_01_{machineID}_Saved.json");
    }

    void Start()
    {
        Debug.Log("[ReportManager] Start");
        if (saveButton != null)
        {
            saveButton.onClick.AddListener(SaveData);
        }
        else
        {
            Debug.LogWarning("[ReportManager] saveButton non assegnato nell'Inspector.");
        }
        
        LoadData();
        GenerateUI();
    }

    public void InitializeForMachine(string machineID)
    {
        Debug.Log($"[ReportManager] Inizializzazione per macchina: {machineID}");
        CurrentMachineID = machineID;
        LoadData();
        GenerateUI();
    }

    private void LoadData()
    {
        // Reset data before loading
        reportData = new ReportData();
        
        if (File.Exists(SaveFilePath))
        {
            try
            {
                string json = File.ReadAllText(SaveFilePath);
                reportData = JsonUtility.FromJson<ReportData>(json);
                if (reportData != null && reportData.Problems != null && reportData.Problems.Count > 0)
                {
                    Debug.Log($"[ReportManager] Dati caricati dal JSON: {reportData.Problems.Count} problemi trovati.");
                    return;
                }
                Debug.LogWarning("[ReportManager] Il file JSON esiste ma sembra vuoto o non valido. Carico dal CSV...");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[ReportManager] Errore caricamento JSON: {e.Message}");
            }
        }

        TextAsset csvFile = Resources.Load<TextAsset>(csvResourceName);
        if (csvFile != null)
        {
            ParseCSV(csvFile.text);
            Debug.Log($"[ReportManager] Dati caricati dal CSV: {reportData.Problems.Count} problemi trovati.");
        }
        else
        {
            Debug.LogError($"[ReportManager] Impossibile trovare {csvResourceName} in Resources!");
        }
    }

    private void ParseCSV(string csvText)
    {
        reportData.Problems.Clear();
        string[] lines = csvText.Split(new[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);

        if (lines.Length < 7) return; // Non abbastanza righe per la struttura attesa

        // Riga 6 (indice 5) ha le intestazioni principali
        // Riga 7 (indice 6) ha le sotto-intestazioni (es. 0.2, 0.5)
        string[] row6 = lines[5].Split(',');
        string[] row7 = lines[6].Split(',');

        List<string> headers = new List<string>();
        string currentMainHeader = "";

        // Ricostruiamo i nomi delle colonne dalla colonna 4 (Visto) in poi
        for (int i = 0; i < row6.Length; i++)
        {
            string h6 = row6[i].Trim();
            string h7 = (i < row7.Length) ? row7[i].Trim() : "";

            if (!string.IsNullOrEmpty(h6))
            {
                currentMainHeader = h6;
            }

            string finalHeader = currentMainHeader;
            if (!string.IsNullOrEmpty(h7))
            {
                finalHeader += " " + h7;
            }

            headers.Add(finalHeader);
        }

        string currentCategory = "";

        // Dalla riga 8 (indice 7) in poi ci sono i dati veri e propri
        for (int i = 7; i < lines.Length; i++)
        {
            string line = lines[i];
            string[] columns = line.Split(',');

            if (columns.Length < 4) continue; // Salta righe malformate
            if (string.IsNullOrEmpty(columns[1].Trim())) continue; // Se non c'è un Codice Difetto, salta (es. riga "Note elemento")

            string code = columns[1].Trim();
            string category = columns[2].Trim();
            string description = columns[3].Trim();

            if (!string.IsNullOrEmpty(category))
            {
                currentCategory = category;
            }

            ProblemCode problem = new ProblemCode()
            {
                Code = code,
                Category = currentCategory,
                Description = description,
                Parameters = new List<ProblemParameter>()
            };

            // Controlla tutte le colonne dalla 4 in poi per cercare "FALSE"
            for (int col = 4; col < columns.Length; col++)
            {
                string val = columns[col].Trim().ToUpper();
                if (val == "FALSE")
                {
                    string paramName = (col < headers.Count) ? headers[col] : $"Parametro {col}";
                    
                    ProblemParameter param = new ProblemParameter()
                    {
                        Name = paramName,
                        Value = false,
                        ColumnIndex = col
                    };
                    problem.Parameters.Add(param);
                }
            }

            // Aggiunge il problema solo se ha almeno un parametro editabile
            if (problem.Parameters.Count > 0)
            {
                reportData.Problems.Add(problem);
            }
        }
    }

    private void GenerateUI()
    {
        if (contentPanel == null)
        {
            Debug.LogError("[ReportManager] contentPanel non assegnato nell'Inspector!");
            return;
        }
        if (problemCategoryPrefab == null)
        {
            Debug.LogError("[ReportManager] problemCategoryPrefab non assegnato nell'Inspector!");
            return;
        }

        Debug.Log($"[ReportManager] Generazione UI per {reportData.Problems.Count} problemi...");

        if (contentPanel.GetComponent<VerticalLayoutGroup>() == null)
        {
            Debug.LogWarning("[ReportManager] ATTENZIONE: contentPanel non ha un VerticalLayoutGroup. Gli oggetti istanziati potrebbero sovrapporsi o non essere visibili correttamente.");
        }
        if (contentPanel.GetComponent<ContentSizeFitter>() == null)
        {
            Debug.LogWarning("[ReportManager] ATTENZIONE: contentPanel non ha un ContentSizeFitter. La ScrollView non si espanderà automaticamente e gli oggetti potrebbero non apparire.");
        }

        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }

        int count = 0;
        foreach (var problem in reportData.Problems)
        {
            GameObject go = Instantiate(problemCategoryPrefab, contentPanel);
            ProblemCategoryUI ui = go.GetComponent<ProblemCategoryUI>();
            if (ui != null)
            {
                ui.Setup(problem);
                count++;
            }
            else
            {
                Debug.LogError("[ReportManager] Il prefab non contiene il componente ProblemCategoryUI!");
            }
        }
        Debug.Log($"[ReportManager] UI Generata: {count} oggetti istanziati.");
    }

    public void SaveData()
    {
        string json = JsonUtility.ToJson(reportData, true);
        File.WriteAllText(SaveFilePath, json);
        Debug.Log("Dati salvati con successo in: " + SaveFilePath);
    }

    public void DeleteDataForMachine(string machineID)
    {
        string path = GetSaveFilePath(machineID);
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"[ReportManager] File eliminato per {machineID}: {path}");
        }
    }

    public void RenameDataForMachine(string oldID, string newID)
    {
        string oldPath = GetSaveFilePath(oldID);
        string newPath = GetSaveFilePath(newID);

        if (File.Exists(oldPath))
        {
            if (File.Exists(newPath)) File.Delete(newPath); // Overwrite if exists
            File.Move(oldPath, newPath);
            Debug.Log($"[ReportManager] File rinominato da {oldID} a {newID}");
        }
    }
}
