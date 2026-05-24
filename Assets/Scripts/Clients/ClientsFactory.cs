using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class ClientsFactory : NetworkBehaviour
{
    [SerializeField] private List<GameObject> _clients;

    private OrderManager _orderManager;

    private void Awake()
    {
        _orderManager = FindObjectOfType<OrderManager>();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        _orderManager.OnOrderSpawned += SpawnClient;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer) return;

        _orderManager.OnOrderSpawned -= SpawnClient;
    }

    private void SpawnClient(OrderProgress order)
    {
        GameObject clientObj = Instantiate(
            SelectRandomClient(),
            transform.position,
            Quaternion.identity
        );

        NetworkObject networkObject = clientObj.GetComponent<NetworkObject>();
        networkObject.Spawn();

        clientObj.GetComponent<ClientBehaviour>().Instantiate(order);
    }

    private GameObject SelectRandomClient()
    {
        return _clients[Random.Range(0, _clients.Count)];
    }
}