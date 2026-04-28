using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class IngredientUI : MonoBehaviour
{
    [SerializeField] private MaterialsDatabase materialsDatabase;
    [SerializeField] private ProcessesDatabase processesDatabase;
    public Image materialIcon;
    public Image processIcon;

    public void Setup(OrderRequirement req, int collected)
    {
        materialIcon.sprite = materialsDatabase.GetMaterialIcon(req.materialType);
        processIcon.sprite = processesDatabase.GetProcessIcon(req.state);

        materialIcon.enabled = true;
        processIcon.enabled = true;

        bool isComplete = collected >= req.quantity;
        Color targetColor = isComplete ? Color.green : Color.white;
        materialIcon.color = targetColor;
        processIcon.color = targetColor;
    }
}