using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MaterialsDatabase", menuName = "ScriptableObjects/MaterialsDatabase")]
public class MaterialsDatabase : ScriptableObject
{
    public static MaterialsDatabase Instance;

    public List<MaterialDatabaseData> materials;

    private void Awake()
    {
        Instance = this;
    }

    public Sprite GetMaterialIcon(MaterialType type)
    {
        return materials.Find(x => x.type == type)?.icon;
    }
}

