using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewOrder", menuName = "Orders/Order")]
public class OrderData : ScriptableObject
{
    [Header("Prefab")]
    public Transform prefab;

    [Header("UI")]
    public Sprite resultIcon;

    [Header("Information")]
    public string orderName;
    public string description;
    public int reward;

    [Header("Requirements")]
    public List<OrderRequirement> requirements;
}