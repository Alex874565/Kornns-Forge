using UnityEngine;
using Unity.Netcode;
using System;
using UnityEngine.Serialization;

public class PlayerCollisionController : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Collider2D feetColl;
    [SerializeField] private Collider2D bodyColl;
    [SerializeField] private PlayerCollisionStats collisionStats;
    
    public bool IsGrounded { get; private set; }
    public bool BumpedHead { get; private set; }
    
    private void FixedUpdate()
    {
        if(!IsOwner) return;
        CheckCollisions();
    }
    

    private void CheckCollisions()
    {
        CheckGroundCollision();
        CheckHeadCollision();
    }
    
    private void CheckGroundCollision()
    {
        Vector2 boxCastOrigin = new Vector2(feetColl.bounds.center.x, bodyColl.bounds.min.y + .2f);
        Vector2 boxCastSize = new Vector2(feetColl.bounds.size.x, .02f);
        
        RaycastHit2D groundHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.down, collisionStats.GroundCheckDistance, collisionStats.GroundLayer);
        IsGrounded = groundHit.collider != null;
    }

    private void CheckHeadCollision()
    {
        Vector2 boxCastOrigin = new Vector2(feetColl.bounds.center.x, bodyColl.bounds.max.y);
        Vector2 boxCastSize = new Vector2(feetColl.bounds.size.x * collisionStats.HeadSize, collisionStats.HeadCheckDistance);
        
        RaycastHit2D groundHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.up, 0f, collisionStats.GroundLayer);
        BumpedHead = groundHit.collider != null;
    }
}