using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ParameterToggleUI : MonoBehaviour
{
    public TextMeshProUGUI parameterNameText;
    public Toggle valueToggle;
    
    private ReportManager.ProblemParameter currentParameter;

    public void Setup(ReportManager.ProblemParameter param)
    {
        currentParameter = param;
        
        if (parameterNameText != null)
        {
            parameterNameText.text = param.Name;
        }
        
        if (valueToggle != null)
        {
            valueToggle.onValueChanged.RemoveAllListeners();
            valueToggle.isOn = param.Value;
            valueToggle.onValueChanged.AddListener(OnToggleValueChanged);
        }
    }

    private void OnToggleValueChanged(bool newValue)
    {
        if (currentParameter != null)
        {
            currentParameter.Value = newValue;
            Debug.Log($"Valore aggiornato per parametro '{currentParameter.Name}': {newValue}");
        }
    }
}
