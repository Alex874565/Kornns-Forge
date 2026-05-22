using UnityEngine;
using UnityEngine.EventSystems;

public class UISelectionKeeper : MonoBehaviour
{
    private GameObject lastSelected;

    private void Update()
    {
        GameObject current = EventSystem.current.currentSelectedGameObject;

        if (current != null)
        {
            lastSelected = current;
        }
        else if (lastSelected != null)
        {
            EventSystem.current.SetSelectedGameObject(lastSelected);
        }
    }

    private void OnDisable()
    {
        //EventSystem.current.SetSelectedGameObject(null);
    }
}