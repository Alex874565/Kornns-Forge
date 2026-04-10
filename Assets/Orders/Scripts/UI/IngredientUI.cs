using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IngredientUI : MonoBehaviour
{
    public Image materialIcon;
    public Image processIcon;

    public void Setup(OrderRequirement req, int collected)
    {
        materialIcon.sprite = IconDatabase.Instance.GetMaterialIcon(req.materialType);
        processIcon.sprite = IconDatabase.Instance.GetProcessIcon(req.state);

        materialIcon.enabled = true;
        processIcon.enabled = true;

        bool isComplete = collected >= req.quantity;
        Color targetColor = isComplete ? Color.green : Color.white;
        materialIcon.color = targetColor;
        processIcon.color = targetColor;
    }
}