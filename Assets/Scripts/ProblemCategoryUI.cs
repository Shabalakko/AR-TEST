using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ProblemCategoryUI : MonoBehaviour
{
    [Header("Category Header UI")]
    public TextMeshProUGUI headerText;
    public Button expandButton;
    public Image expandIcon; // Opzionale: freccina che ruota
    
    [Header("Parameters Container")]
    public GameObject parametersContainer;
    public GameObject parameterTogglePrefab;

    [Header("Layout Settings")]
    public float minHeaderHeight = 60f;

    private ReportManager.ProblemCode currentProblem;
    private bool isExpanded = false;

    public void Setup(ReportManager.ProblemCode problem)
    {
        currentProblem = problem;
        
        if (headerText != null)
        {
            headerText.text = $"<b>[{problem.Category}]</b> {problem.Code} - {problem.Description}";
        }

        if (expandButton != null)
        {
            expandButton.onClick.RemoveAllListeners();
            expandButton.onClick.AddListener(ToggleExpand);

            // Assicuriamoci che l'header abbia un'altezza minima
            LayoutElement layout = expandButton.GetComponent<LayoutElement>();
            if (layout == null) layout = expandButton.gameObject.AddComponent<LayoutElement>();
            layout.minHeight = minHeaderHeight;
            layout.preferredHeight = minHeaderHeight;
        }

        // Genera i toggle all'interno del contenitore
        GenerateParameters();

        // Di default, teniamo il menu collassato
        SetExpanded(false);
    }

    private void GenerateParameters()
    {
        if (parametersContainer == null)
        {
            Debug.LogError($"[ProblemCategoryUI] parametersContainer non assegnato su {gameObject.name}");
            return;
        }
        if (parameterTogglePrefab == null)
        {
            Debug.LogError($"[ProblemCategoryUI] parameterTogglePrefab non assegnato su {gameObject.name}");
            return;
        }

        // Pulisce figli esistenti
        foreach (Transform child in parametersContainer.transform)
        {
            Destroy(child.gameObject);
        }

        int count = 0;
        foreach (var param in currentProblem.Parameters)
        {
            GameObject go = Instantiate(parameterTogglePrefab, parametersContainer.transform);
            ParameterToggleUI toggleUI = go.GetComponent<ParameterToggleUI>();
            if (toggleUI != null)
            {
                toggleUI.Setup(param);
                count++;
            }
        }
        // Debug.Log($"[ProblemCategoryUI] Generati {count} parametri per {currentProblem.Code}");
    }

    private void ToggleExpand()
    {
        Debug.Log($"[ProblemCategoryUI] Bottone cliccato per {currentProblem.Code}. Stato attuale: {isExpanded}");
        SetExpanded(!isExpanded);
    }

    private void SetExpanded(bool expand)
    {
        isExpanded = expand;
        if (parametersContainer != null)
        {
            parametersContainer.SetActive(isExpanded);
        }

        // Opzionale: ruota l'icona se presente
        if (expandIcon != null)
        {
            expandIcon.transform.localRotation = Quaternion.Euler(0, 0, isExpanded ? -90f : 0f);
        }

        // Forza l'aggiornamento del layout della ScrollView
        LayoutRebuilder.ForceRebuildLayoutImmediate(transform.parent as RectTransform);
    }
}
