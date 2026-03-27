using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class OrderManager : MonoBehaviour
{
    public List<OrderProgress> activeOrders = new List<OrderProgress>();

    public Transform ordersPanel;
    public GameObject orderPrefab;

    public void AddStep(CraftingSteps step)
    {
        /* Find the first incomplete order that needs this step */
        foreach (var progress in activeOrders)
        {
            if (!progress.HasRequiredSteps() && 
                progress.order.requiredSteps.Contains(step) &&
                !progress.completedSteps.Contains(step))
            {
                progress.completedSteps.Add(step);
                break; 
            }
        }

        RefreshUI();
    }

    public void AddItem(ItemType item, MaterialType material)
    {
        ItemRequirements newItem = new ItemRequirements
        {
            itemType = item,
            materialType = material,
            quantity = 1
        };

        /* Check if this order needs this item and has completed all steps */
        foreach (var progress in activeOrders)
        {
            if (NeedsItem(progress, item, material) && progress.HasRequiredSteps())
            {
                progress.collectedItems.Add(newItem);
                break;
            }
        }

        RefreshUI();
    }

    bool NeedsItem(OrderProgress progress, ItemType item, MaterialType material)
    {
        foreach (var req in progress.order.requirements)
        {
            if (req.itemType == item && req.materialType == material)
            {
                /* Check if more items like this need to be crafted */
                int collected = progress.collectedItems
                    .Where(i => i.itemType == item && i.materialType == material)
                    .Sum(i => i.quantity);

                return collected < req.quantity;
            }
        }
        return false;
    }

    public void TryDeliver()
    {
        /* Find first complete order from the list of active orders*/
        for (int i = 0; i < activeOrders.Count; i++)
        {
            if (activeOrders[i].IsComplete())
            {
                Deliver(activeOrders[i]);
                return;
            }
        }
    }

    void Deliver(OrderProgress progress)
    {
        activeOrders.Remove(progress);
        RefreshUI();
    }

    public void RefreshUI()
    {
        foreach (Transform child in ordersPanel)
        {
            Destroy(child.gameObject);
        }

        foreach (var progress in activeOrders)
        {
            GameObject obj = Instantiate(orderPrefab, ordersPanel);
            obj.GetComponent<OrderUI>().Setup(progress);
        }
    }

    public void AddOrder(OrderData order)
    {
        activeOrders.Add(new OrderProgress(order));
        RefreshUI();
    }
}