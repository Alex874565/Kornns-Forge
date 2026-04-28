using Unity.Netcode;
using UnityEngine;
using System;
using System.Linq;

public class ProcessingStationController : BaseStation, IGiveElement, IReceiveElement
{
    [SerializeField] private ProcessingStationStats stats;

    public event Action OnInteract;
    public event NetworkVariable<StationElementData>.OnValueChangedDelegate OnChangeElement;
    public event NetworkVariable<bool>.OnValueChangedDelegate OnProcessingChanged;
    public event NetworkVariable<float>.OnValueChangedDelegate OnProcessingTimeChanged;
    
    private NetworkVariable<StationElementData> _currentElement = new ();

    private readonly NetworkVariable<bool> _isProcessing =  new ();
    private readonly NetworkVariable<float> _processingTime = new ();

    //public bool InteractOnlyOnce { get; set; }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        
        _currentElement.Value = new StationElementData();
        OnChangeElement += StartProcessing;
        _currentElement.OnValueChanged += OnChangeElement;
        _isProcessing.OnValueChanged += OnProcessingChanged;
        _processingTime.OnValueChanged += OnProcessingTimeChanged;
        
        //InteractOnlyOnce = stats.IsAutomatic;
    }

    public void Update()
    {
        if (!IsOwner) return;
        
        if(_isProcessing.Value)
            Process();
    }

    public override void OnNetworkDespawn()
    {
        if(!IsOwner) return;
        OnChangeElement -= StartProcessing;
        _isProcessing.OnValueChanged -= OnProcessingChanged;
        _processingTime.OnValueChanged -= OnProcessingTimeChanged;
    }

    #region Processing

    private void StartProcessing(StationElementData oldElement, StationElementData newElement)
    {
        Debug.Log("Starting processing " + newElement.Type);
        _currentElement.Value = newElement;
        _isProcessing.Value = true;
        _processingTime.Value = 0f;
    }

    private void Process()
    {
        _processingTime.Value += Time.deltaTime;
        if(_processingTime.Value >= _currentElement.Value.ProcessingTime)
        {
            StopProcessing();
        }
    }

    private void StopProcessing()
    {
        Debug.Log("Finished processing " + _currentElement.Value.Type);
        _isProcessing.Value = false;
    }
    
    #endregion

    #region Give/Receive Element

    public void GiveElement(MaterialData material, IReceiveElement receiver)
    {
        Debug.Log("Giving element " + material.Type + " from station " + gameObject.name);
        receiver.ReceiveElement(material);
        _currentElement.Value = new StationElementData();
    }

    public void ReceiveElement(MaterialData material)
    {
        if (CanReceiveElement(material))
        {
            Debug.Log("Received element " + material.Type + " at station " + gameObject.name);
            _currentElement.Value = new StationElementData(stats.AcceptedElements.Find(e =>
                e.Type == material.Type &&
                e.AcceptedState == material.State));
        }
    }

    public bool CanReceiveElement(MaterialData material)
    {
        return (!stats.IsAutomatic || _isProcessing.Value == false) && stats.AcceptedElements.Any(e => e.Type == material.Type && e.AcceptedState == material.State);
    }

    #endregion
    
    #region Interaction
    
    public override bool CanInteract(PlayerStatusController player)
    {
        // Player takes element
        if (_currentElement.Value.Type != MaterialType.None)
        {
            if(stats.IsAutomatic && _isProcessing.Value) return false;
            
            return player.CanReceiveElement(new MaterialData(_currentElement.Value.Type, _currentElement.Value.FinishedState));
        }
        
        // Player gives element
        return CanReceiveElement(player.HeldElement.Value);
    }
    
    public override void Interact(PlayerStatusController player)
    {
        if(!IsOwner) return;
        
        if (!CanInteract(player)) return;
        
        if (_currentElement.Value.Type == MaterialType.None)
        {
            player.GiveElement(player.HeldElement.Value, this);
        }
        else
        {
            MaterialData materialToGive = new MaterialData(_currentElement.Value.Type, _currentElement.Value.FinishedState);
            GiveElement(materialToGive, player);   
        }
        
        OnInteract?.Invoke();
    }
    
    #endregion
}