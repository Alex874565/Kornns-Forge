using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkUIController : MonoBehaviour
{
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;

    private void Awake()
    {
        hostButton.onClick.AddListener(() => 
            {
                NetworkManager.Singleton.StartHost();
            }
        );

        clientButton.onClick.AddListener(() =>
            {
                NetworkManager.Singleton.StartClient();
            }
        );
    }
}