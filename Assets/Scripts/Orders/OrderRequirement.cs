[System.Serializable]
public class OrderRequirement
{
    public ItemType itemType;
    public MaterialType materialType;
    public Process state;
    /* Always one item */
    public int quantity = 1;   

    public bool Matches(ItemType type, MaterialType mat, Process proc)
    {
        return itemType == type && materialType == mat && state == proc;
    }
}

[System.Serializable]
public class CollectedItem
{
    public ItemType itemType;
    public MaterialType materialType;
    public Process state;

    public bool Matches(ItemType type, MaterialType mat, Process proc)
    {
        return itemType == type && materialType == mat && state == proc;
    }
}