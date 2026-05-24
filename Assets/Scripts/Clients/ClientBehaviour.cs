using System.Collections;
using Unity.Netcode;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Serialization;

public class ClientBehaviour : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private Vector2 targetPositionOffsetLimits;
    
    private SpriteRenderer sprite;
    
    private Vector2 targetPos;

    private void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();
    }
    
    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
    }

    private void Update()
    {
        if (!IsServer) return;
        
        transform.position = Vector2.MoveTowards(transform.position, targetPos, Time.deltaTime * moveSpeed);
    }

    public void Instantiate(OrderProgress order)
    {
        Debug.Log("Client instantiated with order: " + order.order.orderName);
        order.OnOrderCompleted += HandleOrderCompletedClientRpc;
        order.OnOrderExpired += HandleOrderExpiredClientRpc;

        targetPos = GetCounterTargetPosition();
    }

    public Vector2 GetCounterTargetPosition()
    {
        Counter counter = FindFirstObjectByType<Counter>();
        Vector2 counterSize = counter.GetComponent<BoxCollider2D>().size;
        Vector2 counterPos = new Vector2(
            counter.transform.position.x,
            counter.transform.position.y - counterSize.y / 2 - .1f);
        Vector2 randomOffset = new Vector2(
            Random.Range(-targetPositionOffsetLimits.x, targetPositionOffsetLimits.x),
            Random.Range(-targetPositionOffsetLimits.y, targetPositionOffsetLimits.y));
        
        return counterPos + randomOffset;
    }
    
    [ClientRpc]
    public void HandleOrderCompletedClientRpc()
    {
        Debug.Log("Handle order completed");

        if (sprite == null)
            sprite = GetComponentInChildren<SpriteRenderer>();

        sprite.DOFade(0, 1f).OnComplete(() =>
        {
            if (IsServer && NetworkObject.IsSpawned)
                NetworkObject.Despawn(true);
        });
    }

    [ClientRpc]
    public void HandleOrderExpiredClientRpc()
    {
        if (sprite == null)
            sprite = GetComponentInChildren<SpriteRenderer>();

        sprite.color = Color.red;

        sprite.DOFade(0, 1f).OnComplete(() =>
        {
            if (IsServer && NetworkObject.IsSpawned)
                NetworkObject.Despawn(true);
        });
    }
}