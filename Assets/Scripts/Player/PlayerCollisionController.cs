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
    private bool previousIsGrounded;
    
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
        Vector2 boxCastOrigin = new Vector2(feetColl.bounds.center.x, feetColl.bounds.min.y + .2f);
        Vector2 boxCastSize = new Vector2(feetColl.bounds.size.x, .02f);
        RaycastHit2D groundHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.down, collisionStats.GroundCheckDistance, collisionStats.GroundLayer);
        bool nowGrounded = groundHit.collider != null;

        // Detect landing event (transition from not grounded -> grounded)
        if (!previousIsGrounded && nowGrounded)
        {
            // If the thing we landed on has a DestructibleTile, request damage
            if (groundHit.collider != null)
            {
                var destructible = groundHit.collider.GetComponent<DestructibleTile>();
                if (destructible != null)
                {
                    destructible.RequestDamage(1);
                }
            }
        }

        previousIsGrounded = nowGrounded;
        IsGrounded = nowGrounded;
    }

    private void CheckHeadCollision()
    {
        Vector2 boxCastOrigin = new Vector2(feetColl.bounds.center.x, bodyColl.bounds.max.y);
        Vector2 boxCastSize = new Vector2(feetColl.bounds.size.x * collisionStats.HeadSize, collisionStats.HeadCheckDistance);
        RaycastHit2D groundHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.up, 0f, collisionStats.GroundLayer);
        BumpedHead = groundHit.collider != null;

        // If we bumped our head into a destructible tile, apply damage from below
        if (BumpedHead && groundHit.collider != null)
        {
            var destructible = groundHit.collider.GetComponent<DestructibleTile>();
            if (destructible != null)
            {
                destructible.RequestDamage(1);
            }
        }
    }
}