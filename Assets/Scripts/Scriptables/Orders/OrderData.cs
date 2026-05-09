using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "NewOrder", menuName = "Orders/Order")]
public class OrderData : ScriptableObject
{
    [Header("Prefab")]
    public Transform prefab;

    [FormerlySerializedAs("resultIcon")] [Header("UI")]
    public Sprite sprite;

    [Header("Information")]
    public string orderName;
    public int timeToComplete;
    public int reward;

    [Header("Requirements")]
    public List<OrderRequirement> requirements;

    public bool CanCraft(List<IngredientSO> ingredients)
    {
        if (requirements == null || ingredients == null)
            return false;

        // Count provided ingredients
        Dictionary<IngredientSO, int> providedCounts = new();

        int providedTotal = 0;

        foreach (IngredientSO so in ingredients)
        {
            if (so == null)
                continue;

            if (!providedCounts.ContainsKey(so))
                providedCounts[so] = 0;

            providedCounts[so]++;
            providedTotal++;
        }

        // Count required ingredients
        int requiredTotal = 0;

        foreach (OrderRequirement req in requirements)
        {
            if (req == null || req.ingredient == null)
                return false;

            requiredTotal += req.quantity;

            if (!providedCounts.TryGetValue(req.ingredient, out int count))
                return false;

            if (count != req.quantity)
                return false;
        }

        // Reject extra ingredients
        if (providedTotal != requiredTotal)
            return false;

        return true;
    }
}