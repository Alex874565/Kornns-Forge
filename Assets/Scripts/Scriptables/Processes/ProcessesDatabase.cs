using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ProcessesDatabase", menuName = "ScriptableObjects/ProcessesDatabase")]
public class ProcessesDatabase : ScriptableObject
{
    public ProcessesDatabase Instance;
    
    private List<ProcessDatabaseData> processes;

    private void Awake()
    {
        Instance = this;
    }
    
    public Sprite GetProcessIcon(Process process)
    {
        return processes.Find(x => x.process == process)?.icon;
    }
}