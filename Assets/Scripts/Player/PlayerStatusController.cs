using Unity.Netcode;
using UnityEngine;
using System;

public class PlayerStatusController : NetworkBehaviour, IGiveElement, IReceiveElement
{
    public event Action<ElementData> OnChangeHeldElement;
    
    private int Tiredness { get; set; }
    
    public NetworkVariable<ElementData> HeldElement { get; private set; } = new (writePerm: NetworkVariableWritePermission.Owner);
    private NetworkVariable<ElementData>.OnValueChangedDelegate OnChangeHeldElementDelegate { get; set; }

    public override void OnNetworkSpawn()
    {        
        if (!IsOwner) return;
        OnChangeHeldElementDelegate= (oldElement, newElement) =>
        {
            Debug.Log("Held element changed to " + newElement);
            OnChangeHeldElement?.Invoke(newElement);
        };
        HeldElement.OnValueChanged += OnChangeHeldElementDelegate;
        HeldElement.Value = new ElementData();
    }
    
    public void GiveElement(ElementData element, IReceiveElement player)
    {
        if(!IsOwner) return;
        Debug.Log("Giving " + element);
        HeldElement.Value = new ElementData();
    }

    public void ReceiveElement(ElementData element)
    {
        if (!IsOwner) return;
        Debug.Log("Receiving " + element);
        HeldElement.Value = new ElementData(element);
    }
}