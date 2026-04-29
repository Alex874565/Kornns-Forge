using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class OrderProgress
{
    public OrderData order;
    public List<CollectedItem> collectedItems = new();

    public bool crafted;

    public float timeRemaining;
    public float maxTime;

    public OrderProgress(OrderData order, float duration)
    {
        this.order = order;
        maxTime = duration;
        timeRemaining = duration;
    }

    public void UpdateTimer(float deltaTime)
    {
        if (crafted) return;

        timeRemaining -= deltaTime;
        timeRemaining = Mathf.Max(timeRemaining, 0f);
    }

    public bool IsExpired()
    {
        return timeRemaining <= 0f;
    }

    public bool IsComplete()
    {
        if (order == null) return false;

        foreach (OrderRequirement req in order.requirements)
        {
            if (GetCollectedCount(req) < req.quantity)
                return false;
        }

        return true;
    }

    public bool NeedsItem(IngredientSO ingredient)
    {
        if (ingredient == null) return false;

        foreach (var req in order.requirements)
        {
            if (req.ingredient != ingredient)
                continue;

            if (GetCollectedCount(req) < req.quantity)
                return true;
        }

        return false;
    }

    public int GetCollectedCount(OrderRequirement req)
    {
        return collectedItems.Count(item =>
            item.ingredient == req.ingredient
        );
    }

    public bool TryAddItem(IngredientSO ingredient)
    {
        if (ingredient == null) return false;
        if (crafted) return false;
        if (IsExpired()) return false;
        if (!NeedsItem(ingredient)) return false;

        collectedItems.Add(new CollectedItem
        {
            ingredient = ingredient
        });

        return true;
    }

    public void MarkCrafted()
    {
        if (IsComplete())
            crafted = true;
    }
}