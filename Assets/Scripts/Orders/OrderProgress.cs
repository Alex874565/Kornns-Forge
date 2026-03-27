using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class OrderProgress
{
    public OrderData order;
    public List<ItemRequirements> collectedItems = new List<ItemRequirements>();
    public List<CraftingSteps> completedSteps = new List<CraftingSteps>();
    public bool stepsCompleted = false;

    public OrderProgress(OrderData order)
    {
        this.order = order;
        this.completedSteps = new List<CraftingSteps>();
        this.stepsCompleted = false;
    }

    public bool HasRequiredSteps()
    {
        foreach (var requiredStep in order.requiredSteps)
        {
            if (!completedSteps.Contains(requiredStep))
                return false;
        }
        return true;
    }

    public bool IsComplete()
    {
        if (!HasRequiredSteps())
            return false;

        foreach (var req in order.requirements)
        {
            int count = collectedItems
                .Where(item => item.itemType == req.itemType &&
                              item.materialType == req.materialType)
                .Sum(item => item.quantity);

            if (count < req.quantity)
                return false;
        }

        return true;
    }
}