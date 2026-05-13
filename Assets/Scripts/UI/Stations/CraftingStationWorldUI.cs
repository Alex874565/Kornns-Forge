using UnityEngine;
using UnityEngine.UI;

public class CraftingStationWorldUI : MonoBehaviour
{
    [SerializeField] private CraftingStationController craftingStation;
    [SerializeField] private Image[] ingredientImages;
    [SerializeField] private Image craftedImage;

    private void Start()
    {
        craftingStation.OnCraftingChanged += CraftingStation_OnCraftingChanged;

        UpdateVisual();
    }

    private void OnDestroy()
    {
        craftingStation.OnCraftingChanged -= CraftingStation_OnCraftingChanged;
    }

    private void CraftingStation_OnCraftingChanged()
    {
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        // -------- INPUT SLOTS --------
        for (int i = 0; i < ingredientImages.Length; i++)
        {
            IngredientSO ingredient = craftingStation.GetIngredient(i);

            if (ingredient != null)
            {
                ingredientImages[i].gameObject.SetActive(true);
                ingredientImages[i].sprite = ingredient.sprite;
            }
            else
            {
                ingredientImages[i].gameObject.SetActive(false);
            }
        }

        // -------- OUTPUT SLOT --------
        if (craftingStation.HasCrafted())
        {
            OrderData order = craftingStation.GetCraftedOrder();

            if (order != null && order.sprite != null)
            {
                craftedImage.gameObject.SetActive(true);
                craftedImage.sprite = order.sprite;
            }
            else
            {
                craftedImage.gameObject.SetActive(false);
            }
        }
        else
        {
            craftedImage.gameObject.SetActive(false);
        }
    }
}