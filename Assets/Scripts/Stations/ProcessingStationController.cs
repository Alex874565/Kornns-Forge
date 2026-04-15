using Unity.Netcode;
using UnityEngine;
using System;
using System.Linq;

public class ProcessingStationController : NetworkBehaviour, IAmPlayerInteractable, IGiveElement, IReceiveElement
{
    [SerializeField] private ProcessingStationStats stats;

    public event Action OnHighlight, OnUnHighlight, OnInteract;
    public event NetworkVariable<StationElementData>.OnValueChangedDelegate OnChangeElement;
    public event NetworkVariable<bool>.OnValueChangedDelegate OnProcessingChanged;
    public event NetworkVariable<float>.OnValueChangedDelegate OnProcessingTimeChanged;
    
    private NetworkVariable<StationElementData> _currentElement = new ();

    private readonly NetworkVariable<bool> _isProcessing =  new ();
    private readonly NetworkVariable<float> _processingTime = new ();

    [SerializeField] private SelectedCounterVisual selectedVisual;

    public bool InteractOnlyOnce { get; set; }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        
        _currentElement.Value = new StationElementData();
        OnChangeElement += StartProcessing;
        _currentElement.OnValueChanged += OnChangeElement;
        _isProcessing.OnValueChanged += OnProcessingChanged;
        _processingTime.OnValueChanged += OnProcessingTimeChanged;
        
        InteractOnlyOnce = !stats.IsAutomatic;
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

    public void GiveElement(ElementData element, IReceiveElement receiver)
    {
        Debug.Log("Giving element " + element.Type + " from station " + gameObject.name);
        receiver.ReceiveElement(element);
        _currentElement.Value = new StationElementData();
    }

    public void ReceiveElement(ElementData element)
    {
        Debug.Log("Received element " + element.Type + " at station " + gameObject.name);
        _currentElement.Value = new StationElementData(stats.AcceptedElements.Find(e =>
            e.Type == element.Type &&
            e.AcceptedState == element.State));
    }

    #endregion
    
    #region Interaction
    
    public bool CanInteract(PlayerStatusController player)
    {
        if (_isProcessing.Value)
        {
            if (stats.IsAutomatic || player.HeldElement.Value.Type != ElementType.None)
                return false;

            return true;
        }
        
        if (_currentElement.Value.Type != ElementType.None)
        {
            return true;
        }
        
        ElementData offeredElement = player.HeldElement.Value;
        return stats.AcceptedElements.Any(e =>
            e.Type == offeredElement.Type &&
            e.AcceptedState == offeredElement.State);
    }

    public void Highlight()
    {
        Debug.Log("Highlighting " + gameObject.name);
        OnHighlight?.Invoke();

        selectedVisual.Show();
    }

    public void UnHighlight()
    {
        Debug.Log("Unhighlight " + gameObject.name);
        OnUnHighlight?.Invoke();

        selectedVisual.Hide();
    }
    
    public void Interact(PlayerStatusController player)
    {
        if(!IsOwner) return;
        
        if (!CanInteract(player)) return;
        
        if (_currentElement.Value.Type == ElementType.None)
        {
            player.GiveElement(player.HeldElement.Value, this);
        }
        else
        {
            ElementData elementToGive = new ElementData(_currentElement.Value.Type, _currentElement.Value.FinishedState);
            GiveElement(elementToGive, player);   
        }
        
        OnInteract?.Invoke();
    }
    
    #endregion
}