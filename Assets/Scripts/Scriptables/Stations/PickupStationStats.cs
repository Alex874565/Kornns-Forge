using UnityEngine;

[CreateAssetMenu(fileName = "PickupStationStats", menuName = "ScriptableObjects/Stations/PickupStationStats", order = 1)]
public class PickupStationStats : ScriptableObject
{
    [field: SerializeField] public ElementData HeldElement  { get; private set; }
}