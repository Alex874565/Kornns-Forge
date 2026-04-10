using System.Collections.Generic;
using UnityEngine;

public class IconDatabase : MonoBehaviour
{
    public static IconDatabase Instance;

    public List<MaterialIcon> materials;
    public List<ProcessIcon> processes;

    private void Awake()
    {
        Instance = this;
    }

    public Sprite GetMaterialIcon(MaterialType type)
    {
        return materials.Find(x => x.type == type)?.icon;
    }

    public Sprite GetProcessIcon(MaterialProcess process)
    {
        return processes.Find(x => x.process == process)?.icon;
    }
}

[System.Serializable]
public class MaterialIcon
{
    public MaterialType type;
    public Sprite icon;
}

[System.Serializable]
public class ProcessIcon
{
    public MaterialProcess process;
    public Sprite icon;
}