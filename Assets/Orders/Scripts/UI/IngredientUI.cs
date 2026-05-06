using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IngredientUI : MonoBehaviour
{
    [SerializeField] private Image ingredientIcon;
    [SerializeField] private Image processIcon;
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

        // Ingredient icon
        if (ingredientIcon != null)
        {
            ingredientIcon.sprite = req.ingredient.sprite;
            ingredientIcon.enabled = req.ingredient.sprite != null;
            ingredientIcon.color = targetColor;
        }

        // Process icon
        if (processIcon != null)
        {
            if (req.processIcon != null)
            {
                processIcon.sprite = req.processIcon;
                processIcon.enabled = req.processIcon != null;
                processIcon.color = targetColor;
            }
            else
            {
                processIcon.color = new Color(0, 0, 0, 0);
            }
        }

        // Amount text
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

        if (processIcon != null)
        {
            processIcon.sprite = null;
            processIcon.enabled = false;
        }

        if (amountText != null)
            amountText.text = "";
    }
}