using UnityEngine;

public class SelectedCounterVisual : MonoBehaviour
{
    [SerializeField] private ProcessingStationController processingStation;
    [SerializeField] private GameObject visualGameObject;

    private void Awake()
    {
        Hide(); // start hidden
    }

    public void Show()
    {
        visualGameObject.SetActive(true);
    }

    public void Hide()
    {
        visualGameObject.SetActive(false);
    }
}
