using Unity.Netcode;
using UnityEngine;
using System;

public class PlayerStatusController : NetworkBehaviour, IGiveElement, IReceiveElement
{
    public event Action<MaterialData> OnChangeHeldElement;
    
    private int Tiredness { get; set; }
    
    public NetworkVariable<MaterialData> HeldElement { get; private set; } = new (writePerm: NetworkVariableWritePermission.Owner);
    private NetworkVariable<MaterialData>.OnValueChangedDelegate OnChangeHeldElementDelegate { get; set; }

    public override void OnNetworkSpawn()
    {        
        if (!IsOwner) return;
        OnChangeHeldElementDelegate= (oldElement, newElement) =>
        {
            Debug.Log("Held element changed to " + newElement);
            OnChangeHeldElement?.Invoke(newElement);
        };
        HeldElement.OnValueChanged += OnChangeHeldElementDelegate;
        HeldElement.Value = new MaterialData();
    }
    
    public void GiveElement(MaterialData material, IReceiveElement receiver)
    {
        if(!IsOwner) return;
        Debug.Log("Giving " + material);
        HeldElement.Value = new MaterialData();
    }

    public void ReceiveElement(MaterialData material)
    {
        if (!IsOwner) return;
        Debug.Log("Receiving " + material);
        HeldElement.Value = new MaterialData(material);
    }

    public bool CanReceiveElement(MaterialData material)
    {
        if (!IsOwner) return false;
        
        return HeldElement.Value.Type == MaterialType.None;
    }
}