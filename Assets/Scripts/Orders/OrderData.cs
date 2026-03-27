using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewOrder", menuName = "Orders/Order")]
public class OrderData : ScriptableObject
{
    public string orderName;
    public string description;
    public int reward;

    public List<ItemRequirements> requirements;
    public List<CraftingSteps> requiredSteps;
}

public enum ItemType { Sword, Helmet, Axe }
public enum MaterialType { Iron, Gold }
public enum CraftingSteps { Forge, Melt, Sharpen }

[System.Serializable]
public class ItemRequirements
{
    public ItemType itemType;
    public MaterialType materialType;
    public int quantity = 1;
}