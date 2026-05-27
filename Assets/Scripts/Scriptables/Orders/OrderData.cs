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

        // ---------------- PROVIDED COUNTS ----------------

        Dictionary<IngredientSO, int> providedCounts = new();

        foreach (IngredientSO ingredient in ingredients)
        {
            if (ingredient == null)
                continue;

            if (!providedCounts.ContainsKey(ingredient))
                providedCounts[ingredient] = 0;

            providedCounts[ingredient]++;
        }

        // ---------------- REQUIRED COUNTS ----------------

        Dictionary<IngredientSO, int> requiredCounts = new();

        foreach (OrderRequirement req in requirements)
        {
            if (req == null || req.ingredient == null)
                return false;

            if (!requiredCounts.ContainsKey(req.ingredient))
                requiredCounts[req.ingredient] = 0;

            requiredCounts[req.ingredient] += req.quantity;
        }

        // ---------------- COMPARE ----------------

        if (providedCounts.Count != requiredCounts.Count)
            return false;

        foreach (var pair in requiredCounts)
        {
            IngredientSO ingredient = pair.Key;
            int requiredAmount = pair.Value;

            if (!providedCounts.TryGetValue(ingredient, out int providedAmount))
                return false;

            if (providedAmount != requiredAmount)
                return false;
        }

        return true;
    }
}