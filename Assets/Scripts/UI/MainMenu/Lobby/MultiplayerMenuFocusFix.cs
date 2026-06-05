using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class LobbyFocusFix : MonoBehaviour
{
    [SerializeField] private GameObject fallbackButton;

    public void RefreshSelection()
    {
        StartCoroutine(SelectNextFrame());
    }

    private IEnumerator SelectNextFrame()
    {
        yield return null;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(fallbackButton);
    }
}