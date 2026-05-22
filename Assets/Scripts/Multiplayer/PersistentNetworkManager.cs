using UnityEngine;

public class PersistentNetworkManager : MonoBehaviour
{
    private static PersistentNetworkManager _instance;
    
    private void Awake()
    {
        if (_instance)
        {
            Destroy(this);
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
