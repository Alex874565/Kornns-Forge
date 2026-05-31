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
    public MovingTile CurrentMovingPlatform { get; private set; }
    public bool ShouldFall { get; private set; }
    
    private bool previousIsGrounded;
    
    private void FixedUpdate()
    {
        if(!IsOwner) return;
        CheckCollisions();
    }
    

    private void CheckCollisions()
    {
        CheckGroundCollision();
        CheckShouldFall();
        CheckHeadCollision();
    }
    
    private void CheckGroundCollision()
    {
        Vector2 boxCastOrigin = new Vector2(feetColl.bounds.center.x, feetColl.bounds.min.y + .2f);
        Vector2 boxCastSize = new Vector2(feetColl.bounds.size.x, .02f);
        RaycastHit2D groundHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.down, collisionStats.GroundCheckDistance, collisionStats.GroundLayer);
        bool nowGrounded = groundHit.collider != null;

        // Detect landing event (transition from not grounded -> grounded)
        if (nowGrounded)
        {
            var movingTile = groundHit.collider.GetComponent<MovingTile>();
            CurrentMovingPlatform = movingTile;
            
            if (!previousIsGrounded)
            {
                var destructible = groundHit.collider.GetComponent<DestructibleTile>();
                if (destructible != null)
                {
                    destructible.RequestDamage(1);
                }
            }
        }
        else
        {
            CurrentMovingPlatform = null;
        }

        previousIsGrounded = nowGrounded;
        IsGrounded = nowGrounded;
    }

    private void CheckShouldFall()
    {
        Vector2 origin = new Vector2(
            feetColl.bounds.center.x,
            feetColl.bounds.min.y
        );

        RaycastHit2D hit = Physics2D.Raycast(
            origin,
            Vector2.down,
            collisionStats.MinGroundDistanceToFall,
            collisionStats.GroundLayer
        );

        ShouldFall = !IsGrounded && hit.collider == null;
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