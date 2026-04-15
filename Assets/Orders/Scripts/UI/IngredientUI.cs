using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IngredientUI : MonoBehaviour
{
    [SerializeField] private IconsDatabase iconsDatabase;
    public Image materialIcon;
    public Image processIcon;

    public void Setup(OrderRequirement req, int collected)
    {
        materialIcon.sprite = iconsDatabase.GetMaterialIcon(req.materialType);
        processIcon.sprite = iconsDatabase.GetProcessIcon(req.state);

        materialIcon.enabled = true;
        processIcon.enabled = true;

        bool isComplete = collected >= req.quantity;
        Color targetColor = isComplete ? Color.green : Color.white;
        materialIcon.color = targetColor;
        processIcon.color = targetColor;
    }
}