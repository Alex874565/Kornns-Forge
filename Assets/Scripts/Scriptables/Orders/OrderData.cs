using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewOrder", menuName = "Orders/Order")]
public class OrderData : ScriptableObject
{
    [Header("UI")]
    public Sprite resultIcon;           

    [Header("Information")]
    public string orderName;            
    public string description; /* Text that can maybe be used for dialogue with a customer */
    public int reward;                  

    [Header("Requirements")]
    public List<OrderRequirement> requirements;  
}