using System;
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ProcessingStationStats", menuName = "ScriptableObjects/Stations/ProcessingStationStats", order = 1)]
public class ProcessingStationStats : ScriptableObject
{
    [field: SerializeField] public List<StationElementData> AcceptedElements { get; private set; }
    [field: SerializeField] public bool IsAutomatic { get; private set; }
}