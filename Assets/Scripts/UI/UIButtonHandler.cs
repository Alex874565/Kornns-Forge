using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    public event Action OnHoverEnter;
    public event Action OnHoverExit;
    public event Action OnSelectAction;
    public event Action OnDeselectAction;

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnHoverEnter?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnHoverExit?.Invoke();
    }

    public void OnSelect(BaseEventData eventData)
    {
        OnSelectAction?.Invoke();   
    }

    public void OnDeselect(BaseEventData eventData)
    {
        OnDeselectAction?.Invoke();
    }

    public void ClearHover()
    {
        OnHoverEnter = null;
        OnHoverExit = null;
    }

    public void ClearSelection()
    {
        OnSelectAction = null;
        OnDeselectAction = null;
    }
    
    public void ClearAll()
    {
        ClearHover();
        ClearSelection();
    }
}