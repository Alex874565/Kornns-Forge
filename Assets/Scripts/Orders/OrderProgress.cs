using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class OrderProgress
{
    public OrderData order;
    public List<CollectedItem> collectedItems = new List<CollectedItem>();
    public bool crafted = false;

    public float timeRemaining;
    public float maxTime;

    public OrderProgress(OrderData order, float duration)
    {
        this.order = order;
        this.maxTime = duration;
        this.timeRemaining = duration;
    }

    public void UpdateTimer(float deltaTime)
    {
        timeRemaining -= deltaTime;
    }

    public bool IsExpired()
    {
        return timeRemaining <= 0f;
    }

    public bool IsComplete()
    {
        foreach (var req in order.requirements)
        {
            int collectedCount = collectedItems.Count(item =>
                item.Matches(req.itemType, req.materialType, req.state));

            if (collectedCount < req.quantity)
                return false;
        }
        return true;
    }

    public bool NeedsItem(ItemType type, MaterialType mat, Process proc)
    {
        foreach (var req in order.requirements)
        {
            if (req.Matches(type, mat, proc))
            {
                int collectedCount = collectedItems.Count(item =>
                    item.Matches(type, mat, proc));

                if (collectedCount < req.quantity)
                    return true;
            }
        }
        return false;
    }
}