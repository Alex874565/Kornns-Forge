using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class OrderUI : MonoBehaviour
{
    public Image resultIcon;
    public Transform ingredientsParent;
    public GameObject ingredientPrefab;

    public Image timerBar;
    private OrderProgress progress;

    public void Setup(OrderProgress progress)
    {
        this.progress = progress;

        OrderData order = progress.order;

        resultIcon.sprite = order.resultIcon;
        resultIcon.color = progress.crafted ? Color.green : Color.white;

        foreach (Transform child in ingredientsParent)
            Destroy(child.gameObject);

        foreach (var req in order.requirements)
        {
            GameObject obj = Instantiate(ingredientPrefab, ingredientsParent);
            IngredientUI ui = obj.GetComponent<IngredientUI>();

            int collected = progress.collectedItems.Count(item =>
                item.Matches(req.itemType, req.materialType, req.state));

            ui.Setup(req, collected);
        }
    }

    private void Update()
    {
        if (progress == null) return;

        float time = progress.timeRemaining / progress.maxTime;

        if (timerBar != null)
            timerBar.fillAmount = time;

        Color color;

        if (time > 0.6f)
            color = Color.green;
        else if (time > 0.3f)
            color = Color.yellow;
        else
            color = Color.red;

        if (timerBar != null)
            timerBar.color = color;
    }
}