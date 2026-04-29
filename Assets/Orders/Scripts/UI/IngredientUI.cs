using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IngredientUI : MonoBehaviour
{
    [SerializeField] private Image ingredientIcon;
    [SerializeField] private TextMeshProUGUI amountText;

    public void Setup(OrderRequirement req, int collected)
    {
        if (req == null || req.ingredient == null)
        {
            Clear();
            return;
        }

        bool isComplete = collected >= req.quantity;
        Color targetColor = isComplete ? Color.green : Color.white;

        ingredientIcon.sprite = req.ingredient.sprite;
        ingredientIcon.enabled = true;
        ingredientIcon.color = targetColor;

        if (amountText != null)
        {
            amountText.text = $"{collected}/{req.quantity}";
            amountText.color = targetColor;
        }
    }

    private void Clear()
    {
        if (ingredientIcon != null)
        {
            ingredientIcon.sprite = null;
            ingredientIcon.enabled = false;
        }

        if (amountText != null)
            amountText.text = "";
    }
}