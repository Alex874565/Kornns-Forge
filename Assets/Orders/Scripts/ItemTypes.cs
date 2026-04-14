public enum ItemType
{
    
    Material,   /* Raw or processed materials */
    Sword,      /* Not used currently, can be used after crafting an item */
    Helmet,     /* Not used currently, can be used after crafting an item */
    Axe         /* Not used currently, can be used after crafting an item */
}

public enum MaterialType
{
    Iron,
    Silver,
    Gold,
    Copper,
    Diamond,
    Wood
}

public enum MaterialProcess
{
    Melt,       /* Furnance */
    Straighten, /* Anvil */
    Chop        /* Wood crafting table */
}

[System.Serializable]
public class OrderRequirement
{
    public ItemType itemType;
    public MaterialType materialType;
    public MaterialProcess state;
    /* Always one item */
    public int quantity = 1;   

    public bool Matches(ItemType type, MaterialType mat, MaterialProcess proc)
    {
        return itemType == type && materialType == mat && state == proc;
    }
}

[System.Serializable]
public class CollectedItem
{
    public ItemType itemType;
    public MaterialType materialType;
    public MaterialProcess state;

    public bool Matches(ItemType type, MaterialType mat, MaterialProcess proc)
    {
        return itemType == type && materialType == mat && state == proc;
    }
}