using UnityEngine;
using UnityEngine.UI;

public class OrderUI : MonoBehaviour
{
    [Header("Main")]
    [SerializeField] private Image resultIcon;

    [Header("Requirements")]
    [SerializeField] private Transform ingredientsParent;
    [SerializeField] private IngredientUI ingredientPrefab;

    [Header("Timer")]
    [SerializeField] private Image timerBar;

    private OrderProgress progress;

    public void Setup(OrderProgress progress)
    {
        this.progress = progress;

        if (progress == null || progress.order == null)
        {
            ClearUI();
            return;
        }

        OrderData order = progress.order;

        if (resultIcon != null)
        {
            resultIcon.sprite = order.resultIcon;
            resultIcon.enabled = order.resultIcon != null;
            resultIcon.color = progress.crafted ? Color.green : Color.white;
        }

        RefreshRequirements();
        RefreshTimer();
    }

    public void Refresh()
    {
        if (progress == null)
            return;

        if (resultIcon != null)
            resultIcon.color = progress.crafted ? Color.green : Color.white;

        RefreshRequirements();
        RefreshTimer();
    }

    private void RefreshRequirements()
    {
        if (ingredientsParent == null || ingredientPrefab == null)
            return;

        foreach (Transform child in ingredientsParent)
            Destroy(child.gameObject);

        foreach (OrderRequirement req in progress.order.requirements)
        {
            IngredientUI ui = Instantiate(ingredientPrefab, ingredientsParent);

            int collected = progress.GetCollectedCount(req);

            ui.Setup(req, collected);
        }
    }

    private void RefreshTimer()
    {
        if (timerBar == null || progress == null)
            return;

        float timePercent = progress.maxTime <= 0f
            ? 0f
            : progress.timeRemaining / progress.maxTime;

        timePercent = Mathf.Clamp01(timePercent);

        timerBar.fillAmount = timePercent;

        if (timePercent > 0.6f)
            timerBar.color = Color.green;
        else if (timePercent > 0.3f)
            timerBar.color = Color.yellow;
        else
            timerBar.color = Color.red;
    }

    private void ClearUI()
    {
        if (resultIcon != null)
        {
            resultIcon.sprite = null;
            resultIcon.enabled = false;
        }

        if (timerBar != null)
            timerBar.fillAmount = 0f;

        if (ingredientsParent != null)
        {
            foreach (Transform child in ingredientsParent)
                Destroy(child.gameObject);
        }
    }

    private void Update()
    {
        if (progress == null)
            return;

        RefreshTimer();
    }
}